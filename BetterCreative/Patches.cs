using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  static class Patches
  {
    public static GameObject GetSelectedPrefab(Player player)
    {
      return player?.GetSelectedPiece()?.gameObject;
    }

    static bool settingUpPlacementGhost = false;

    // Detours Player.SetupPlacementGhost
    // Refs: 
    //  - Player.m_buildPieces
    //  - PieceTable.GetSelectedPrefab
    [HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
    class PlayerSetupPlacementGhost
    {
      static void Prefix(Player __instance)
      {
        GameObject selected = GetSelectedPrefab(__instance);

        if (selected != null)
        {
          settingUpPlacementGhost = true;
        }
      }

      static void Postfix()
      {
        settingUpPlacementGhost = false;
      }
    }

    static Piece placingPiece = null;
    static GameObject createdGameObject = null;

    [HarmonyPatch(typeof(Player), "PlacePiece")]
    class OnPlacePiece
    {
      static void Prefix(Piece piece)
      {
        placingPiece = piece;
      }

      static void Postfix(bool __result)
      {
        if (__result && createdGameObject)
        {
          BetterCreative.undoManager.AddItem(new CreateObjectAction(createdGameObject));
        }

        placingPiece = null;
        createdGameObject = null;
      }
    }

    [HarmonyPatch(typeof(Player), "RemovePiece")]
    class OnRemovePiece
    {
      static bool Prefix(Player __instance, ref bool __result, ref Piece __state, ref int ___m_removeRayMask)
      {
        __state = null;
        // Copy of piece finding code, since we cannot easily access it directly from the function
        if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out RaycastHit hit, 50f, ___m_removeRayMask)
          && Vector3.Distance(hit.point, __instance.m_eye.position) < __instance.m_maxPlaceDistance)
        {
          __state = hit.collider.GetComponentInParent<Piece>();
          if (__state == null && hit.collider.GetComponent("Heightmap"))
          {
            __state = TerrainModifier.FindClosestModifierPieceInRange(hit.point, 2.5f);
          }
        }

        if (Configs.UnrestrictedPlacement.Value && __state)
        {
          WearNTear wear = __state.GetComponent<WearNTear>();
          if (wear)
          {
            wear.Remove();
          }
          else
          {
            ZNetView view = __state.GetComponent<ZNetView>();
            view.ClaimOwnership();
            __state.DropResources();
            ZNetScene.instance.Destroy(__state.gameObject);
          }
          __result = true;
          return false;
        }
        return true;
      }

      static void Postfix(bool __result, ref Piece __state)
      {
        if (__result && __state)
        {
          BetterCreative.undoManager.AddItem(new DestroyObjectAction(__state.gameObject));
        }
      }
    }

    // This is to grab the created GameObject from inside of Player.PlacePiece which we otherwise wouldn't have direct access to
    [HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    class ObjectInstantiate3
    {
      static void Postfix(UnityEngine.Object original, Vector3 position, Quaternion rotation, UnityEngine.Object __result)
      {
        if (original == placingPiece?.gameObject && __result is GameObject)
        {
          createdGameObject = __result as GameObject;
        }
      }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), "Internal_CloneSingle", new Type[] { typeof(UnityEngine.Object) })]
    class ObjectInstantiate1
    {
      static bool Prefix(ref UnityEngine.Object __result, UnityEngine.Object data)
      {
        if (settingUpPlacementGhost)
        {
          settingUpPlacementGhost = false;
          var tempParent = new GameObject();
          tempParent.SetActive(false);
          tempParent.AddComponent<Transform>();

          var tempPrefab = UnityEngine.Object.Instantiate(data, tempParent.transform, false);
          Prefabs.PrepareGhostPrefab(tempPrefab as GameObject);

          __result = UnityEngine.Object.Instantiate(tempPrefab);

          UnityEngine.Object.DestroyImmediate(tempParent);
          return false;
        }
        return true;
      }
    }

    // Detours Player.UpdatePlacementGhost
    // Refs:
    //  - Player.m_placementStatus
    //  - Player.PlacementStatus
    //  - Player.SetPlacementGhostValid
    //  - Player.m_placementGhost
    public static GameObject lastPlacementGhost = null;

    [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
    class PlayerUpdatePlacementGhost
    {
      static void Postfix(Player __instance, GameObject ___m_placementGhost, ref object ___m_placementStatus)
      {
        lastPlacementGhost = ___m_placementGhost;
        if (Configs.UnrestrictedPlacement.Value && ___m_placementGhost)
        {
          Type player_t = typeof(Player);
          Type placementStatus_t = player_t.GetNestedType("PlacementStatus", BindingFlags.NonPublic);

          ___m_placementStatus = Enum.Parse(placementStatus_t, "0");
          ___m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(false);
        }
      }
    }

    // Detours Piece.DropResources
    [HarmonyPatch(typeof(Piece), "DropResources")]
    class DropPieceResources
    {
      static bool Prefix()
      {
        // Run original if not enabled
        return !Configs.NoPieceDrops.Value;
      }
    }

    // Detours ZNetScene.Awake
    public static Dictionary<ZDO, ZNetView> znetSceneInstances;
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    class ZNetSceneAwake
    {
      static void Prefix(ZNetScene __instance, Dictionary<ZDO, ZNetView> ___m_instances)
      {
        znetSceneInstances = ___m_instances;
        BetterCreative.ModifyItems();

        Prefabs.ModifyExistingPieces(__instance);
        if (Configs.AllPrefabs.Value)
          Prefabs.RegisterPrefabs(__instance);
      }
    }

    // Detours Player.HaveStamina
    [HarmonyPatch(typeof(Player), "HaveStamina")]
    class HaveStamina
    {
      static bool Prefix(ref bool __result)
      {
        if (Configs.UnlimitedStamina.Value)
        {
          __result = true;
          return false;
        }
        return true;
      }
    }

    // Detours Player.SetLocalPlayer
    // Refs:
    //  - Console.TryRunCommand
    //  - Player.SetGodMode
    //  - Player.SetGhostMode
    //  - Player.ToggleNoPlacementCost
    //  - Player.m_placeDelay
    [HarmonyPatch(typeof(Player), "SetLocalPlayer")]
    class SetLocalPlayer
    {
      static void Postfix(Player __instance)
      {
        if (Configs.DevCommands.Value)
        {
          Console.instance.TryRunCommand("devcommands", silentFail: true, skipAllowedCheck: true);
          if (Configs.DebugMode.Value)
            Console.instance.TryRunCommand("debugmode", silentFail: true, skipAllowedCheck: true);

          if (Configs.God.Value)
            __instance.SetGodMode(true);

          if (Configs.Ghost.Value)
            __instance.SetGhostMode(true);

          if (Configs.NoCost.Value)
            __instance.ToggleNoPlacementCost();

          if (Configs.NoPieceDelay.Value)
            __instance.m_placeDelay = 0;
        }
      }
    }

    [HarmonyPatch(typeof(ZNetView), "Awake")]
    class ZNetViewAwake
    {
      static bool Prefix(ZNetView __instance)
      {
        if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null)
        {
          Jotunn.Logger.LogWarning($"Double ZNetview when initializing object {__instance.name}; OVERRIDE: Deleting duplicate");
          UnityEngine.Object.Destroy(__instance);
          return false;
        }
        return true;
      }
    }
  }
}

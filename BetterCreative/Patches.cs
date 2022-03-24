using HarmonyLib;
using Jotunn.Managers;
using System;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  static class Patches
  {
    private static GameObject ghostOverridePiece = null;

    // Detours PieceTable.GetSelectedPrefab
    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPrefab))]
    class PieceTableGetSelectedPrefab
    {
      static bool Prefix(ref GameObject __result)
      {
        if (ghostOverridePiece != null)
        {
          __result = ghostOverridePiece;
          return false;
        }
        return true;
      }
    }

    // Detours Player.SetupPlacementGhost
    // Refs: 
    //  - Player.m_buildPieces
    //  - PieceTable.GetSelectedPrefab
    [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
    class PlayerSetupPlacementGhost
    {
      static void Prefix(Player __instance)
      {
        GameObject selected = __instance.m_buildPieces?.GetSelectedPrefab();

        if (selected != null)
        {
          GameObject ghost = PrefabManager.Instance.GetPrefab(selected.name + "_ghostfab");
          if (ghost != null)
          {
            ghostOverridePiece = ghost;
          }
        }
      }

      static void Postfix()
      {
        ghostOverridePiece = null;
      }
    }

    static Piece placingPiece = null;
    static GameObject createdGameObject = null;

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    class OnPlacePiece
    {
      static void Prefix(Piece piece)
      {
        placingPiece = piece;
      }

      static bool Postfix(bool result)
      {
        if (result && createdGameObject)
        {
          BetterCreative.undoManager.AddItem(new CreateObjectAction(createdGameObject));
        }

        placingPiece = null;
        createdGameObject = null;
        return result;
      }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
    class OnRemovePiece
    {
      static void Prefix(Player __instance, ref Piece __state)
      {
        __state = null;
        // Copy of piece finding code, since we cannot easily access it directly from the function
        if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out RaycastHit hit, 50f, __instance.m_removeRayMask)
          && Vector3.Distance(hit.point, __instance.m_eye.position) < __instance.m_maxPlaceDistance)
        {
          __state = hit.collider.GetComponentInParent<Piece>();
          if (__state == null && hit.collider.GetComponent("Heightmap"))
          {
            __state = TerrainModifier.FindClosestModifierPieceInRange(hit.point, 2.5f);
          }
        }
      }

      static bool Postfix(bool result, ref Piece __state)
      {
        if (result && __state)
        {
          BetterCreative.undoManager.AddItem(new DestroyObjectAction(__state.gameObject));
        }
        return result;
      }
    }

    // This is to grab the created GameObject from inside of Player.PlacePiece which we otherwise wouldn't have direct access to
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    class ObjectInstantiate
    {
      static void Postfix(UnityEngine.Object original, Vector3 position, Quaternion rotation, UnityEngine.Object __result)
      {
        if (original == placingPiece?.gameObject && __result is GameObject)
        {
          createdGameObject = __result as GameObject;
        }
      }
    }

    // Detours Player.UpdatePlacementGhost
    // Refs:
    //  - Player.m_placementStatus
    //  - Player.PlacementStatus
    //  - Player.SetPlacementGhostValid
    //  - Player.m_placementGhost
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    class PlayerUpdatePlacementGhost
    {
      static void Postfix(Player __instance)
      {
        if (Configs.UnrestrictedPlacement.Value && __instance.m_placementGhost)
        {
          __instance.m_placementStatus = Player.PlacementStatus.Valid;
          __instance.SetPlacementGhostValid(true);
        }
      }
    }

    // Detours Piece.DropResources
    [HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
    class DropPieceResources
    {
      static bool Prefix()
      {
        // Run original if not enabled
        return !Configs.NoPieceDrops.Value;
      }
    }

    // Detours ZNetScene.Awake
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    class ZNetSceneAwake
    {
      static void Prefix(ZNetScene __instance)
      {
        BetterCreative.ModifyItems();

        Prefabs.ModifyExistingPieces(__instance);
        if (Configs.AllPrefabs.Value)
          Prefabs.RegisterPrefabs(__instance);
      }
    }

    // Detours Player.HaveStamina
    [HarmonyPatch(typeof(Player), nameof(Player.HaveStamina))]
    class HaveStamina
    {
      static bool Prefix()
      {
        // Run original if not enabled
        return !Configs.UnlimitedStamina.Value;
      }
    }

    // Detours Player.SetLocalPlayer
    // Refs:
    //  - Console.TryRunCommand
    //  - Player.SetGodMode
    //  - Player.SetGhostMode
    //  - Player.ToggleNoPlacementCost
    //  - Player.m_placeDelay
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
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
  }
}

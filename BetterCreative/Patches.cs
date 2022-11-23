using HarmonyLib;
using Jotunn.Managers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Heinermann.BetterCreative
{
  [HarmonyPatch]
  static class Patches
  {
    public static GameObject GetSelectedPrefab(Player player)
    {
      return player?.GetSelectedPiece()?.gameObject;
    }

    static bool settingUpPlacementGhost = false;

    // TODO: Redo this patch, use PieceTable.GetSelectedPrefab
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

    // Called when piece is just placed
    [HarmonyPatch(typeof(Piece), "SetCreator"), HarmonyPrefix]
    static void PieceSetCreatorPrefix(long uid, Piece __instance)
    {
      UndoHelper.Create(__instance.gameObject);

      var container = __instance.GetComponent<Container>();
      if (container && (container.m_autoDestroyEmpty || __instance.GetComponent("TombStone")))
      {
        container.GetInventory().AddItem(PrefabManager.Instance.GetPrefab("Wood"), 1);
      }
    }

    // TODO simplify remove patches
    static bool inRemovePiece = false;
    [HarmonyPatch(typeof(Player), "RemovePiece")]
    class OnRemovePiece
    {
      static bool Prefix(Player __instance, ref bool __result, ref int ___m_removeRayMask)
      {
        inRemovePiece = true;

        Piece piece = null;
        // Copy of piece finding code, since we cannot easily access it directly from the function
        if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out RaycastHit hit, 50f, ___m_removeRayMask)
          && Vector3.Distance(hit.point, __instance.m_eye.position) < __instance.m_maxPlaceDistance)
        {
          piece = hit.collider.GetComponentInParent<Piece>();
          if (piece == null && hit.collider.GetComponent("Heightmap"))
          {
            piece = TerrainModifier.FindClosestModifierPieceInRange(hit.point, 2.5f);
          }
        }

        if (Configs.UnrestrictedPlacement.Value && piece)
        {
          WearNTear wear = piece.GetComponent<WearNTear>();
          if (wear)
          {
            wear.Remove();
          }
          else
          {
            ZNetView view = piece.GetComponent<ZNetView>();
            view.ClaimOwnership();
            piece.DropResources();
            ZNetScene.instance.Destroy(piece.gameObject);
          }
          __result = true;
          return false;
        }
        return true;
      }

      static void Postfix()
      {
        inRemovePiece = false;
      }
    }

    [HarmonyPatch(typeof(Player), "IsEncumbered"), HarmonyPrefix]
    static bool PlayerIsEncumberedPrefix(ref bool __result)
    {
      if (Configs.NoEncumbered.Value)
      {
        __result = false;
        return false;
      }
      return true;
    }

    [HarmonyPatch(typeof(UnityEngine.Object), "Internal_CloneSingle", new Type[] { typeof(UnityEngine.Object) }), HarmonyPrefix]
    static bool ObjectInstantiate1Prefix(ref UnityEngine.Object __result, UnityEngine.Object data)
    {
      if (settingUpPlacementGhost)
      {
        settingUpPlacementGhost = false;
        if (Prefabs.AddedPrefabs.Contains(data.name))
        {
          Jotunn.Logger.LogInfo($"Setting up placement ghost for {data.name}");

          var staging = new GameObject();
          staging.SetActive(false);

          var ghostPrefab = UnityEngine.Object.Instantiate(data, staging.transform, false);
          Prefabs.PrepareGhostPrefab(ghostPrefab as GameObject);

          __result = UnityEngine.Object.Instantiate(ghostPrefab);

          UnityEngine.Object.DestroyImmediate(staging);
          return false;
        }
      }
      return true;
    }

    // Detours Player.UpdatePlacementGhost
    // Refs:
    //  - Player.m_placementStatus
    //  - Player.PlacementStatus
    //  - Player.SetPlacementGhostValid
    //  - Player.m_placementGhost
    public static GameObject lastPlacementGhost = null;

    [HarmonyPatch(typeof(Player), "UpdatePlacementGhost"), HarmonyPostfix]
    static void PlayerUpdatePlacementGhostPostfix(ref GameObject ___m_placementGhost, ref int ___m_placementStatus)
    {
      lastPlacementGhost = ___m_placementGhost;
      if (Configs.UnrestrictedPlacement.Value && ___m_placementGhost)
      {
        ___m_placementStatus = 0;
        ___m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(false);
      }
    }

    // Detours Piece.DropResources
    [HarmonyPatch(typeof(Piece), "DropResources"), HarmonyPrefix]
    static bool DropPieceResourcesPrefix()
    {
      // Run original if not enabled
      return !Configs.NoPieceDrops.Value;
    }

    [HarmonyPatch(typeof(ZNetScene), "Destroy"), HarmonyPrefix]
    static void ZNetSceneDestroyPrefix(GameObject go)
    {
      if (!inRemovePiece) return;
      UndoHelper.Remove(go);
    }

    // Hook just before Jotunn registers the Pieces
    // TODO: Rework Jotunn to be able to add pieces later
    [HarmonyPatch(typeof(ObjectDB), "Awake"), HarmonyPrefix]
    static void ObjectDBAwakePrefix()
    {
      if (SceneManager.GetActiveScene().name == "main")
      {
        BetterCreative.ModifyItems();

        if (Configs.AllPrefabs.Value)
          Prefabs.FindAndRegisterPrefabs();
      }
    }

    // Detours Player.HaveStamina
    [HarmonyPatch(typeof(Player), "HaveStamina"), HarmonyPrefix]
    static bool HaveStaminaPrefix(ref bool __result)
    {
      if (Configs.UnlimitedStamina.Value)
      {
        __result = true;
        return false;
      }
      return true;
    }

    [HarmonyPatch(typeof(Player), "ShowTutorial"), HarmonyPrefix]
    static bool ShowTutorialPrefix(string name, bool force)
    {
      return false;
    }

    [HarmonyPatch(typeof(MessageHud), "QueueUnlockMsg"), HarmonyPrefix]
    static bool MessageHudQueueUnlockMessagePrefix(Sprite icon, string topic, string description)
    {
      return !Configs.NoUnlockMsg.Value;
    }

    // Detours Player.SetLocalPlayer
    // Refs:
    //  - Console.TryRunCommand
    //  - Player.SetGodMode
    //  - Player.SetGhostMode
    //  - Player.ToggleNoPlacementCost
    //  - Player.m_placeDelay
    [HarmonyPatch(typeof(Player), "SetLocalPlayer"), HarmonyPostfix]
    static void SetLocalPlayerPostfix(Player __instance)
    {
      if (Configs.DevCommands.Value && __instance)
      {
        Console.instance.TryRunCommand("devcommands", silentFail: true, skipAllowedCheck: true);
        if (Configs.DebugMode.Value)
          Player.m_debugMode = true;

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

    [HarmonyPatch(typeof(ZNetView), "Awake"), HarmonyPrefix]
    static bool ZNetViewAwakePrefix(ZNetView __instance)
    {
      if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null)
      {
        Jotunn.Logger.LogWarning($"Double ZNetview when initializing object {__instance.name}; OVERRIDE: Deleting the {__instance.name} gameobject");
        ZNetScene.instance.Destroy(__instance.gameObject);
        return false;
      }
      return true;
    }
  }
}

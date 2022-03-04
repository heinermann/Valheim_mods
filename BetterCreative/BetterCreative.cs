using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.None)]
  internal class BetterCreative : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.bettercreative";
    public const string PluginName = "BetterCreative";
    public const string PluginVersion = "1.1.1";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private UndoManager undoManager = new UndoManager();

    private void Awake()
    {
      Configs.Init(Config);
      Console.SetConsoleEnabled(true);

      On.ZNetScene.Awake += ZNetSceneAwake;

      On.Player.HaveStamina += HaveStamina;
      On.Player.SetLocalPlayer += SetLocalPlayer;
      On.Player.PlacePiece += OnPlacePiece;
      On.Player.RemovePiece += OnRemovePiece;

      On.Piece.DropResources += DropPieceResources;

      On.Player.SetupPlacementGhost += PlayerSetupPlacementGhost;
      On.Player.UpdatePlacementGhost += PlayerUpdatePlacementGhost;
      On.PieceTable.GetSelectedPrefab += PieceTableGetSelectedPrefab;

      harmony.PatchAll();
    }

    public static void ShowHUDMessage(string msg)
    {
      Jotunn.Logger.LogInfo(msg);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, msg);
    }

    private void KeyDeletePressed()
    {
      GameObject selected = Player.m_localPlayer?.m_buildPieces?.GetSelectedPrefab();
      if (selected == null) return;

      GameObject ghost = Player.m_localPlayer?.m_placementGhost;
      if (ghost == null) return;

      string matchPattern = "^" + Regex.Escape(selected.name) + "(\\(Clone\\))?$";
      float sqrRadius = Configs.DeleteRange.Value * Configs.DeleteRange.Value;

      List<GameObject> toDelete = ZNetScene.instance.m_instances.Values
        .Where(inst => Regex.IsMatch(inst.name, matchPattern))
        .Where(inst => (inst.transform.position - ghost.transform.position).sqrMagnitude <= sqrRadius)
        .Select(inst => inst.gameObject)
        .ToList();

      undoManager.AddItem(new DeleteObjectsAction(toDelete));
      toDelete.ForEach(ZNetScene.instance.Destroy);
      ShowHUDMessage($"Deleted {toDelete.Count} Objects");
    }

    private void Update()
    {
      if (!ZNetScene.instance || ZNetScene.instance.InLoadingScreen()) return;

      if (Configs.KeyUndo1.Value.IsDown() || Configs.KeyUndo2.Value.IsDown())
      {
        undoManager.Undo();
      }
      else if (Configs.KeyRedo1.Value.IsDown() || Configs.KeyRedo2.Value.IsDown())
      {
        undoManager.Redo();
      }
      else if (Configs.KeyDelete1.Value.IsDown() || Configs.KeyDelete2.Value.IsDown())
      {
        KeyDeletePressed();
      }
    }

    private static GameObject ghostOverridePiece = null;

    // Detours PieceTable.GetSelectedPrefab
    private GameObject PieceTableGetSelectedPrefab(On.PieceTable.orig_GetSelectedPrefab orig, PieceTable self)
    {
      if (ghostOverridePiece != null)
        return ghostOverridePiece;
      return orig(self);
    }

    // Detours Player.SetupPlacementGhost
    // Refs: 
    //  - Player.m_buildPieces
    //  - PieceTable.GetSelectedPrefab
    private void PlayerSetupPlacementGhost(On.Player.orig_SetupPlacementGhost orig, Player self)
    {
      GameObject selected = self.m_buildPieces?.GetSelectedPrefab();
      if (selected == null)
      {
        orig(self);
        return;
      }

      GameObject ghost = PrefabManager.Instance.GetPrefab(selected.name + "_ghostfab");

      if (ghost == null)
      {
        orig(self);
        return;
      }

      ghostOverridePiece = ghost;
      orig(self);
      ghostOverridePiece = null;
    }

    // This is an abridged version of Player.PlacePiece
    public static GameObject PlacePiece(GameObject prefab, Vector3 position, Quaternion rotation)
    {
      TerrainModifier.SetTriggerOnPlaced(trigger: true);
      GameObject result = UnityEngine.Object.Instantiate(prefab, position, rotation);
      TerrainModifier.SetTriggerOnPlaced(trigger: false);

      CraftingStation crafter = result.GetComponentInChildren<CraftingStation>();
      if (crafter)
      {
        Player.m_localPlayer.AddKnownStation(crafter);
      }
      result.GetComponent<Piece>()?.SetCreator(Player.m_localPlayer.GetPlayerID());
      result.GetComponent<PrivateArea>()?.Setup(Game.instance.GetPlayerProfile().GetName());
      result.GetComponent<WearNTear>()?.OnPlaced();
      return result;
    }

    static Piece placingPiece = null;
    static GameObject createdGameObject = null;
    private bool OnPlacePiece(On.Player.orig_PlacePiece orig, Player self, Piece piece)
    {
      placingPiece = piece;
      bool result = orig(self, piece);

      if (result && createdGameObject)
      {
        undoManager.AddItem(new CreateObjectAction(createdGameObject));
      }

      placingPiece = null;
      createdGameObject = null;
      return result;
    }

    private bool OnRemovePiece(On.Player.orig_RemovePiece orig, Player self)
    {
      Piece piece = null;
      // Copy of piece finding code, since we cannot easily access it directly from the function
      if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out RaycastHit hit, 50f, self.m_removeRayMask) && Vector3.Distance(hit.point, self.m_eye.position) < self.m_maxPlaceDistance)
      {
        piece = hit.collider.GetComponentInParent<Piece>();
        if (piece == null && hit.collider.GetComponent("Heightmap"))
        {
          piece = TerrainModifier.FindClosestModifierPieceInRange(hit.point, 2.5f);
        }
      }

      bool result = orig(self);
      if (result && piece)
      {
        undoManager.AddItem(new DestroyObjectAction(piece.gameObject));
      }
      return result;
    }

    // This is to grab the created GameObject from inside of Player.PlacePiece which we otherwise wouldn't have direct access to
    [HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
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
    private void PlayerUpdatePlacementGhost(On.Player.orig_UpdatePlacementGhost orig, Player self, bool flashGuardStone)
    {
      orig(self, flashGuardStone);

      if (Configs.UnrestrictedPlacement.Value && self.m_placementGhost)
      {
        self.m_placementStatus = Player.PlacementStatus.Valid;
        self.SetPlacementGhostValid(true);
      }
    }

    // Detours Piece.DropResources
    private void DropPieceResources(On.Piece.orig_DropResources orig, Piece self)
    {
      if (!Configs.NoPieceDrops.Value)
        orig(self);
    }

    // Refs:
    //  - ObjectDB.m_items
    //  - ItemDrop.m_itemData.m_shared.m_useDurabilityDrain
    private void ModifyItems()
    {
      if (!Configs.NoDurabilityDrain.Value) return;

      foreach (GameObject prefab in ObjectDB.instance.m_items)
      {
        var item = prefab.GetComponent<ItemDrop>();
        if (item == null) continue;
        item.m_itemData.m_shared.m_useDurabilityDrain = 0;
      }
    }

    // Detours ZNetScene.Awake
    private void ZNetSceneAwake(On.ZNetScene.orig_Awake orig, ZNetScene self)
    {
      ModifyItems();

      Prefabs.ModifyExistingPieces(self);
      if (Configs.AllPrefabs.Value)
        Prefabs.RegisterPrefabs(self);

      orig(self);
    }

    // Detours Player.HaveStamina
    private bool HaveStamina(On.Player.orig_HaveStamina orig, Player self, float amount)
    {
      if (Configs.UnlimitedStamina.Value)
        return true;
      return orig(self, amount);
    }

    // Detours Player.SetLocalPlayer
    // Refs:
    //  - Console.TryRunCommand
    //  - Player.SetGodMode
    //  - Player.SetGhostMode
    //  - Player.ToggleNoPlacementCost
    //  - Player.m_placeDelay
    private void SetLocalPlayer(On.Player.orig_SetLocalPlayer orig, Player self)
    {
      orig(self);
      if (Configs.DevCommands.Value)
      {
        Console.instance.TryRunCommand("devcommands", silentFail: true, skipAllowedCheck: true);
        if (Configs.DebugMode.Value)
          Console.instance.TryRunCommand("debugmode", silentFail: true, skipAllowedCheck: true);

        if (Configs.God.Value)
          self.SetGodMode(true);

        if (Configs.Ghost.Value)
          self.SetGhostMode(true);

        if (Configs.NoCost.Value)
          self.ToggleNoPlacementCost();

        if (Configs.NoPieceDelay.Value)
          self.m_placeDelay = 0;
      }
    }
  }
}
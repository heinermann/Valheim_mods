using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections;
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
    public const string PluginVersion = "1.0.3";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Configs.Init(Config);
      Console.SetConsoleEnabled(true);

      On.ZNetScene.Awake += ZNetSceneAwake;

      On.Player.HaveStamina += HaveStamina;
      On.Player.SetLocalPlayer += SetLocalPlayer;
      On.Player.PlacePiece += OnPlacePiece;

      On.Piece.DropResources += DropPieceResources;

      On.Player.SetupPlacementGhost += PlayerSetupPlacementGhost;
      On.Player.UpdatePlacementGhost += PlayerUpdatePlacementGhost;
      On.PieceTable.GetSelectedPrefab += PieceTableGetSelectedPrefab;

      harmony.PatchAll();
    }

    private bool IsCtrlPressed()
    {
      return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    private bool IsShiftPressed()
    {
      return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void Undo()
    {
      Jotunn.Logger.LogInfo($"Undo {undoPosition}/{undoBuffer.Count}");
      if (undoPosition > 0)
      {
        undoPosition--;

        UndoEntry undo = (undoBuffer[undoPosition] as UndoEntry?).Value;
        GameObject.Destroy(undo.createdEntity);

        undo.createdEntity = null;
        undoBuffer[undoPosition] = undo;
      }
    }

    private void Redo()
    {
      Jotunn.Logger.LogInfo($"Redo {undoPosition}/{undoBuffer.Count}");
      if (undoPosition < undoBuffer.Count)
      {
        UndoEntry undo = (undoBuffer[undoPosition] as UndoEntry?).Value;

        if (undo.player.m_placementGhost)
        {
          invokingRedo = true;
          incomingUndoData = undo;

          undo.player.m_placementGhost.transform.position = undo.position;
          undo.player.m_placementGhost.transform.rotation = undo.orientation;
          undo.player.PlacePiece(undo.piece);

          undoBuffer[undoPosition] = incomingUndoData;
          incomingUndoData = default(UndoEntry);

          invokingRedo = false;

          undoPosition++;
        }
      }
    }

    private void Update()
    {
      if (Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.KeypadPlus))
      {
        // TODO Increase monster level
      }
      else if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus))
      {
        // TODO Decrease monster level
      }
      else if (IsCtrlPressed() && !IsShiftPressed() && Input.GetKeyUp(KeyCode.Z))
      {
        Undo();
      }
      else if ((IsCtrlPressed() && Input.GetKeyUp(KeyCode.Y)) || (IsCtrlPressed() && IsShiftPressed() && Input.GetKeyUp(KeyCode.Z)))
      {
        Redo();
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
      if (self.m_buildPieces?.GetSelectedPrefab() == null)
      {
        orig(self);
        return;
      }

      GameObject ghost = PrefabManager.Instance.GetPrefab(self.m_buildPieces.GetSelectedPrefab().name + "_ghostfab");

      if (ghost == null)
      {
        orig(self);
        return;
      }

      ghostOverridePiece = ghost;
      orig(self);
      ghostOverridePiece = null;
    }

    static UndoEntry incomingUndoData = default(UndoEntry);
    private bool OnPlacePiece(On.Player.orig_PlacePiece orig, Player self, Piece piece)
    {
      incomingUndoData.player = self;
      incomingUndoData.piece = piece;

      bool result = orig(self, piece);

      if (!invokingRedo)
      {
        if (result)
          AddUndoItem(incomingUndoData);

        incomingUndoData = default(UndoEntry);
      }
      return result;
    }

    struct UndoEntry
    {
      public Player player;
      public Piece piece;
      public Vector3 position;
      public Quaternion orientation;
      public GameObject createdEntity;
    }

    static int undoPosition = 0;
    static ArrayList undoBuffer = new ArrayList();
    static bool invokingRedo = false;

    private static void ClearUndoBuffer()
    {
      undoBuffer.RemoveRange(undoPosition, undoBuffer.Count - undoPosition);
    }

    private static void AddUndoItem(UndoEntry item)
    {
      ClearUndoBuffer();
      undoBuffer.Add(item);
      undoPosition++;
    }

    [HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    class ObjectInstantiate
    {
      static void Postfix(UnityEngine.Object original, Vector3 position, Quaternion rotation, UnityEngine.Object __result)
      {
        if (incomingUndoData.piece != null && incomingUndoData.piece.gameObject == original && __result is GameObject)
        {
          incomingUndoData.createdEntity = __result as GameObject;
          incomingUndoData.position = incomingUndoData.createdEntity.transform.position;
          incomingUndoData.orientation = incomingUndoData.createdEntity.transform.rotation;
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
      if (!invokingRedo)
      {
        orig(self, flashGuardStone);
      }

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
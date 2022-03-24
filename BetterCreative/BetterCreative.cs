using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.ServerMustHaveMod, VersionStrictness.None)]
  internal class BetterCreative : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.bettercreative";
    public const string PluginName = "BetterCreative";
    public const string PluginVersion = "1.1.1";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    public static UndoManager undoManager = new UndoManager();

    private void Awake()
    {
      Configs.Init(Config);
      Console.SetConsoleEnabled(true);
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

    // Refs:
    //  - ObjectDB.m_items
    //  - ItemDrop.m_itemData.m_shared.m_useDurabilityDrain
    public static void ModifyItems()
    {
      if (!Configs.NoDurabilityDrain.Value) return;

      foreach (GameObject prefab in ObjectDB.instance.m_items)
      {
        var item = prefab.GetComponent<ItemDrop>();
        if (item == null) continue;
        item.m_itemData.m_shared.m_useDurabilityDrain = 0;
      }
    }

  }
}
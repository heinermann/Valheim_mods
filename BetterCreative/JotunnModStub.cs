// BetterCreative
// a Valheim mod skeleton using Jötunn
// 
// File:    BetterCreative.cs
// Project: BetterCreative

using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
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
    public const string PluginVersion = "1.0.0";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    public static ConfigEntry<bool> UnlimitedStamina;
    public static ConfigEntry<bool> Ghost;
    public static ConfigEntry<bool> God;
    public static ConfigEntry<bool> NoCost;
    public static ConfigEntry<bool> DebugMode;
    public static ConfigEntry<bool> AllPrefabs;
    public static ConfigEntry<bool> UnrestrictedPlacement;
    public static ConfigEntry<bool> NoPieceDrops;
    public static ConfigEntry<bool> NoPieceDelay;
    public static ConfigEntry<bool> NoDurabilityDrain;

    private void InitConfig()
    {
      NoCost = Config.Bind("Command States", "nocost", true, "No build cost, unlocks everything.");
      God = Config.Bind("Command States", "god", true, "Makes it so you don't take damage from monsters.");
      Ghost = Config.Bind("Command States", "ghost", true, "Prevents mobs from seeing you.");
      DebugMode = Config.Bind("Command States", "debugmode", false, "Enables fly mode and debug hotkeys.");

      UnlimitedStamina = Config.Bind("Improvements", "Unlimited Stamina", true, "Can always perform stamina actions regardless of stamina amount.");
      AllPrefabs = Config.Bind("Improvements", "All Prefabs", true, "Allow placement of all functional prefabs.");
      UnrestrictedPlacement = Config.Bind("Improvements", "Unrestricted Placement", false, "Allow unrestricted placements (no collision, campfire on wood, etc).");
      NoPieceDrops = Config.Bind("Improvements", "No Piece Drops", true, "Don't drop materials when pieces are destroyed.");
      NoPieceDelay = Config.Bind("Improvements", "No Placement Delay", true, "No cooldowns for the hammer, cultivator, or hoe.");
      NoDurabilityDrain = Config.Bind("Improvements", "No Durability Drain", true, "Tools don't lose durability.");
    }

    private void Awake()
    {
      InitConfig();
      Console.SetConsoleEnabled(true);

      On.ZNetScene.Awake += ZNetSceneAwake;
      On.Player.HaveStamina += HaveStamina;
      On.Player.SetLocalPlayer += SetLocalPlayer;
      On.Piece.DropResources += DropPieceResources;

      On.Player.SetupPlacementGhost += PlayerSetupPlacementGhost;
      On.PieceTable.GetSelectedPrefab += PieceTableGetSelectedPrefab;
    }

    private static GameObject ghostOverridePiece = null;
    private GameObject PieceTableGetSelectedPrefab(On.PieceTable.orig_GetSelectedPrefab orig, PieceTable self)
    {
      if (ghostOverridePiece != null)
        return ghostOverridePiece;
      return orig(self);
    }

    void PlayerSetupPlacementGhost(On.Player.orig_SetupPlacementGhost orig, Player self)
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

    private void DropPieceResources(On.Piece.orig_DropResources orig, Piece self)
    {
      if (!NoPieceDrops.Value)
        orig(self);
    }

    private void ModifyItems()
    {
      if (!NoDurabilityDrain.Value) return;

      foreach (GameObject prefab in ObjectDB.instance.m_items)
      {
        var item = prefab.GetComponent<ItemDrop>();
        if (item == null) continue;
        item.m_itemData.m_shared.m_useDurabilityDrain = 0;
      }
    }

    private void ZNetSceneAwake(On.ZNetScene.orig_Awake orig, ZNetScene self)
    {
      ModifyItems();

      Prefabs.ModifyExistingPieces(self);
      if (AllPrefabs.Value)
        Prefabs.RegisterPrefabs(self);

      orig(self);
    }

    private bool HaveStamina(On.Player.orig_HaveStamina orig, Player self, float amount)
    {
      if (UnlimitedStamina.Value)
        return true;
      return orig(self, amount);
    }

    private void SetLocalPlayer(On.Player.orig_SetLocalPlayer orig, Player self)
    {
      orig(self);

      Console.instance.TryRunCommand("devcommands", silentFail: true, skipAllowedCheck: true);
      if (DebugMode.Value)
        Console.instance.TryRunCommand("debugmode", silentFail: true, skipAllowedCheck: true);

      if (God.Value)
        self.SetGodMode(true);

      if (Ghost.Value)
        self.SetGhostMode(true);

      if (NoCost.Value)
        self.ToggleNoPlacementCost();

      if (NoPieceDelay.Value)
        self.m_placeDelay = 0;
    }
  }
}
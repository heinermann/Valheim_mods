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
    public const string PluginVersion = "1.0.2";

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
      DebugMode = Config.Bind("Command States", "debugmode", true, "Enables fly mode and debug hotkeys.");

      UnlimitedStamina = Config.Bind("Improvements", "Unlimited Stamina", true, "Can always perform stamina actions regardless of stamina amount.");
      AllPrefabs = Config.Bind("Improvements", "All Prefabs", true, "Allow placement of all functional prefabs. (Requires restart to take effect)");
      UnrestrictedPlacement = Config.Bind("Improvements", "Unrestricted Placement", true, "Allow unrestricted placements (no collision, campfire on wood, etc). Note: Disabling this won't allow placement of some objects. (Requires restart to take effect)");
      NoPieceDrops = Config.Bind("Improvements", "No Piece Drops", true, "Don't drop materials when pieces are destroyed.");
      NoPieceDelay = Config.Bind("Improvements", "No Placement Delay", true, "No cooldowns for the hammer, cultivator, or hoe. (Requires restart to take effect)");
      NoDurabilityDrain = Config.Bind("Improvements", "No Durability Drain", true, "Tools don't lose durability. (Requires restart to take effect)");
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
      On.Player.UpdatePlacementGhost += PlayerUpdatePlacementGhost;
      On.PieceTable.GetSelectedPrefab += PieceTableGetSelectedPrefab;
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

    // Detours Player.UpdatePlacementGhost
    // Refs:
    //  - Player.m_placementStatus
    //  - Player.PlacementStatus
    //  - Player.SetPlacementGhostValid
    //  - Player.m_placementGhost
    private void PlayerUpdatePlacementGhost(On.Player.orig_UpdatePlacementGhost orig, Player self, bool flashGuardStone)
    {
      orig(self, flashGuardStone);
      if (UnrestrictedPlacement.Value && self.m_placementGhost)
      {
        self.m_placementStatus = Player.PlacementStatus.Valid;
        self.SetPlacementGhostValid(true);
      }
    }

    // Detours Piece.DropResources
    private void DropPieceResources(On.Piece.orig_DropResources orig, Piece self)
    {
      if (!NoPieceDrops.Value)
        orig(self);
    }

    // Refs:
    //  - ObjectDB.m_items
    //  - ItemDrop.m_itemData.m_shared.m_useDurabilityDrain
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

    // Detours ZNetScene.Awake
    private void ZNetSceneAwake(On.ZNetScene.orig_Awake orig, ZNetScene self)
    {
      ModifyItems();

      Prefabs.ModifyExistingPieces(self);
      if (AllPrefabs.Value)
        Prefabs.RegisterPrefabs(self);

      orig(self);
    }

    // Detours Player.HaveStamina
    private bool HaveStamina(On.Player.orig_HaveStamina orig, Player self, float amount)
    {
      if (UnlimitedStamina.Value)
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
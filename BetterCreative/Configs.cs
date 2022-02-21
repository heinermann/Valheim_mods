using BepInEx.Configuration;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  public static class Configs
  {
    // Command States
    const string CommandStates = "Command States";
    public static ConfigEntry<bool> DevCommands;
    public static ConfigEntry<bool> NoCost;
    public static ConfigEntry<bool> God;
    public static ConfigEntry<bool> Ghost;
    public static ConfigEntry<bool> DebugMode;

    // Improvements
    const string Improvements = "Improvements";
    public static ConfigEntry<bool> UnlimitedStamina;
    public static ConfigEntry<bool> AllPrefabs;
    public static ConfigEntry<bool> UnrestrictedPlacement;
    public static ConfigEntry<bool> NoPieceDrops;
    public static ConfigEntry<bool> NoPieceDelay;
    public static ConfigEntry<bool> NoDurabilityDrain;
    public static ConfigEntry<int> DeleteRange;

    // Keybinds
    const string Hotkeys = "Hotkeys";
    public static ConfigEntry<KeyboardShortcut> KeyUndo1;
    public static ConfigEntry<KeyboardShortcut> KeyUndo2;
    public static ConfigEntry<KeyboardShortcut> KeyRedo1;
    public static ConfigEntry<KeyboardShortcut> KeyRedo2;
    public static ConfigEntry<KeyboardShortcut> KeyDelete1;
    public static ConfigEntry<KeyboardShortcut> KeyDelete2;

    public static void Init(ConfigFile config)
    {
      DevCommands = config.Bind(CommandStates, "devcommands", true, "Enable devcommands automatically. Required for other commands to function.");
      NoCost = config.Bind(CommandStates, "nocost", true, "No build cost, unlocks everything.");
      God = config.Bind(CommandStates, "god", true, "Makes it so you don't take damage from monsters.");
      Ghost = config.Bind(CommandStates, "ghost", true, "Prevents mobs from seeing you.");
      DebugMode = config.Bind(CommandStates, "debugmode", true, "Enables fly mode and debug hotkeys.");

      UnlimitedStamina = config.Bind(Improvements, "Unlimited Stamina", true, "Can always perform stamina actions regardless of stamina amount.");
      AllPrefabs = config.Bind(Improvements, "All Prefabs", true, "Allow placement of all functional prefabs. (Requires restart to take effect)");
      UnrestrictedPlacement = config.Bind(Improvements, "Unrestricted Placement", true, "Allow unrestricted placements (no collision, campfire on wood, etc). Note: Disabling this won't allow placement of some objects. (Requires restart to take effect)");
      NoPieceDrops = config.Bind(Improvements, "No Piece Drops", true, "Don't drop materials when pieces are destroyed.");
      NoPieceDelay = config.Bind(Improvements, "No Placement Delay", true, "No cooldowns for the hammer, cultivator, or hoe. (Requires restart to take effect)");
      NoDurabilityDrain = config.Bind(Improvements, "No Durability Drain", true, "Tools don't lose durability. (Requires restart to take effect)");
      DeleteRange = config.Bind(Improvements, "Delete Range", 5, "Range to delete objects with the delete key. This is the radius from the placement ghost's center.");

      var defaultUndo = new KeyboardShortcut(KeyCode.Z, KeyCode.LeftControl);
      KeyUndo1 = config.Bind(Hotkeys, "Undo", defaultUndo, "Warning: default (ctrl+z) conflicts with sneak and fly.");
      KeyUndo2 = config.Bind(Hotkeys, "Undo (alt)", KeyboardShortcut.Empty, "Alternative.");

      var defaultRedo1 = new KeyboardShortcut(KeyCode.Z, KeyCode.LeftControl, KeyCode.LeftShift);
      var defaultRedo2 = new KeyboardShortcut(KeyCode.Y, KeyCode.LeftControl);
      KeyRedo1 = config.Bind(Hotkeys, "Redo", defaultRedo1, "Warning: default (ctrl+shift+z) conflicts with sneak and fly.");
      KeyRedo2 = config.Bind(Hotkeys, "Redo (alt)", defaultRedo2, "Alternative.");

      KeyDelete1 = config.Bind(Hotkeys, "Delete", new KeyboardShortcut(KeyCode.Delete), "Destroys all prefabs that match the currently selected piece in the area.");
      KeyDelete2 = config.Bind(Hotkeys, "Delete (alt)", KeyboardShortcut.Empty);
    }
  }
}

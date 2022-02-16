using BepInEx.Configuration;

namespace Heinermann.BetterCreative
{
  public static class Configs
  {
    public static ConfigEntry<bool> UnlimitedStamina;
    public static ConfigEntry<bool> Ghost;
    public static ConfigEntry<bool> God;
    public static ConfigEntry<bool> NoCost;
    public static ConfigEntry<bool> DevCommands;
    public static ConfigEntry<bool> DebugMode;
    public static ConfigEntry<bool> AllPrefabs;
    public static ConfigEntry<bool> UnrestrictedPlacement;
    public static ConfigEntry<bool> NoPieceDrops;
    public static ConfigEntry<bool> NoPieceDelay;
    public static ConfigEntry<bool> NoDurabilityDrain;

    public static void Init(ConfigFile config)
    {
      DevCommands = config.Bind("Command States", "devcommands", true, "Enable devcommands automatically. Required for other commands to function.");
      NoCost = config.Bind("Command States", "nocost", true, "No build cost, unlocks everything.");
      God = config.Bind("Command States", "god", true, "Makes it so you don't take damage from monsters.");
      Ghost = config.Bind("Command States", "ghost", true, "Prevents mobs from seeing you.");
      DebugMode = config.Bind("Command States", "debugmode", true, "Enables fly mode and debug hotkeys.");

      UnlimitedStamina = config.Bind("Improvements", "Unlimited Stamina", true, "Can always perform stamina actions regardless of stamina amount.");
      AllPrefabs = config.Bind("Improvements", "All Prefabs", true, "Allow placement of all functional prefabs. (Requires restart to take effect)");
      UnrestrictedPlacement = config.Bind("Improvements", "Unrestricted Placement", true, "Allow unrestricted placements (no collision, campfire on wood, etc). Note: Disabling this won't allow placement of some objects. (Requires restart to take effect)");
      NoPieceDrops = config.Bind("Improvements", "No Piece Drops", true, "Don't drop materials when pieces are destroyed.");
      NoPieceDelay = config.Bind("Improvements", "No Placement Delay", true, "No cooldowns for the hammer, cultivator, or hoe. (Requires restart to take effect)");
      NoDurabilityDrain = config.Bind("Improvements", "No Durability Drain", true, "Tools don't lose durability. (Requires restart to take effect)");
    }
  }
}

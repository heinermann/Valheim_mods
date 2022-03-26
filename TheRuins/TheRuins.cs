using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

// TODO (remaining items):
// - Vines
// - Add random mobs (mob spawners)
// - Remove carpets if they are not on fully flat ground (QoL)
// - Configure to not spawn anywhere near each other
// - Post-spawn hooks:
//    - Filling armor stands with armours
//    - Settle floating pickable items (wood, stone piles, and campfires need to drop too)
//    - Randomize banner palettes
// - Source more builds
// - Mod Configuration
// - Location configuration overrides
// - Advanced stuff
//    - Dock detection (some builds are docks which need to be aligned to and face the water)
//    - Bridge detection (some builds have two end points supported by pillars)
//    - Dynamic terrain height requirements detection (some builds were originally built on slopes, higher than they were saved, or utilize the terrain in some way)
//

namespace Heinermann.TheRuins
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.ServerMustHaveMod, VersionStrictness.None)]
  internal class TheRuins : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.theruins";
    public const string PluginName = "TheRuins";
    public const string PluginVersion = "1.0.0";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      RuinCatalog.LoadAll();
      //Structural.InitMaterialLookup();

      PrefabManager.OnVanillaPrefabsAvailable += PrefabsAvailable;
    }

    private void PrefabsAvailable()
    {
      RuinCatalog.RegisterRuins();
    }
  }
}
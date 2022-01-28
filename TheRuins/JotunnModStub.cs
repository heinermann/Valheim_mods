using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;

namespace Heinermann.TheRuins
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.None)]
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
      PrefabManager.OnVanillaPrefabsAvailable += RegisterLocations;
    }

    private void RegisterLocations()
    {

    }
  }
}
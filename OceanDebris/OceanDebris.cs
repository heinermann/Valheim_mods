// OceanDebris
// a Valheim mod skeleton using Jötunn
// 
// File:    OceanDebris.cs
// Project: OceanDebris

using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Heinermann.OceanDebris
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class OceanDebris : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.oceandebris";
    public const string PluginName = "OceanDebris";
    public const string PluginVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Configs.Init(Config);
      harmony.PatchAll();

      PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;
    }



    private void OnVanillaPrefabsAvailable()
    {
      var coreWood = PrefabManager.Instance.GetPrefab("RoundLog");

      var conf = new VegetationConfig()
      {
        Biome = Heightmap.Biome.Ocean,
        BiomeArea = Heightmap.BiomeArea.Everything, // TODO
        //ForestThresholdMax = 1,
        //ForestThresholdMin = 0,
        GroupSizeMin = 2,
        GroupSizeMax = 7,
        //InForest = true,
        GroupRadius = 4,
        Max = 0.5f,
        Min = 5,
        ScaleMin = 0.5f,
        ScaleMax = 20f,
        MinAltitude = float.NegativeInfinity,
        MaxAltitude = float.PositiveInfinity,
        BlockCheck = false,
        ForcePlacement = true,
        MinOceanDepth = float.NegativeInfinity,
        MaxOceanDepth = float.PositiveInfinity,
        ForestThresholdMin = 0,
        ForestThresholdMax = float.PositiveInfinity
      };
      var vegetation = new CustomVegetation(coreWood, false, conf);

      ZoneManager.Instance.AddCustomVegetation(vegetation);
    }
  }
}
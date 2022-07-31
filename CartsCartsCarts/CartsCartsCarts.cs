// CartsCartsCarts
// a Valheim mod skeleton using Jötunn
// 
// File:    CartsCartsCarts.cs
// Project: CartsCartsCarts

using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Heinermann.CartsCartsCarts
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class CartsCartsCarts : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.cartscartscarts";
    public const string PluginName = "CartsCartsCarts";
    public const string PluginVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Configs.Init();

      harmony.PatchAll();
    }
  }
}
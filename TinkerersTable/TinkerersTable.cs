// TinkerersTable
// a Valheim mod skeleton using Jötunn
// 
// File:    TinkerersTable.cs
// Project: TinkerersTable

using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Heinermann.TinkerersTable
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class TinkerersTable : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.tinkererstable";
    public const string PluginName = "TinkerersTable";
    public const string PluginVersion = "0.0.1";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
      Jotunn.Logger.LogInfo("ModStub has landed");

      // To learn more about Jotunn's features, go to
      // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
    }
  }
}
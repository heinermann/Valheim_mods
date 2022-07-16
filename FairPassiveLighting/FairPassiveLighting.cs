// FairPassiveLighting
// a Valheim mod skeleton using Jötunn
// 
// File:    FairPassiveLighting.cs
// Project: FairPassiveLighting

using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Heinermann.FairPassiveLighting
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class FairPassiveLighting : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.fairpassivelighting";
    public const string PluginName = "FairPassiveLighting";
    public const string PluginVersion = "0.0.1";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {

      //PieceManager.Instance.AddPiece()
    }
  }
}
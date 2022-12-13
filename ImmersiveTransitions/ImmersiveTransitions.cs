// ImmersiveTransitions
// a Valheim mod skeleton using Jötunn
// 
// File:    ImmersiveTransitions.cs
// Project: ImmersiveTransitions

using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Heinermann.ImmersiveTransitions
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class ImmersiveTransitions : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.immersivetransitions";
    public const string PluginName = "ImmersiveTransitions";
    public const string PluginVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(PluginGUID);

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    internal static Shader PortalShader;
    internal static Shader SliceShader;

    private void Awake()
    {
      Configs.Init(Config);
      harmony.PatchAll();
      
      AssetBundle bundle = AssetUtils.LoadAssetBundleFromResources("portals");
      PortalShader = bundle.LoadAsset<Shader>("Portal");
      SliceShader = bundle.LoadAsset<Shader>("Slice");
    }
  }
}

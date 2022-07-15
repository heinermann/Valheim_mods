using BepInEx;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Jotunn
{
  [BepInPlugin(ModGuid, ModName, Version)]
  [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
  public class JotunnX : BaseUnityPlugin
  {
    public const string Version = "2.6.4";
    public const string ModName = "JotunnX";
    public const string ModGuid = "com.heinermann.jotunnx";

    internal static JotunnX Instance;
    internal static Harmony Harmony;
    internal static GameObject RootObject;

    private void Awake()
    {
      Instance = this;

      Harmony = new Harmony(ModGuid);

      AudioManager.Instance.Init();
      BiomeManager.Instance.Init();
      ClutterManager.Instance.Init();
      EnvironmentManager.Instance.Init();
    }
  }
}

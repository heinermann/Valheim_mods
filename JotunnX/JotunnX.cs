using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
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

    private void Awake()
    {
      Instance = this;

      Harmony = new Harmony(ModGuid);

      BiomeEnum.Init();
      AudioManager.Instance.Init();
      BiomeManager.Instance.Init();
      ClutterManager.Instance.Init();
      EnvironmentManager.Instance.Init();

      AddTestBiome();
    }

    // TODO: w/e
    private void AddTestBiome()
    {
      BiomeConfig biome = new BiomeConfig()
      {
        Id = "TestBiome",
        MinDistance = 100f,
        BiomeShaderColor = new Color32(128, 0, 128, 0),
        MinimapPixelColor = new Color(1f, 0.4f, 1f),
        GroundMaterial = FootStep.GroundMaterial.Wood,

        BiomeOverrideConfig = new NoiseConfig(),
        HeightmapConfig = new NoiseConfig(),
      };

      Heightmap.Biome myBiome = BiomeManager.Instance.AddBiome(biome);

      EnvironmentManager.Instance.AddEnvironment(new EnvSetup()
      {
        m_name = "TestDefault",
        m_default = true,
        m_psystems = { }
      });

      EnvironmentManager.Instance.AddBiomeEnvironment(new BiomeEnvSetup()
      {
        m_biome = myBiome,
        m_name = "TestDefault",
        m_environments = { new EnvEntry() { m_environment = "TestDefault", m_weight = 1f } }
      });
    }
  }
}

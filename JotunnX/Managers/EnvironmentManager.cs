using HarmonyLib;
using System.Collections.Generic;

namespace Jotunn.Managers
{
  /**
   * Manager for Biome environments such as weather, music, lighting, fog, and more.
   * 
   * The Valheim class is EnvMan.
   */
  public class EnvironmentManager
  {
    private static EnvironmentManager _instance;

    /// <summary>
    ///     The singleton instance of this manager.
    /// </summary>
    public static EnvironmentManager Instance => _instance ??= new EnvironmentManager();

    /// <summary>
    ///     Hide .ctor
    /// </summary>
    private EnvironmentManager() { }

    public void Init()
    {
      JotunnX.Harmony.PatchAll(typeof(Patches));
    }

    private static class Patches
    {
      [HarmonyPatch(typeof(EnvMan), "Awake"), HarmonyPostfix]
      private static void EnvMan_Awake() => Instance.EnvMan_Awake();
    }

    private List<EnvSetup> EnvironmentsToAdd = new List<EnvSetup>();
    private List<BiomeEnvSetup> BiomeEnvironmentsToAdd = new List<BiomeEnvSetup>();

    // Must add environments before adding biome environment
    public void AddBiomeEnvironment(BiomeEnvSetup biomeEnv)
    {
      if (EnvMan.instance)
      {
        EnvMan.instance.AppendBiomeSetup(biomeEnv);
        return;
      }

      BiomeEnvironmentsToAdd.Add(biomeEnv);
    }

    // Must add environments before adding biome environment
    public void AddEnvironment(EnvSetup env)
    {
      if (EnvMan.instance)
      {
        EnvMan.instance.AppendEnvironment(env);
        return;
      }

      EnvironmentsToAdd.Add(env);
    }

    private void EnvMan_Awake()
    {
      foreach(var env in EnvironmentsToAdd)
      {
        EnvMan.instance.AppendEnvironment(env);
      }

      foreach(var biomeEnv in BiomeEnvironmentsToAdd)
      {
        EnvMan.instance.AppendBiomeSetup(biomeEnv);
      }
    }
  }
}

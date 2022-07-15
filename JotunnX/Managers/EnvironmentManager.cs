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
    }

    // Must add environments before adding biome environment
    public void AddBiomeEnvironment(BiomeEnvSetup biomeEnv)
    {
      EnvMan.instance.AppendBiomeSetup(biomeEnv);
    }

    // Must add environments before adding biome environment
    public void AddEnvironment(EnvSetup env)
    {
      EnvMan.instance.AppendEnvironment(env);
    }
  }
}

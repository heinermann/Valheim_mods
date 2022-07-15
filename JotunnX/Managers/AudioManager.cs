namespace Jotunn.Managers
{
  /**
   * Manager for ambient biome SFX.
   * 
   * The Valheim class is AudioMan.
   */
  public class AudioManager
  {
    private static AudioManager _instance;

    /// <summary>
    ///     The singleton instance of this manager.
    /// </summary>
    public static AudioManager Instance => _instance ??= new AudioManager();

    /// <summary>
    ///     Hide .ctor
    /// </summary>
    private AudioManager() { }

    public void Init()
    {
    }

    public void AddBiomeAmbience(AudioMan.BiomeAmbients biomeAmbience)
    {
      AudioMan.instance.m_randomAmbients.Add(biomeAmbience);
    }
  }
}

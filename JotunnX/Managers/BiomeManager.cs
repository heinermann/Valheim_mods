namespace Jotunn.Managers
{
  /**
   * Manager to create new biomes from scratch.
   */
  public class BiomeManager
  {
    private static BiomeManager _instance;

    /// <summary>
    ///     The singleton instance of this manager.
    /// </summary>
    public static BiomeManager Instance => _instance ??= new BiomeManager();

    /// <summary>
    ///     Hide .ctor
    /// </summary>
    private BiomeManager() { }
  }
}

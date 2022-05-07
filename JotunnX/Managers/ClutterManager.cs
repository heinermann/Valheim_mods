namespace Jotunn.Managers
{
  /**
   * Manager for non-interactible biome vegetation and clutter, such as grass, reeds, lilly pads, etc.
   * 
   * The Valheim class is ClutterSystem.
   */
  public class ClutterManager
  {
    private static ClutterManager _instance;

    /// <summary>
    ///     The singleton instance of this manager.
    /// </summary>
    public static ClutterManager Instance => _instance ??= new ClutterManager();

    /// <summary>
    ///     Hide .ctor
    /// </summary>
    private ClutterManager() { }
  }
}

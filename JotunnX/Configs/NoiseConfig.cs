using UnityEngine;

namespace Jotunn.Configs
{
  /**
   * TODO Replace this entirely with a LibNoise implementation used for both biome determination and biome height.
   * 
   * Simple configuration for biome noise, determining things such as where the biome will exist and the terrain height.
   * 
   * Note: Y in this context is z in the game.
   * 
   * The formula used in conditionals is:
   * <c> Mathf.PerlinNoise((OffsetX + x) * 0.001f, (OffsetY + y) * 0.001f) < ConditionalThreshold && distance > MinDistance && distance < MaxDistance && baseHeight > MinHeight && baseHeight < MaxHeight </c>
   *
   */
  public class NoiseConfig
  {
    /**
     * Random offset to distinguish where the biome will exist in the world.
     */
    public float OffsetX { get; set; } = Random.Range(-10000, 10000);

    /**
     * Random offset to distinguish where the biome will exist in the world.
     */
    public float OffsetY { get; set; } = Random.Range(-10000, 10000);

    /**
     * Between 0f and 1f. Mountains are always at a base height of 0.4f, swamps are between 0.05f and 0.25f.
     */
    public float MinHeight { get; set; } = 0f;

    /**
     * Between 0f and 1f. Swamps are maximum 0.25f, mountains are at least 0.4f.
     */
    public float MaxHeight { get; set; } = 1f;

    /**
     * Minimum distance from spawn.
     */
    public float MinDistance { get; set; } = 600f;

    /**
     * Maximum distance from spawn.
     */
    public float MaxDistance { get; set; } = float.MaxValue;

    /**
     * Threshold to meet conditional requirements will be the result of PerlinNoise is less than this value.
     */
    public float ConditionalThreashold { get; set; } = 0.4f;


    public bool MatchesBiome(float x, float y, float distanceFromCenter, float baseHeight)
    {
      return Mathf.PerlinNoise((OffsetX + x) * 0.001f, (OffsetY + y) * 0.001f) < ConditionalThreashold &&
        MinDistance < distanceFromCenter && distanceFromCenter < MaxDistance &&
        MinHeight < baseHeight && baseHeight < MaxHeight;
    }
  }
}

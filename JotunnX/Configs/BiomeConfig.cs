using UnityEngine;

namespace Jotunn.Configs
{
  /**
   * Configuration for new biomes.
   */
  public class BiomeConfig
  {
    /**
     * The Biome ID. This is used for the enum entry and the name is determined by $biome_[Id] (i.e. $biome_mybiomeid).
     */
    public string Id { get; set; } = string.Empty;

    /**
     * The default ground material for determining footstep sounds.
     */
    FootStep.GroundMaterial GroundMaterial { get; set; } = FootStep.GroundMaterial.Default;

    /**
     * The shader colour used for biome ground texture.
     */
    Color32 BiomeShaderColor { get; set; } = new Color32(0, 0, 0, 0);
    
    /**
     * The biome colour for the minimap.
     */
    Color MinimapPixelColor { get; set; } = Color.white;

    /**
     * Determines how dense a "forest" needs to be to draw trees on the minimap.
     * 0f means never draw trees and float.PositiveInfinity means always draw trees.
     * Values are usually between 0f and 2f. Defaults are 1.15f for Meadows and 0.8f for Plains.
     */
    float MinimapForestFactor { get; set; } = 1.15f;

    /**
     * The minimum distance from spawn that the biome can appear.
     */
    float MinDistance { get; set; } = 0f;

    /**
     * The maximum distance from spawn that the biome can appear.
     */
    float MaxDistance { get; set; } = 12000f;

    /**
     * Noise indicating the biome heightmap.
     */
    NoiseConfig HeightmapConfig { get; set; } = null;

    /**
     * Assigns the biome based on the noise value. If the value is greater than 0.5f then the area overwrites the surrounding biomes.
     */
    NoiseConfig BiomeOverrideConfig { get; set; } = null;

    // TODO: Need to consider accidental erasure of biomes, there should be a mechanism to ensure
    // vanilla biomes will still exist after replacing them.

    // TODO: Rivers config (heightmap config)
  }
}

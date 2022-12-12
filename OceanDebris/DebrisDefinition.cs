namespace Heinermann.OceanDebris
{
  public class DebrisDefinition
  {
    public string PrefabName { get; set; }
    public Heightmap.BiomeArea BiomeArea { get; set; } = Heightmap.BiomeArea.Everything;
    
    public bool InForest { get; set; } = false;
    public float ForestThresholdMin { get; set; } = 0f;
    public float ForestThresholdMax { get; set; } = float.PositiveInfinity;

    public (int, int) GroupSize { get; set; } = (1, 1);

    public float GroupRadius { get; set; } = 25f;

    public float MinOceanDepth { get; set; } = float.NegativeInfinity;
    public float MaxOceanDepth { get; set; } = float.PositiveInfinity;

    public float Min { get; set; } = 1f;
    public float Max { get; set; } = 0.5f;

    public (float, float) Scale { get; set; } = (1f, 1f);
  }
}

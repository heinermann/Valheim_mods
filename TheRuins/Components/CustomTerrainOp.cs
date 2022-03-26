using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.TheRuins.Components
{
  public class CustomTerrainOp : SlowUpdate
  {
    public TerrainOp.Settings m_settings = new TerrainOp.Settings() {
      m_square = false,
      m_paintCleared = false
    };

    public void OnEnable()
    {
      List<Heightmap> heightmaps = new List<Heightmap>();
      Heightmap.FindHeightmap(transform.position, m_settings.GetRadius(), heightmaps);
      foreach (Heightmap hm in heightmaps)
      {
        TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(hm.transform.position);
        ApplyOperation(terrainComp);
      }

      GameObject.Destroy(this);
    }

    private void ApplyOperation(TerrainComp terrainComp)
    {
      ZPackage zPackage = new ZPackage();
      zPackage.Write(transform.position);
      m_settings.Serialize(zPackage);
      terrainComp.GetComponent<ZNetView>().InvokeRPC("ApplyOperation", zPackage);
    }
  }
}

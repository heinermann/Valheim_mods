using Heinermann.TheRuins;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheRuinsFeatureGen
{
  /**
   * Feature ideas
   * - Nearest vertical beam bottom (dx, dy, dz, abs distance)
   * - Nearest horizontal beam bottom (dx, dy, dz, abs distance)
   * - Nearest floor bottom (dx, dy, dz, abs distance)
   * - Nearest roof bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest wall bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest angled beam bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest campfire bottom (dx, dy, dz, abs distance)
   * - Nearest other piece (dx, dy, dz, abs distance)
   * - Lowest cell piece y
   * - Lowest/median/avg neighbour cell piece y
   * - Diff lowest cell and lowest neighbour
   * - Matrix of nearest + lowest and each neighbour pieces (16 x 8?)
   */
  public static class FeatureGenerator
  {
    class FeatureData
    {
      public PieceEntry component;
      
      public float y;

      public FeatureData(PieceEntry piece)
      {
        component = piece;
        y = piece.position.y - PieceData.GetHeightExtent(piece.prefabName);
      }
    }

    class CellManager
    {
      public Dictionary<Tuple<int, int>, FeatureData> cells = new Dictionary<Tuple<int, int>, FeatureData>();

      public CellManager(Blueprint blueprint)
      {
        blueprint.Pieces.ForEach(Add);
      }

      public FeatureData At(Tuple<int,int> key)
      {
        FeatureData result = null;
        cells.TryGetValue(key, out result);
        return result;
      }

      public FeatureData At(int x, int z)
      {
        return At(Tuple.Create(x, z));
      }

      public void Add(Tuple<int, int> key, PieceEntry piece)
      {
        FeatureData data = At(key);
        if (data == null)
        {
          data = new FeatureData(piece);
          cells.Add(key, data);
          return;
        }

        var newData = new FeatureData(piece);
        if (newData.y < data.y)
        {
          cells[key] = newData;
        }
      }

      public void Add(int x, int z, PieceEntry piece)
      {
        Add(Tuple.Create(x, z), piece);
      }

      public void Add(PieceEntry piece)
      {
        int x = Mathf.RoundToInt(piece.position.x);
        int z = Mathf.RoundToInt(piece.position.z);
        Add(x, z, piece);
      }

      public int CountEmptyNeighbours(int x, int z, int range = 1)
      {
        int result = 0;
        for (int i = x - range; i <= x + range; ++i)
        {
          for (int j = z - range; j <= z + range; ++j)
          {
            if (At(i, j) == null)
              result++;
          }
        }
        return result;
      }
    }

    static void GenerateFeatures(Blueprint blueprint)
    {
      CellManager cellMgr = new CellManager(blueprint);
      
      int rad = Mathf.CeilToInt(blueprint.GetMaxBuildRadius() + 1f);
      for (int z = -rad; z <= rad; ++z)
      {
        for (int x = -rad; x <= rad; ++x)
        {

        }
      }

      foreach (Tuple<int, int> key in cellMgr.cells.Keys)
      {

      }
    }

    public static void GenerateFeaturesForAll(List<Blueprint> blueprints)
    {
      blueprints.ForEach(GenerateFeatures);
    }
  }
}

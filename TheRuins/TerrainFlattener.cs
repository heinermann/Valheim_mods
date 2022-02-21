using Heinermann.TheRuins.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Heinermann.TheRuins
{
  internal static class TerrainFlattener
  {
    // Granularity of below algorithms, higher = more fidelity but more terrain modifier instances
    const float TerrainGranularity = 2f;

    private struct LevelData
    {
      public Vector3 position;
      public WearNTear component;

      public LevelData(float x, float y, float z, WearNTear component)
      {
        position = new Vector3(x, y, z);
        this.component = component;
      }

      public Tuple<int,int> GetPositionTuple()
      {
        int x = Mathf.RoundToInt(position.x * TerrainGranularity);
        int z = Mathf.RoundToInt(position.z * TerrainGranularity);
        return Tuple.Create(x, z);
      }
    }

    private static GameObject CreateTerrainModifierPrefab(GameObject parent)
    {
      // TODO: Should be in asset bundle
      GameObject prefab = new GameObject("Terrain_Mod_Prefab");
      var terrain = prefab.AddComponent<TerrainModifier>();
      InitTerrainModifier(terrain);

      var znet = prefab.AddComponent<ZNetView>();
      znet.m_persistent = true;
      znet.m_type = ZDO.ObjectType.Default;

      GameObject result = GameObject.Instantiate(prefab, parent.transform, false);
      result.transform.position = Vector3.zero;
      result.transform.rotation = Quaternion.identity;
      return result;
    }

    private static void InitTerrainModifier(TerrainModifier modifier)
    {
      modifier.m_sortOrder = 0;
      modifier.m_square = false;
      modifier.m_paintCleared = false;
      modifier.m_playerModifiction = false;
      modifier.m_spawnAtMaxLevelDepth = true;
    }

    private static TerrainModifier FlattenAreaAt(GameObject prefab, float relativeTargetLevel, Vector3 position, float radius, string name)
    {
      GameObject pieceObj = CreateTerrainModifierPrefab(prefab);
      pieceObj.transform.position = position;
      pieceObj.name = name;

      var modifier = pieceObj.GetComponent<TerrainModifier>();
      modifier.m_level = true;
      modifier.m_levelRadius = radius;
      modifier.m_levelOffset = relativeTargetLevel;

      modifier.m_sortOrder = Mathf.RoundToInt(-(position.y + relativeTargetLevel) * 100f);

      modifier.m_smooth = true;
      modifier.m_smoothPower = 3f;
      modifier.m_smoothRadius = radius + 4f;
      return modifier;
    }

    private static TerrainModifier FlattenArea(GameObject prefab, float relativeTargetLevel, float flattenAreaRadius)
    {
      TerrainModifier modifier = FlattenAreaAt(prefab, relativeTargetLevel, Vector3.zero, flattenAreaRadius + 2f, "TheRuins_Flatten_Area");
      modifier.m_smooth = true;
      modifier.m_smoothPower = 4f;
      modifier.m_smoothRadius = modifier.m_levelRadius + 4f;
      return modifier;
    }

    private static void CompareComponentHeights(Dictionary<Tuple<int, int>, LevelData> lowestPositions, WearNTear wear, int x, int z)
    {
      var point = Tuple.Create(x, z);

      // TODO optimize this so it isn't recalc'd
      float thisY = wear.transform.position.y - PrefabHelpers.GetPieceHeight(wear.gameObject) / 2f;

      if (wear.name.ContainsAny("ladder", "stair", "upsidedown", "26", "45"))
      {
        thisY += 0.5f;
      }

      if (wear.name.Contains("floor"))
      {
        thisY -= 0.1f;
      }

      LevelData compareWith;
      if (lowestPositions.TryGetValue(point, out compareWith))
      {
        if (thisY < compareWith.position.y)
          lowestPositions[point] = new LevelData(x / TerrainGranularity, thisY, z / TerrainGranularity, wear);
      }
      else
      {
        lowestPositions.Add(point, new LevelData(x / TerrainGranularity, thisY, z / TerrainGranularity, wear));
      }
    }

    private static float GetLowestAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, Tuple<int, int> target)
    {
      float best = positions[target].position.y;
      for (int i = -1; i <= 1; ++i)
      {
        for (int j = -1; j <= 1; ++j)
        {
          if (i == 0 && j == 0) continue;

          LevelData current;
          if (positions.TryGetValue(Tuple.Create(target.Item1 + i, target.Item2 + j), out current))
          {
            if (current.position.y < best)
            {
              best = current.position.y;
            }
          }
        }
      }
      return best;
    }

    private static float GetMedianAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, Tuple<int, int> target)
    {
      List<float> heights = new List<float>();
      for (int i = -1; i <= 1; ++i)
      {
        for (int j = -1; j <= 1; ++j)
        {
          if (i == 0 && j == 0) continue;

          LevelData current;
          if (positions.TryGetValue(Tuple.Create(target.Item1 + i, target.Item2 + j), out current))
          {
            heights.Add(current.position.y);
          }
        }
      }
      
      if (heights.Count == 0) return 0;
      
      heights.Sort();

      int mid = heights.Count / 2;
      if (heights.Count % 2 != 0)
      {
        return heights[mid];
      }
      return (heights[mid] + heights[mid - 1]) / 2;
    }

    private static int NumEmptyAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, LevelData data)
    {
      int result = 0;
      Tuple<int, int> target = data.GetPositionTuple();
      for (int i = -1; i <= 1; ++i)
      {
        for (int j = -1; j <= 1; ++j)
        {
          if (!positions.ContainsKey(Tuple.Create(target.Item1 + i, target.Item2 + j)))
          {
            ++result;
          }
        }
      }
      return result;
    }

    private static bool HasEmptyAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, LevelData data)
    {
      Tuple<int, int> target = data.GetPositionTuple();
      for (int i = -1; i <= 1; ++i)
      {
        for (int j = -1; j <= 1; ++j)
        {
          if (!positions.ContainsKey(Tuple.Create(target.Item1 + i, target.Item2 + j)))
            return true;
        }
      }
      return false;
    }

    private static void MakeDirt(GameObject piece)
    {
      TerrainModifier terrain = piece.gameObject.AddComponent<TerrainModifier>();
      InitTerrainModifier(terrain);

      terrain.m_paintCleared = true;
      terrain.m_paintRadius = PrefabHelpers.GetPieceRadius(piece.gameObject) + 0.5f;
      terrain.m_paintType = TerrainModifier.PaintType.Dirt;
      terrain.m_sortOrder = 10000;
    }

    private static void DebugHeights(Dictionary<Tuple<int, int>, LevelData> positions, string prefabName)
    {
      if (positions.Count == 0) return;

      int xMin = positions.Keys.Min(v => v.Item1);
      int xMax = positions.Keys.Max(v => v.Item1);
      int zMin = positions.Keys.Min(v => v.Item2);
      int zMax = positions.Keys.Max(v => v.Item2);

      StringBuilder heightStr = new StringBuilder($"Heights for {prefabName}\n");
      for (int z = zMin; z <= zMax; ++z)
      {
        for (int x = xMin; x <= xMax; ++x)
        {
          LevelData data;
          if (positions.TryGetValue(Tuple.Create(x, z), out data))
          {
            heightStr.Append($"{data.position.y:F1};");
          }
          else
          {
            heightStr.Append(";");
          }
        }
        heightStr.Append("\n");
      }
      Jotunn.Logger.LogInfo(heightStr);
    }

    // Naiive solution, doesn't consider geometries (i.e. 4-long stone wall) and has fairly major rounding errors to integer
    public static void PrepareTerrainModifiers(GameObject prefab)
    {
      // Get the leveling data from each position
      var lowestPositions = new Dictionary<Tuple<int, int>, LevelData>();
      foreach (WearNTear wear in prefab.GetComponentsInChildren<WearNTear>(includeInactive: true))
      {
        //if (wear.name.Contains("roof")) continue;

        float pieceRadius = PrefabHelpers.GetPieceRadius(wear.gameObject);

        int pieceX = Mathf.RoundToInt(wear.transform.position.x * TerrainGranularity);
        int pieceZ = Mathf.RoundToInt(wear.transform.position.z * TerrainGranularity);
        
        int xMin, xMax, zMin, zMax;
        xMin = zMin = Mathf.FloorToInt(-pieceRadius * TerrainGranularity - 0.5f);
        xMax = zMax = Mathf.CeilToInt(pieceRadius * TerrainGranularity + 0.5f);

        for (int z = zMin; z <= zMax; ++z)
        {
          for (int x = xMin; x <= xMax; ++x)
          {
            if (x * x + z * z <= Mathf.Max(xMax * xMax, xMin * xMin))
            {
              CompareComponentHeights(lowestPositions, wear, x + pieceX, z + pieceZ);
            }
          }
        }
      }



      DebugHeights(lowestPositions, prefab.name);

      // Flatten areas with pieces in it
      int index = 0;
      foreach (LevelData current in lowestPositions.Values)
      {
        float radius = 1.5f / TerrainGranularity;

        Vector3 targetPosition = current.position;
        if (!current.component.gameObject.HasAnyComponent("Fireplace")) {
          float lowest = GetLowestAdjacent(lowestPositions, current.GetPositionTuple());
          if (lowest < current.position.y) continue;
        }

        if (current.component.m_materialType == WearNTear.MaterialType.Stone || current.component.GetComponent("Fireplace"))
        {
          MakeDirt(current.component.gameObject);
        }

        FlattenAreaAt(prefab, 0, targetPosition, radius, $"TheRuins_Flatten_{index}");
        ++index;
      }
    }
  }
}

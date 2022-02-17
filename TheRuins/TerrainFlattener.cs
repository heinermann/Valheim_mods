using Heinermann.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private static LevelData GetLowestAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, Tuple<int, int> target)
    {
      LevelData best = positions[target];
      for (int i = -1; i <= 1; ++i)
      {
        for (int j = -1; j <= 1; ++j)
        {
          if (i == 0 && j == 0) continue;

          LevelData current;
          if (positions.TryGetValue(Tuple.Create(i, j), out current))
          {
            if (current.position.y < best.position.y)
            {
              best = current;
            }
          }
        }
      }
      return best;
    }

    private static bool HasLowerAdjacent(Dictionary<Tuple<int, int>, LevelData> positions, LevelData data)
    {
      int x = Mathf.RoundToInt(data.position.x * TerrainGranularity);
      int z = Mathf.RoundToInt(data.position.z * TerrainGranularity);
      return !GetLowestAdjacent(positions, Tuple.Create(x, z)).Equals(data);
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

    // Naiive solution, doesn't consider geometries (i.e. 4-long stone wall) and has fairly major rounding errors to integer
    public static void PrepareTerrainModifiers(GameObject prefab, float flattenAreaRadius)
    {
      // Get the leveling data from each position
      var lowestPositions = new Dictionary<Tuple<int, int>, LevelData>();
      foreach (WearNTear wear in prefab.GetComponentsInChildren<WearNTear>(includeInactive: true))
      {
        int x = Mathf.RoundToInt(wear.transform.position.x * TerrainGranularity);
        int z = Mathf.RoundToInt(wear.transform.position.z * TerrainGranularity);
        for (int i = -1; i <= 1; ++i)
        {
          for (int j = -1; j <= 1; ++j)
          {
            CompareComponentHeights(lowestPositions, wear, x + i, z + j);
          }
        }
      }

      // Flatten entire area at high priority first
      float targetY = Mathf.Max(lowestPositions.Values
        .Select(w => w.position.y)
        .Where(y => y < 0.5f)
        .Average(), 0);

      TerrainModifier areaFlatten = FlattenArea(prefab, targetY, flattenAreaRadius);
      areaFlatten.m_sortOrder = -10000;

      // Flatten areas with pieces in it
      int index = 0;
      foreach (LevelData lowest in lowestPositions.Values)
      {
        if (lowest.position.y > 0f &&
          HasLowerAdjacent(lowestPositions, lowest) &&
          !lowest.component.gameObject.HasAnyComponent("Fireplace") &&
          !lowest.component.name.Contains("floor")
        )
        {
          continue;
        }

        if (lowest.component.m_materialType == WearNTear.MaterialType.Stone || lowest.component.GetComponent("Fireplace"))
        {
          MakeDirt(lowest.component.gameObject);
        }

        if (lowest.position.y == 0f) continue;

        float radius = 1f / TerrainGranularity;
        FlattenAreaAt(prefab, 0, lowest.position, radius, $"TheRuins_Flatten_{index}");

        ++index;
      }
    }
  }
}

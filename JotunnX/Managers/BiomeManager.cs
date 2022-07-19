using HarmonyLib;
using Jotunn.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Heightmap;

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

    public void Init()
    {
      GetBaseHeightInfo = AccessTools.Method("WorldGenerator:GetBaseHeight");

      JotunnX.Harmony.PatchAll(typeof(Patches));
    }

    private static class Patches
    {
      [HarmonyPatch(typeof(FootStep), "GetGroundMaterial"), HarmonyPostfix]
      private static void FootStep_GetGroundMaterial(Character character, Vector3 point, ref FootStep.GroundMaterial __result) => Instance.FootStep_GetGroundMaterial(character, point, ref __result);

      [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiome)), HarmonyPrefix]
      private static bool Heightmap_GetBiome(Heightmap __instance, Vector3 point, ref Biome __result, Biome[] ___m_cornerBiomes) => Instance.Heightmap_GetBiome(__instance, point, ref __result, ___m_cornerBiomes);

      [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiomeColor), new[] { typeof(Biome) }), HarmonyPrefix]
      private static bool Heightmap_GetBiomeColor(Biome biome, ref Color32 __result) => Instance.Heightmap_GetBiomeColor(biome, ref __result);

      [HarmonyPatch(typeof(Minimap), "GetPixelColor"), HarmonyPrefix]
      private static bool Minimap_GetPixelColor(Biome biome, ref Color __result) => Instance.Minimap_GetPixelColor(biome, ref __result);

      [HarmonyPatch(typeof(Minimap), "GetMaskColor"), HarmonyPrefix]
      private static bool Minimap_GetMaskColor(float wx, float wy, float height, Biome biome, ref Color __result) => Instance.Minimap_GetMaskColor(wx, wy, height, biome, ref __result);

      [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new[] { typeof(float), typeof(float) }), HarmonyPrefix]
      private static bool WorldGenerator_GetBiome(float wx, float wy, ref Biome __result, World ___m_world, WorldGenerator __instance) => Instance.WorldGenerator_GetBiome(wx, wy, ref __result, ___m_world, __instance);

      [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight)), HarmonyPrefix]
      private static bool WorldGenerator_GetBiomeHeight(Biome biome, float wx, float wy, ref float __result, World ___m_world, WorldGenerator __instance) => Instance.WorldGenerator_GetBiomeHeight(biome, wx, wy, ref __result, ___m_world, __instance);
    }

    private MethodInfo GetBaseHeightInfo;


    // ##############################################################################################################################
    // ## Manager
    // ##############################################################################################################################

    internal Dictionary<Biome, BiomeConfig> biomes = new Dictionary<Biome, BiomeConfig>();

    public Biome AddBiome(BiomeConfig biome)
    {
      Biome biomeValue = BiomeEnum.RegisterBiome(biome.Id);
      biomes.Add(biomeValue, biome);
      return biomeValue;
    }

    public BiomeConfig GetBiome(string id)
    {
      Biome biome = (Biome)Enum.Parse(typeof(Biome), id);
      biomes.TryGetValue(biome, out BiomeConfig result);
      return result;
    }

    // ##############################################################################################################################
    // ## Patching
    // ##############################################################################################################################

    // REFS:
    //  - Character.GetLastGroundNormal()
    //  - Character.GetLastGroundCollider()
    //  - Heightmap.GetBiome()
    private void FootStep_GetGroundMaterial(Character character, Vector3 point, ref FootStep.GroundMaterial result)
    {
      // If result is GenericGround that means all the other conditions for the biome/piece checks are fulfilled
      if (result != FootStep.GroundMaterial.GenericGround) return;

      float angle = Mathf.Acos(Mathf.Clamp01(character.GetLastGroundNormal().y)) * (180 / Mathf.PI);

      Heightmap heightmap = character.GetLastGroundCollider()?.GetComponent<Heightmap>();
      if (heightmap != null && biomes.TryGetValue(heightmap.GetBiome(point), out BiomeConfig config))
      {
        if (angle < 40f) result = config.GroundMaterial;
      }
    }

    private static void WorldToNormalizedHM(Heightmap self, Vector3 worldPos, out float x, out float z)
    {
      float num = self.m_width * self.m_scale;
      Vector3 vector = worldPos - self.transform.position;
      Vector3 normalized = vector / num + Vector3.one * 0.5f;
      x = normalized.x;
      z = normalized.z;
    }

    private static float Distance(float x, float y, float rx, float ry)
    {
      float dx = x - rx;
      float dy = y - ry;
      float r = 1.414f - Mathf.Sqrt(dx * dx + dy * dy);
      return r * r * r;
    }

    // REFS:
    //  - Heightmap.m_isDistantLod
    //  - Heightmap.m_cornerBiomes
    //  - Heightmap.m_width (call to WorldToNormalizedHM)
    //  - Heightmap.m_scale (call to WorldToNormalizedHM)
    private float[] cornerDistances = new float[4];
    private bool Heightmap_GetBiome(Heightmap self, Vector3 point, ref Biome result, Biome[] m_cornerBiomes)
    {
      if (self.m_isDistantLod) return true;

      if (m_cornerBiomes.All(biome => biome == m_cornerBiomes[0]))
      {
        result = m_cornerBiomes[0];
        return false;
      }

      WorldToNormalizedHM(self, point, out float x, out float z);

      // Is this even needed?
      cornerDistances[0] = Distance(x, z, 0, 0);
      cornerDistances[1] = Distance(x, z, 1, 0);
      cornerDistances[2] = Distance(x, z, 0, 1);
      cornerDistances[3] = Distance(x, z, 1, 1);
      int cornerChoice = Array.IndexOf(cornerDistances, cornerDistances.Max());

      result = m_cornerBiomes[cornerChoice];
      return false;
    }

    private bool Heightmap_GetBiomeColor(Biome biome, ref Color32 result)
    {
      if (biomes.TryGetValue(biome, out BiomeConfig config))
      {
        result = config.BiomeShaderColor;
        return false;
      }
      return true;
    }

    private bool Minimap_GetPixelColor(Biome biome, ref Color result)
    {
      if (biomes.TryGetValue(biome, out BiomeConfig config))
      {
        result = config.MinimapPixelColor;
        return false;
      }
      return true;
    }

    private static Color forest = new Color(1f, 0f, 0f, 0f);
    private static Color noForest = new Color(0f, 0f, 0f, 0f);
    // REFS:
    //  - ZoneSystem.instance.m_waterLevel
    //  - WorldGenerator.GetForestFactor
    private bool Minimap_GetMaskColor(float wx, float wy, float height, Biome biome, ref Color result)
    {
      if (height < ZoneSystem.instance.m_waterLevel) return true;

      if (biomes.TryGetValue(biome, out BiomeConfig config))
      {
        bool isInForest = WorldGenerator.GetForestFactor(new Vector3(wx, 0f, wy)) < config.MinimapForestFactor;
        result = isInForest ? forest : noForest;
        return false;
      }
      return true;
    }

    // REFS:
    //  - WorldGenerator.m_world.m_menu
    private bool WorldGenerator_GetBiome(float wx, float wy, ref Biome result, World m_world, WorldGenerator self)
    {
      if (m_world.m_menu) return true;

      float distanceFromCenter = new Vector2(wx, wy).magnitude;

      // TODO: private access?
      float baseHeight = (float)GetBaseHeightInfo.Invoke(self, new object[] { wx, wy, false });

      foreach (var configPair in biomes)
      {
        BiomeConfig config = configPair.Value;

        if (config.BiomeOverrideConfig.MatchesBiome(wx, wy, distanceFromCenter, baseHeight))
        {
          result = configPair.Key;
          return false;
        }
      }
      return true;
    }

    // REFS:
    //  - WorldGenerator.m_world.m_menu
    private bool WorldGenerator_GetBiomeHeight(Biome biome, float wx, float wy, ref float result, World m_world, WorldGenerator self)
    {
      if (m_world.m_menu) return true;

      if (biomes.TryGetValue(biome, out BiomeConfig config))
      {
        // TODO config.HeightmapConfig / NoiseConfig
        result = self.GetBiomeHeight(Biome.Meadows, wx, wy);
        return false;
      }
      return true;
    }
  }
}

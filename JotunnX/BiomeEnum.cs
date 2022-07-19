using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jotunn
{
  // TODO: Need to patch more Enum functions, some calls will not be compatible
  [HarmonyPatch(typeof(Enum))]
  public static class BiomeEnum
  {
    public static Dictionary<Heightmap.Biome, string> BiomeNameLookup = new Dictionary<Heightmap.Biome, string>();
    public static readonly Dictionary<string, object> ReverseBiomeNameLookup = new Dictionary<string, object>();

    public static void Init()
    {
      JotunnX.Harmony.PatchAll(typeof(BiomeEnum));
    }

    public static Heightmap.Biome RegisterBiome(string name)
    {
      uint bit = 0x40000000;
      while (BiomeNameLookup.ContainsKey((Heightmap.Biome)bit) && bit > (uint)Heightmap.Biome.BiomesMax) {
        bit >>= 1;
      }

      if (bit <= (uint)Heightmap.Biome.BiomesMax) throw new Exception("Exceeded the biome number limit. There are too many biomes.");

      RegisterBiome(name, bit);
      Jotunn.Logger.LogInfo($"Registered biome \"{name}\" as value {bit}");
      return (Heightmap.Biome)bit;
    }

    private static void RegisterBiome(string name, uint value)
    {
      Heightmap.Biome enumValue = (Heightmap.Biome)value;
      BiomeNameLookup.Add(enumValue, name);
      ReverseBiomeNameLookup.Add(name, enumValue);
    }

    /* // INVALID IL CODE, look up Type.GetEnumName, Type.GetEnumNames, Type.GetEnumValues, Type.GetEnumData, Type.IsEnumDefined
    [HarmonyPrefix]
    [HarmonyPatch("GetName")]
    static bool GetNamePrefix(ref Enum __instance, Type enumType, object value, ref string __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      return !BiomeNameLookup.TryGetValue((Heightmap.Biome)__instance, out __result);
    }*/

    /*
    [HarmonyPostfix]
    [HarmonyPatch("GetNames")]
    static void GetNamesPostfix(ref Enum __instance, Type enumType, ref string[] __result)
    {
      if (!(__instance is Heightmap.Biome)) return;
      __result = __result.AddRangeToArray(ReverseBiomeNameLookup.Keys.ToArray());
    }*/

    /*
    [HarmonyPostfix]
    [HarmonyPatch("GetValues")]
    static void GetValuesPostfix(ref Enum __instance, Type enumType, ref Array __result)
    {
      if (!(__instance is Heightmap.Biome)) return;
      __result = __result.Cast<object>().ToArray().AddRangeToArray(ReverseBiomeNameLookup.Values.ToArray());
    }
    */
    /*
    [HarmonyPrefix]
    [HarmonyPatch("IsDefined")]
    static bool IsDefinedPrefix(ref Enum __instance, Type enumType, object value, ref bool __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      __result = BiomeNameLookup.ContainsKey((Heightmap.Biome)value);
      return !__result;
    }*/

    [HarmonyPrefix]
    [HarmonyPatch("ToString", new Type[] { })]
    static bool ToStringPrefix(ref Enum __instance, ref string __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      return !BiomeNameLookup.TryGetValue((Heightmap.Biome)__instance, out __result);
    }
  }
}

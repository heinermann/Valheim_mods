﻿using HarmonyLib;
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

    public static void RegisterBiome(string name, int value)
    {
      Heightmap.Biome enumValue = (Heightmap.Biome)value;
      BiomeNameLookup.Add(enumValue, name);
      ReverseBiomeNameLookup.Add(name, enumValue);
    }

    [HarmonyPrefix]
    [HarmonyPatch("GetName")]
    static bool GetNamePrefix(ref Enum __instance, Type enumType, object value, ref string __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      return !BiomeNameLookup.TryGetValue((Heightmap.Biome)__instance, out __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch("GetNames")]
    static void GetNamesPostfix(ref Enum __instance, Type enumType, ref string[] __result)
    {
      if (!(__instance is Heightmap.Biome)) return;
      __result = __result.AddRangeToArray(ReverseBiomeNameLookup.Keys.ToArray());
    }

    [HarmonyPostfix]
    [HarmonyPatch("GetValues")]
    static void GetValuesPostfix(ref Enum __instance, Type enumType, ref Array __result)
    {
      if (!(__instance is Heightmap.Biome)) return;
      __result = __result.Cast<object>().ToArray().AddRangeToArray(ReverseBiomeNameLookup.Values.ToArray());
    }

    [HarmonyPrefix]
    [HarmonyPatch("IsDefined")]
    static bool IsDefinedPrefix(ref Enum __instance, Type enumType, object value, ref bool __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      __result = BiomeNameLookup.ContainsKey((Heightmap.Biome)value);
      return !__result;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ToString", new Type[] { })]
    static bool ToStringPrefix(ref Enum __instance, ref string __result)
    {
      if (!(__instance is Heightmap.Biome)) return true;
      return !BiomeNameLookup.TryGetValue((Heightmap.Biome)__instance, out __result);
    }
  }
}

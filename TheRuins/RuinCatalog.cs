using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heinermann.TheRuins
{
  internal static class RuinCatalog
  {
    static Dictionary<Heightmap.Biome, List<Blueprint>> biomeRuins = new Dictionary<Heightmap.Biome, List<Blueprint>>();

    public static void RegisterRuins()
    {
      foreach (var biome in biomeRuins)
      {
        foreach (var blueprint in biome.Value)
        {
          var ruin = new Ruin(blueprint, biome.Key);
          ruin.FullyRuinBlueprintToLocation();
        }
      }
    }

    private static void LoadForBiome(Heightmap.Biome biome)
    {
      string biomeName = Enum.GetName(typeof(Heightmap.Biome), biome).ToLower();
      string pluginConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, TheRuins.PluginName);

      var matcher = new Matcher();
      matcher.AddInclude($"**/{biomeName}/**/*.blueprint");
      matcher.AddInclude($"**/{biomeName}/**/*.vbuild");

      var files = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(pluginConfigPath)));

      var blueprints = new List<Blueprint>();
      foreach (var file in files.Files)
      {
        string blueprintPath = Path.Combine(pluginConfigPath, file.Path);
        try
        {
          Blueprint blueprint = Blueprint.FromFile(blueprintPath);
          blueprint.UniqueName = "TheRuins_" + file.Path.Replace('/', '_').Replace('\\', '_');
          blueprints.Add(blueprint);
        }
        catch(Exception e)
        {
          Jotunn.Logger.LogError($"Failed to load blueprint: {blueprintPath}");
          Jotunn.Logger.LogError(e);
        }
      }
      biomeRuins.Add(biome, blueprints);
      Jotunn.Logger.LogInfo($"[TheRuins] Loaded {blueprints.Count} blueprints/vbuilds for {biomeName} biome");
    }

    public static void LoadAll()
    {
      Array allBiomes = Enum.GetValues(typeof(Heightmap.Biome));
      foreach (var biome in allBiomes.Cast<Heightmap.Biome>())
      {
        LoadForBiome(biome);
      }
    }
  }
}

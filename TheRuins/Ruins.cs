using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Heinermann.TheRuins
{
  internal class Ruin
  {
    Blueprint blueprint;
    Heightmap.Biome biome;

    public Ruin(Blueprint blueprint, Heightmap.Biome biome)
    {
      this.blueprint = blueprint;
      this.biome = biome;
    }

    List<Vector3> cookingPositionCache = null;
    private bool NearCookingStation(PieceEntry checkPiece)
    {
      if (cookingPositionCache == null)
      {
        cookingPositionCache = new List<Vector3>();

        foreach (PieceEntry piece in blueprint.Pieces)
        {
          switch(piece.prefabName)
          {
            case "piece_cookingstation":
            case "piece_cookingstation_iron":
            case "piece_cauldron":
            case "cauldron_ext1_spice":
            case "cauldron_ext3_butchertable":
            case "cauldron_ext4_pots":
              cookingPositionCache.Add(piece.position);
              break;
          }
        }
      }

      foreach(Vector3 v in cookingPositionCache)
      {
        var diff = v - checkPiece.position;
        diff.y = 0;

        if (Mathf.Abs(v.y - checkPiece.position.y) < 4 && diff.sqrMagnitude < 7*7)
          return true;
      }
      return false;
    }

    private string GetPickableTreasure(PieceEntry piece)
    {
      if (NearCookingStation(piece))
      {
        return "Pickable_RandomFood";
      }
      else if (biome == Heightmap.Biome.Meadows || biome == Heightmap.Biome.BlackForest)
      {
        return "Pickable_DolmenTreasure";
      }
      return "Pickable_ForestCryptRandom";
    }

    private string GetWoodTreasureChest()
    {
      switch (biome)
      {
        case Heightmap.Biome.Meadows:
          return "TreasureChest_meadows";
        case Heightmap.Biome.Swamp:
          return "TreasureChest_swamp";
        case Heightmap.Biome.Mountain:
        case Heightmap.Biome.DeepNorth:
          return "TreasureChest_mountains";
        case Heightmap.Biome.BlackForest:
          return "TreasureChest_blackforest";
        case Heightmap.Biome.Plains:
          return "TreasureChest_heath";
        case Heightmap.Biome.AshLands:
          return "TreasureChest_meadows_buried";  // Has silver necklaces and fire arrows
        default:
          return "shipwreck_karve_chest";
      }
    }

    private string GetStoneTreasureChest()
    {
      // TreasureChest_fCrypt: Feathers, Flint arrows, coins, Amber
      // TreasureChest_forestcrypt: Feathers, Flint arrows, coins, Amber, Ruby
      // TreasureChest_plains_stone: Feathers, obsidian arrows, silver necklace, coins, goblin totem
      // TreasureChest_sunkencrypt: WitheredBone, Iron or Poison arrows, coins, amber, pearl, ruby, chain, ancient bark, iron scrap
      // TreasureChest_trollcave: Wood, stone, ruby, coins, deer hide, bones, leather scraps
      switch (biome)
      {
        case Heightmap.Biome.Meadows:
        case Heightmap.Biome.BlackForest:
          return "TreasureChest_fCrypt";
        case Heightmap.Biome.Swamp:
        case Heightmap.Biome.Mountain:
        case Heightmap.Biome.DeepNorth:
          return "TreasureChest_forestcrypt";
        case Heightmap.Biome.Plains:
          return "TreasureChest_plains_stone";
        default:
          return "TreasureChest_forestcrypt";
      }
    }

    private string GetReplacementPrefabName(PieceEntry piece)
    {
      switch (piece.prefabName)
      {
        case "piece_groundtorch_green":
          return "CastleKit_groundtorch_green";
        case "piece_groundtorch":
          return "CastleKit_groundtorch";
        case "sign":
          return "sign_notext";
        case "bed":
          return "goblin_bed";
        // This doesn't work out well because gates can be surrounded by iron bars and then just be awkwardly free standing.
        //case "iron_grate":
        //  return "dungeon_sunkencrypt_irongate";
        case "itemstandh":
          return GetPickableTreasure(piece);
        case "loot_chest_wood":
        case "piece_chest_wood":
        case "piece_chest":
        case "piece_chest_blackmetal":
        case "piece_chest_private":
          return GetWoodTreasureChest();
        case "stonechest":
        case "loot_chest_stone":
          return GetStoneTreasureChest();
        default:
          return piece.prefabName;
      }
    }

    // A step that makes replacements to allow additional pieces and loot spawns.
    private void MakeInitialReplacements()
    {
      foreach (PieceEntry piece in blueprint.Pieces)
      {
        piece.prefabName = GetReplacementPrefabName(piece);
      }
    }

    // Also serves as material whitelist
    // Items without recipes get ??% chance
    static readonly Dictionary<string, float> materialSpawnChance = new Dictionary<string, float>()
    {
      {"Wood", 0.6f},
      {"RoundLog", 0.8f},
      {"FineWood", 0.3f},
      {"Resin", 0.8f},
      {"Tar", 0.7f},
      {"GreydwarfEye", 0.5f},
      {"Stone", 0.9f},
      {"Coal", 0.5f},
      {"Flint", 0.7f},
      {"LeatherScraps", 0.7f},
      {"DeerHide", 0.6f},
      {"Chain", 0.2f},
      {"Raspberry", 0.2f},
      {"Blueberries", 0.2f},
      {"Cloudberry", 0.2f},
      {"Bloodbag", 0.2f},
      {"Guck", 0.2f},
      {"IronNails", 0.2f},
      {"BronzeNails", 0.5f},
      {"Dandelion", 0.4f},
      {"Mushroom", 0.4f},
      {"MushroomBlue", 0.4f},
      {"MushroomYellow", 0.4f},
      {"Thistle", 0.4f},
      {"Carrot", 0.2f},
      {"Turnip", 0.2f}
    };

    static HashSet<string> blacklistedPieces = new HashSet<string>() {
          "piece_gift1", "piece_gift2", "piece_gift3", "piece_xmastree", "piece_jackoturnip"
        };
    private bool IsBlackListedPiece(PieceEntry piece)
    {
      GameObject prefab = piece.prefab();

      // Named blacklist
      if (blacklistedPieces.Contains(piece.prefabName)) return true;

      // Blacklist some piece types to prevent interaction and advancement for the player
      if (prefab.GetComponent("CraftingStation") ||
        prefab.GetComponent("Bed") ||
        prefab.GetComponent("TeleportWorld") ||
        prefab.GetComponent("PrivateArea") ||
        prefab.GetComponent("Beehive") ||
        prefab.GetComponent("Ship"))
      {
        return true;
      }

      // Blacklist pieces built with certain items to prevent the player from obtaining them early
      Piece pieceData = prefab.GetComponent<Piece>();
      if (pieceData)
      {
        foreach (var requirement in pieceData.m_resources)
        {
          if (!materialSpawnChance.ContainsKey(requirement.m_resItem.name))
            return true;
        }
      }

      return false;
    }

    private void RemoveBlacklisted()
    {
      blueprint.Pieces.RemoveAll(IsBlackListedPiece);
      // TODO: Review items and apply biome blacklisting
    }

    private float? cachedMaxBuildRadius = null;
    private float GetMaxBuildRadius()
    {
      if (cachedMaxBuildRadius == null)
      {
        float result = 0;
        foreach (PieceEntry piece in blueprint.Pieces)
        {
          result = Mathf.Max(Vector2.SqrMagnitude(new Vector2(piece.position.x, piece.position.z)), result);
        }
        cachedMaxBuildRadius = Mathf.Sqrt(result);
      }
      return cachedMaxBuildRadius.Value;
    }

    // i.e. disallow excess treasure chests based on build size
    private void PruneQuantities()
    {
      // Get all treasure chest and piackable treasures
      List<PieceEntry> treasureChests = new List<PieceEntry>();
      List<PieceEntry> pickableTreasure = new List<PieceEntry>();
      foreach (PieceEntry piece in blueprint.Pieces)
      {
        GameObject prefab = piece.prefab();
        if (prefab.GetComponent("Container"))
        {
          treasureChests.Add(piece);
        }
        else if (prefab.GetComponent("PickableItem"))
        {
          pickableTreasure.Add(piece);
        }
      }

      // Find the amount we want to keep
      int numDesiredChests = Math.Min((int)(Math.Sqrt(GetMaxBuildRadius()) / 2), treasureChests.Count);
      int numDesiredPickables = Math.Min((int)Math.Sqrt(GetMaxBuildRadius()), pickableTreasure.Count);

      // Whitelist some random chests that we're going to keep in the build
      for (int i = 0; i < numDesiredChests; ++i)
      {
        treasureChests.RemoveAt(UnityEngine.Random.Range(0, treasureChests.Count - 1));
      }
      for (int i = 0; i < numDesiredPickables; ++i)
      {
        pickableTreasure.RemoveAt(UnityEngine.Random.Range(0, pickableTreasure.Count - 1));
      }

      // Get rid of the others
      HashSet<PieceEntry> remChests = new HashSet<PieceEntry>(treasureChests);
      HashSet<PieceEntry> remPickable = new HashSet<PieceEntry>(pickableTreasure);
      blueprint.Pieces.RemoveAll(remChests.Contains);
      blueprint.Pieces.RemoveAll(remPickable.Contains);
    }

    private GameObject RebuildBlueprint()
    {
      GameObject prefab = PrefabManager.Instance.CreateEmptyPrefab(blueprint.Name, false); // new GameObject(blueprint.Name);
      GameObject.DestroyImmediate(prefab.GetComponent("MeshRenderer"));
      GameObject.DestroyImmediate(prefab.GetComponent("BoxCollider"));
      GameObject.DestroyImmediate(prefab.GetComponent("MeshFilter"));
      var pieceCounts = new Dictionary<string, int>();

      foreach (PieceEntry piece in blueprint.Pieces)
      {
        GameObject piecePrefab = piece.prefab();
        if (piecePrefab == null) continue;

        if (!pieceCounts.ContainsKey(piece.prefabName))
          pieceCounts.Add(piece.prefabName, 0);

        GameObject pieceObj = GameObject.Instantiate(piecePrefab, prefab.transform, false);
        pieceObj.transform.position = piece.position;
        pieceObj.transform.rotation = piece.rotation;
        pieceObj.name = $"{piece.prefabName} ({pieceCounts[piece.prefabName]})";

        pieceCounts[piece.prefabName]++;
        /*
        if (pieceObj.GetComponent<ZNetView>() == null)
        {
          var znetview = pieceObj.AddComponent<ZNetView>();
          znetview.m_persistent = true;
        }*/
      }

      return prefab;
    }

    private float GetSpawnChance(GameObject prefab)
    {
      Piece piece = prefab.GetComponent<Piece>();

      if (piece == null || piece.m_resources == null || piece.m_resources.Length == 0) return 0.5f;

      float spawnChance = 0;
      foreach (var requirement in piece.m_resources)
      {
        float value = 0.5f;
        materialSpawnChance.TryGetValue(requirement.m_resItem.name, out value);
        spawnChance += value;
      }
      return spawnChance / piece.m_resources.Length;
    }

    private PickableItem.RandomItem[] PrefabStringsToItems(params string[] items)
    {
      return items.Select(item => new PickableItem.RandomItem() {
        m_itemPrefab = PrefabManager.Instance.GetPrefab(item).GetComponent<ItemDrop>(),
        m_stackMin = 1,
        m_stackMax = 1
      }).ToArray();
    }

    private PickableItem.RandomItem[] GetBiomeTrophies()
    {
      switch (biome)
      {
        case Heightmap.Biome.Meadows:
          return PrefabStringsToItems("TrophyNeck", "TrophyBoar");
        case Heightmap.Biome.Swamp:
          return PrefabStringsToItems("TrophySkeleton", "TrophyLeech", "TrophyDraugr", "TrophyBlob");
        case Heightmap.Biome.Mountain:
          return PrefabStringsToItems("TrophySkeleton", "TrophyDraugr", "TrophyWolf", "TrophyFenring");
        case Heightmap.Biome.BlackForest:
          return PrefabStringsToItems("TrophyGreydwarf", "TrophyGreydwarfBrute", "TrophyGreydwarfShaman");
        case Heightmap.Biome.Plains:
          return PrefabStringsToItems("TrophyDeathsquito", "TrophyLox", "TrophyGrowth", "TrophyGoblin");
        case Heightmap.Biome.AshLands:
          return PrefabStringsToItems("TrophySurtling");
      }
      return null;
    }

    private void RuinPrefab(GameObject prefab)
    {
      // Add the RandomSpawn component
      foreach (var wear in prefab.GetComponentsInChildren<WearNTear>())
      {
        if (wear.gameObject.GetComponent<RandomSpawn>()) continue;

        var randomSpawn = wear.gameObject.AddComponent<RandomSpawn>();
        randomSpawn.m_chanceToSpawn = GetSpawnChance(prefab);
      }

      // Remove fireplace fuel
      foreach (var fireplace in prefab.GetComponentsInChildren<Fireplace>())
      {
        fireplace.m_startFuel = 0;
      }

      // Populate item/armorstands
      foreach (var armorStand in prefab.GetComponentsInChildren<ArmorStand>())
      {
        // TODO Needs some kind of RandomItems component
      }

      // Populate itemstands
      foreach(var itemStand in prefab.GetComponentsInChildren<ItemStand>())
      {
        // TODO Need to have a RandomObject component to switch stuff out
        //var itemPool = GetBiomeTrophies();
      }
    }

    private void AddFoliage()
    {
      if (biome == Heightmap.Biome.AshLands ||
        biome == Heightmap.Biome.DeepNorth ||
        biome == Heightmap.Biome.Mountain ||
        biome == Heightmap.Biome.Ocean) return;

      // TODO (vines, saplings, bushes, roots, trees; determine where there is open ground)
    }

    private void AddBeeHives()
    {
      if (biome != Heightmap.Biome.Meadows) return;
      // TODO
    }

    private void AddMobs()
    {
      // TODO (determine free-placed mobs vs visible spawner vs invisible spawner)
    }

    private void CreateLocation(GameObject prefab)
    {
      LocationConfig config = new LocationConfig()
      {
        Biome = biome,
        ExteriorRadius = GetMaxBuildRadius(),
        Group = blueprint.Name,
        MaxTerrainDelta = 10,
        MinAltitude = 0,
        Quantity = 200,
        RandomRotation = true,
        ClearArea = true
      };
      CustomLocation location = new CustomLocation(prefab, false, config);
      location.Location.m_applyRandomDamage = true;

      ZoneManager.Instance.AddCustomLocation(location);
    }

    public void FullyRuinBlueprintToLocation()
    {
      MakeInitialReplacements();
      RemoveBlacklisted();
      PruneQuantities();
      AddFoliage();
      AddBeeHives();
      AddMobs();

      GameObject prefab = RebuildBlueprint();

      RuinPrefab(prefab);
      CreateLocation(prefab);
    }
  }

  internal static class Ruins
  {
    static Dictionary<Heightmap.Biome, List<Blueprint>> biomeRuins = new Dictionary<Heightmap.Biome, List<Blueprint>>();

    public static void RegisterRuins()
    {
      foreach(var biome in biomeRuins)
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
        blueprints.Add(Blueprint.FromFile(blueprintPath));
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

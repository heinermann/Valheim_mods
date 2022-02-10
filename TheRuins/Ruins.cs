using Heinermann.UnityExtensions;
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

        if (Mathf.Abs(v.y - checkPiece.position.y) < 2 && diff.sqrMagnitude < 9*9)
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
        case "CargoCrate":
          return "CastleKit_braided_box01";
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
      {"Wood", 60f},
      {"RoundLog", 70f},
      {"FineWood", 30f},
      {"Resin", 60f},
      {"Tar", 60f},
      {"GreydwarfEye", 50f},
      {"Stone", 80f},
      {"Coal", 50f},
      {"Flint", 50f},
      {"LeatherScraps", 50f},
      {"DeerHide", 30f},
      {"Chain", 10f},
      {"Raspberry", 20f},
      {"Blueberries", 20f},
      {"Cloudberry", 20f},
      {"Bloodbag", 20f},
      {"Guck", 20f},
      {"IronNails", 10f},
      {"BronzeNails", 20f},
      {"Dandelion", 30f},
      {"Mushroom", 30f},
      {"MushroomBlue", 30f},
      {"MushroomYellow", 30f},
      {"Thistle", 30f}
    };

    static HashSet<string> blacklistedPieces = new HashSet<string>() {
      "piece_gift1", "piece_gift2", "piece_gift3", "piece_xmastree", "piece_jackoturnip", "piece_groundtorch_wood"
    };

    private bool IsBlackListedPiece(PieceEntry piece)
    {
      GameObject prefab = piece.prefab();

      if (prefab == null)
      {
        Jotunn.Logger.LogWarning($"Prefab not found: {piece.prefabName}");
        return true;
      }

      // Named blacklist
      if (blacklistedPieces.Contains(piece.prefabName)) return true;

      // Blacklist some piece types to prevent interaction and advancement for the player
      // TODO Carts are bugged because it RandomSpawns the Container
      if (prefab.HasAnyComponent("CraftingStation", "Bed", "TeleportWorld", "PrivateArea", "Beehive", "Smelter", "Vagon", "Ship"))
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

    private static float SqrMagnitude2D(PieceEntry piece)
    {
      return Vector2.SqrMagnitude(new Vector2(piece.position.x, piece.position.z));
    }

    private float? cachedMaxBuildRadius = null;
    private float GetMaxBuildRadius()
    {
      if (cachedMaxBuildRadius == null)
      {
        float result = blueprint.Pieces
          .Where(piece => piece.prefab()?.GetComponent("WearNTear"))
          .Max(SqrMagnitude2D);

        cachedMaxBuildRadius = Mathf.Sqrt(result);
      }
      return cachedMaxBuildRadius.Value;
    }

    private float? cachedFlattenRadius = null;
    private float GetFlattenRadius()
    {
      if (cachedFlattenRadius == null)
      {
        float result = blueprint.Pieces
          .Where(piece => piece.position.y <= 1f && piece.prefab()?.GetComponent("WearNTear"))
          .Max(SqrMagnitude2D);

        cachedFlattenRadius = Mathf.Sqrt(result);
      }
      return cachedFlattenRadius.Value;
    }

    private float GetPieceRadius(GameObject prefab)
    {
      var collider = prefab.GetComponentInChildren<Collider>();
      if (collider)
      {
        return Mathf.Max(collider.bounds.size.x, collider.bounds.size.z, 1f);
      }
      Jotunn.Logger.LogInfo($"No collider on {prefab.name}");
      return 1f;
    }

    private float GetPieceHeight(GameObject prefab)
    {
      var collider = prefab.GetComponentInChildren<Collider>();
      if (collider)
      {
        return collider.bounds.size.y;
      }
      Jotunn.Logger.LogInfo($"No collider on {prefab.name}");
      return 0.25f;
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

        if (piece.prefabName == "itemstand")
        {
          pieceObj.AddComponent<RandomItemStand>();
        }

        /* NOT WORKING
        if (pieceObj.GetComponent("Door"))
        {
          pieceObj.AddComponent<RandomDoor>();
        }
        */

        pieceCounts[piece.prefabName]++;
      }

      return prefab;
    }

    private float GetSpawnChance(GameObject prefab)
    {
      Piece piece = prefab.GetComponent<Piece>();

      if (piece == null || piece.m_resources == null || piece.m_resources.Length == 0) return 50f;

      float spawnChance = 0;
      foreach (var requirement in piece.m_resources)
      {
        float value = 50f;
        materialSpawnChance.TryGetValue(requirement.m_resItem.name, out value);
        spawnChance += value;
      }
      return spawnChance / piece.m_resources.Length;
    }

    private void RuinPrefab(GameObject prefab)
    {
      List<RandomSpawn> beehiveSpawns = new List<RandomSpawn>();

      // Add the RandomSpawn component
      foreach (var wear in prefab.GetComponentsInChildren<WearNTear>())
      {
        if (wear.gameObject.GetComponent<RandomSpawn>()) continue;

        var randomSpawn = wear.gameObject.AddComponent<RandomSpawn>();
        if (wear.name.StartsWith("Beehive"))
        {
          beehiveSpawns.Add(randomSpawn);
        }
        else
        {
          float heightBias = 1f - Mathf.Clamp(wear.transform.position.y + 1f, 0, 10f) / 10f;
          float spawnChance = GetSpawnChance(wear.gameObject);
          float spawnChanceBias = (100f - spawnChance) * heightBias;

          randomSpawn.m_chanceToSpawn = spawnChance + spawnChanceBias;
        }
      }

      // Remove fireplace fuel
      foreach (var fireplace in prefab.GetComponentsInChildren<Fireplace>())
      {
        fireplace.m_startFuel = 0;
      }

      foreach (var beehive in beehiveSpawns)
      {
        // In vanilla locations, Beehives have a 25% chance to spawn in 11/15 of the locations (overall 18% excluding dungeons)
        // Will use 15% since there will be more total locations in the world (and therefore more opportunities to spawn bees)
        beehive.m_chanceToSpawn = 15f / beehiveSpawns.Count;
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

    // TODO: Naiive implementation, just under any random piece with "roof_top" in the name
    private void AddBeeHives()
    {
      if (biome != Heightmap.Biome.Meadows) return;

      List<PieceEntry> beehivesToAdd = new List<PieceEntry>();
      foreach (var piece in blueprint.Pieces)
      {
        if (!piece.prefabName.Contains("roof_top") || piece.prefabName.Contains("wall")) continue;

        var position = piece.position;
        position.y -= 0.2f;
        beehivesToAdd.Add(new PieceEntry("Beehive", position));
      }
      blueprint.Pieces.AddRange(beehivesToAdd);
    }

    private void AddMobs()
    {
      // TODO (determine free-placed mobs vs visible spawner vs invisible spawner)
    }

    private float GetTreasureDistributionChance(int numObjects)
    {
      float buildRadius = GetMaxBuildRadius();
      return (100f / numObjects) * Mathf.Sqrt(buildRadius);
    }

    private void DistributeTreasureChestProbabilities(GameObject prefab)
    {
      var treasureChests = prefab.GetComponentsInChildren<Container>();
      float chestSpawnChance = GetTreasureDistributionChance(treasureChests.Length) / 2f;
      foreach (Container treasureChest in treasureChests)
      {
        var spawn = treasureChest.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = chestSpawnChance;
      }
    }

    private void AddFlies(GameObject prefab, Vector3 position, string name, float spawnChance)
    {
      GameObject fliesPrefab = PrefabManager.Instance.GetPrefab("Flies");
      if (fliesPrefab == null) return;

      GameObject flies = GameObject.Instantiate(fliesPrefab, prefab.transform, false);
      flies.transform.position = position;
      flies.name += $"Flies_{name}";
      flies.AddComponent<RandomSpawn>().m_chanceToSpawn = spawnChance;
    }

    private void DistributePickableProbabilities(GameObject prefab)
    {
      var pickableTreasures = prefab.GetComponentsInChildren<PickableItem>();
      float pickableSpawnChance = GetTreasureDistributionChance(pickableTreasures.Length) / 2f;

      foreach (PickableItem pickable in pickableTreasures)
      {
        var spawn = pickable.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = pickableSpawnChance;

        // Randomly add flies for food
        if (pickable.name.StartsWith("Pickable_RandomFood"))
          AddFlies(prefab, pickable.transform.position, pickable.name, pickableSpawnChance);
      }
    }

    private void DistributeItemStandProbabilities(GameObject prefab)
    {
      var itemStands = prefab.GetComponentsInChildren<ItemStand>();
      float standSpawnChance = GetTreasureDistributionChance(itemStands.Length);
      foreach (ItemStand stand in itemStands)
      {
        var spawn = stand.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = standSpawnChance;
      }
    }

    // Applies random spawn chances to pickable treasures, and adds flies to pickable food spawns
    private void DistributeTreasures(GameObject prefab)
    {
      DistributeTreasureChestProbabilities(prefab);
      DistributePickableProbabilities(prefab);
      DistributeItemStandProbabilities(prefab);
    }

    private float GetMaxTerrainDelta()
    {
      switch (biome)
      {
        case Heightmap.Biome.Mountain:
          return 6f;
        case Heightmap.Biome.BlackForest:
          return 3f;
        default:
          return 2f;
      }
    }

    private void CreateLocation(GameObject prefab)
    {
      LocationConfig config = new LocationConfig()
      {
        Biome = biome,
        ExteriorRadius = GetMaxBuildRadius(),
        Group = blueprint.Name,
        MaxTerrainDelta = GetMaxTerrainDelta(),
        MinAltitude = 0.5f,
        Quantity = 100,
        RandomRotation = true,
        ClearArea = true,
      };
      CustomLocation location = new CustomLocation(prefab, false, config);
      location.Location.m_applyRandomDamage = true;
      location.Location.m_noBuild = false;

      ZoneManager.Instance.AddCustomLocation(location);
    }

    private float? cachedLowestPieceOffset = null;
    private float LowestOffset()
    {
      if (cachedLowestPieceOffset == null)
      {
        cachedLowestPieceOffset = blueprint.Pieces.Min(piece => piece.position.y);
      }
      return cachedLowestPieceOffset.Value;
    }

    private GameObject CreateTerrainModifierPrefab(GameObject parent)
    {
      // TODO: Should be in asset bundle
      GameObject prefab = new GameObject("Terrain_Mod_Prefab");
      var terrain = prefab.AddComponent<TerrainModifier>();
      terrain.m_sortOrder = 0;
      terrain.m_square = false;
      terrain.m_paintCleared = false;
      terrain.m_playerModifiction = false;
      terrain.m_spawnAtMaxLevelDepth = true;

      var znet = prefab.AddComponent<ZNetView>();
      znet.m_persistent = true;
      znet.m_type = ZDO.ObjectType.Default;

      GameObject result = GameObject.Instantiate(prefab, parent.transform, false);
      result.transform.position = Vector3.zero;
      result.transform.rotation = Quaternion.identity;
      return result;
    }

    private TerrainModifier FlattenAreaAt(GameObject prefab, float relativeTargetLevel, Vector3 position, float radius, string name)
    {
      GameObject pieceObj = CreateTerrainModifierPrefab(prefab);
      pieceObj.transform.position = position;
      pieceObj.name = name;

      var modifier = pieceObj.GetComponent<TerrainModifier>();
      modifier.m_level = true;
      modifier.m_levelRadius = radius;
      modifier.m_levelOffset = relativeTargetLevel;

      modifier.m_sortOrder = Mathf.RoundToInt(-(position.y + relativeTargetLevel) * 10f);

      modifier.m_smooth = true;
      modifier.m_smoothPower = 3f;
      modifier.m_smoothRadius = radius + 2f;
      return modifier;
    }

    private TerrainModifier FlattenArea(GameObject prefab, float relativeTargetLevel)
    {
      TerrainModifier modifier = FlattenAreaAt(prefab, relativeTargetLevel, Vector3.zero, GetFlattenRadius() + 2f, "TheRuins_Flatten_Area");
      modifier.m_smooth = true;
      modifier.m_smoothPower = 3f;
      modifier.m_smoothRadius = modifier.m_levelRadius + 4f;
      return modifier;
    }

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

    // Granularity of below algorithms, higher = more fidelity but more terrain modifier instances
    const float terrainGranularity = 2f;

    private void CompareComponentHeights(Dictionary<Tuple<int, int>, LevelData> lowestPositions, WearNTear wear, int x, int z)
    {
      var point = Tuple.Create(x, z);

      // TODO optimize this so it isn't recalc'd
      float thisY = wear.transform.position.y - GetPieceHeight(wear.gameObject) / 2f;

      LevelData compareWith;
      if (lowestPositions.TryGetValue(point, out compareWith))
      {
        if (thisY < compareWith.position.y)
          lowestPositions[point] = new LevelData(x / terrainGranularity, thisY, z / terrainGranularity, wear);
      }
      else
      {
        lowestPositions.Add(point, new LevelData(x / terrainGranularity, thisY, z / terrainGranularity, wear));
      }
    }

    // Naiive solution, doesn't consider geometries (i.e. 4-long stone wall) and has fairly major rounding errors to integer
    private void PrepareTerrainModifiers(GameObject prefab)
    {
      // Get the leveling data from each position
      var lowestPositions = new Dictionary<Tuple<int, int>, LevelData>();
      foreach (WearNTear wear in prefab.GetComponentsInChildren<WearNTear>(includeInactive: true))
      {
        int x = Mathf.RoundToInt(wear.transform.position.x * terrainGranularity);
        int z = Mathf.RoundToInt(wear.transform.position.z * terrainGranularity);
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

      TerrainModifier areaFlatten = FlattenArea(prefab, targetY);
      areaFlatten.m_sortOrder = -1000;

      // Flatten areas with pieces in it
      int index = 0;
      foreach (LevelData lowest in lowestPositions.Values)
      {
        if (lowest.position.y > 1f) continue;

        float radius = 2f / terrainGranularity;
        TerrainModifier mod = FlattenAreaAt(prefab, 0, lowest.position, radius, $"TheRuins_Flatten_{index}");
        if (lowest.component.m_materialType == WearNTear.MaterialType.Stone || lowest.component.GetComponent("Fireplace"))
        {
          mod.m_paintCleared = true;
          mod.m_paintRadius = GetPieceRadius(lowest.component.gameObject) + 0.5f;
          mod.m_paintType = TerrainModifier.PaintType.Dirt;
        }
        ++index;
      }

      // TODO smooth surrounding border
    }

    public void FullyRuinBlueprintToLocation()
    {
      MakeInitialReplacements();
      RemoveBlacklisted();
      AddFoliage();
      AddBeeHives();
      AddMobs();

      GameObject prefab = RebuildBlueprint();
      prefab.AddComponent<LocationSettling>();

      RuinPrefab(prefab);
      DistributeTreasures(prefab);
      PrepareTerrainModifiers(prefab);

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
        Blueprint blueprint = Blueprint.FromFile(blueprintPath);
        blueprints.Add(blueprint);
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

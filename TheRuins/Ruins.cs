using Heinermann.TheRuins.Components;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
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
      {"Thistle", 30f},
      {"BoneFragments", 50f},
      {"JuteRed", 10f}
    };

    static HashSet<string> blacklistedPieces = new HashSet<string>() {
      "SmokeBall", "piece_gift1", "piece_gift2", "piece_gift3", "piece_xmastree", "piece_jackoturnip", "piece_groundtorch_wood"
    };

    private bool CheckIsBlacklistedPiece(PieceEntry piece)
    {
      // Named blacklist
      if (blacklistedPieces.Contains(piece.prefabName)) return true;

      GameObject prefab = piece.Prefab();
      if (prefab == null)
      {
        Jotunn.Logger.LogWarning($"Prefab not found: {piece.prefabName}");
        return true;
      }

      // Blacklist some piece types to prevent interaction and advancement for the player
      // TODO Carts are bugged because it RandomSpawns the Container
      if (prefab.HasAnyComponent("CraftingStation", "Bed", "TeleportWorld", "PrivateArea", "Beehive", "Smelter", "Vagon", "Ship", "CookingStation"))
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

    private static Dictionary<string, bool> blacklistedPieceCache = new Dictionary<string, bool>();
    private bool IsBlacklistedPiece(PieceEntry piece)
    {
      bool result;
      if (blacklistedPieceCache.TryGetValue(piece.prefabName, out result))
      {
        return result;
      }

      result = CheckIsBlacklistedPiece(piece);
      blacklistedPieceCache.Add(piece.prefabName, result);
      return result;
    }

    private void RemoveBlacklisted()
    {
      blueprint.Pieces.RemoveAll(IsBlacklistedPiece);
      // TODO: Review items and apply biome blacklisting
    }

    private GameObject RebuildBlueprint()
    {
      GameObject prefab = PrefabManager.Instance.CreateEmptyPrefab(blueprint.UniqueName, false); // new GameObject(blueprint.Name);
      GameObject.DestroyImmediate(prefab.GetComponent("MeshRenderer"));
      GameObject.DestroyImmediate(prefab.GetComponent("BoxCollider"));
      GameObject.DestroyImmediate(prefab.GetComponent("MeshFilter"));

      var pieceCounts = new Dictionary<string, int>();
      foreach (PieceEntry piece in blueprint.Pieces)
      {
        GameObject piecePrefab = piece.Prefab();
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

        if (pieceObj.GetComponent("Door"))
        {
          pieceObj.AddComponent<RandomDoor>();
        }

        if (pieceObj.GetComponent("WearNTear"))
        {
          // TODO fix infinite loop first
          //pieceObj.AddComponent<StructuralPiece>();
        }

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

    private float GetMaxTerrainDelta()
    {
      switch (biome)
      {
        case Heightmap.Biome.Mountain:
          return 6f;
        case Heightmap.Biome.BlackForest:
          return 4f;
        default:
          return 3f;
      }
    }

    private float GetMinAltitude()
    {
      switch (biome)
      {
        case Heightmap.Biome.Swamp:
          return -0.5f;
        default:
          return 1f;
      }
    }

    private void CreateLocation(GameObject prefab)
    {
      LocationConfig config = new LocationConfig()
      {
        Biome = biome,
        ExteriorRadius = blueprint.GetMaxBuildRadius(),
        Group = blueprint.Name,
        MaxTerrainDelta = GetMaxTerrainDelta(),
        MinAltitude = GetMinAltitude(),
        Quantity = 25,
        RandomRotation = true,
        ClearArea = false,
        CenterFirst = true,
        MinDistanceFromSimilar = 250,
      };
      CustomLocation location = new CustomLocation(prefab, false, config);
      location.Location.m_applyRandomDamage = true;
      location.Location.m_noBuild = false;

      ZoneManager.Instance.AddCustomLocation(location);
    }

    public void FullyRuinBlueprintToLocation()
    {
      MakeInitialReplacements();
      RemoveBlacklisted();
      AddFoliage();
      AddBeeHives();
      AddMobs();

      GameObject prefab = RebuildBlueprint();

      RuinPrefab(prefab);
      new TreasureDistributor(prefab, blueprint.GetMaxBuildRadius()).DistributeTreasures();
      TerrainFlattener.PrepareTerrainModifiers(prefab);

      CreateLocation(prefab);
    }
  }
}

using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.TheRuins
{
  internal class Ruin
  {
    Blueprint blueprint;
    Heightmap.Biome biome;

    List<Vector3> cookingPositionCache = null;

    private bool NearCookingStation(PieceEntry checkPiece)
    {
      if (cookingPositionCache == null)
      {
        cookingPositionCache = new List<Vector3>();

        foreach (PieceEntry piece in blueprint.pieces)
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
        {
          return true;
        }
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

    // A step that makes replacements to allow additional pieces and loot spawns.
    private void MakeInitialReplacements()
    {
      foreach (PieceEntry piece in blueprint.pieces)
      {
        switch(piece.prefabName)
        {
          case "piece_groundtorch_green":
            piece.prefabName = "CastleKit_groundtorch_green";
            break;
          case "piece_groundtorch":
            piece.prefabName = "CastleKit_groundtorch";
            break;
          case "sign":
            piece.prefabName = "sign_notext";
            break;
          case "bed":
            piece.prefabName = "goblin_bed";
            break;
          // This doesn't work out well because gates can be surrounded by iron bars and then just be awkwardly free standing.
          //case "iron_grate":
          //  piece.prefabName = "dungeon_sunkencrypt_irongate";
          //  break;
          case "itemstandh":
            piece.prefabName = GetPickableTreasure(piece);
            break;
          case "loot_chest_wood":
          case "piece_chest_wood":
          case "piece_chest":
          case "piece_chest_blackmetal":
          case "piece_chest_private":
            piece.prefabName = GetWoodTreasureChest();
            break;
          case "stonechest":
          case "loot_chest_stone":
            piece.prefabName = GetStoneTreasureChest();
            break;
        }
      }
    }

    static HashSet<string> blacklistedPieces = null;

    // Also serves as material whitelist
    // Items without recipes get ??% chance
    static readonly Dictionary<string, double> materialDestructionChance = new Dictionary<string, double>()
    {
      {"Wood", 0.2},
      {"RoundLog", 0.1},
      {"FineWood", 0.6},
      {"Resin", 0.2},
      {"Tar", 0.2},
      {"GreydwarfEye", 0.5},
      {"Stone", 0.05},
      {"Coal", 0.4},
      {"Flint", 0.3},
      {"LeatherScraps", 0.3},
      {"DeerHide", 0.35},
      {"Chain", 0.8},
      {"Raspberry", 0.8},
      {"Blueberries", 0.8},
      {"Cloudberry", 0.8},
      {"Bloodbag", 0.8},
      {"Guck", 0.8},
      {"IronNails", 0.8},
      {"BronzeNails", 0.5},
      {"Dandelion", 0.6},
      {"Mushroom", 0.6},
      {"MushroomBlue", 0.6},
      {"MushroomYellow", 0.6},
      {"Thistle", 0.6},
      {"Carrot", 0.8},
      {"Turnip", 0.8}
    };

    private bool IsBlackListedPiece(PieceEntry piece)
    {
      if (blacklistedPieces == null)
      {
        blacklistedPieces = new HashSet<string>() {
          "piece_gift1", "piece_gift2", "piece_gift3", "piece_xmastree", "piece_jackoturnip"
        };

        // TODO: iterate prefabs and generate blacklist based on whitelisted materials
        // Also blacklist crafting stations (CraftingStation component), beds (Bed component), and anything that requires the artisan table (piece_artisanstation)
        // Blacklist components:
        // - CraftingStation
        // - Bed
        // - TeleportWorld
        // - PrivateArea
        // - Ship
      }
      return blacklistedPieces.Contains(piece.prefabName);
    }

    private void RemoveBlacklisted()
    {
      blueprint.pieces.RemoveAll(IsBlackListedPiece);
      // TODO: Review items and maybe apply biome blacklisting
    }

    private void GetMaxBuildRadius()
    {
      // TODO
    }

    // i.e. disallow excess treasure chests based on build size
    private void PruneQuantities()
    {
      // TODO
    }

    private void CreateLocationPrefab()
    {
      // TODO
    }

    private void AddFoilage()
    {
      if (biome == Heightmap.Biome.Mountain || biome == Heightmap.Biome.DeepNorth || biome == Heightmap.Biome.AshLands) return;
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

    private void ApplyExistenceProbabilities()
    {
      // TODO
    }

    private void FinalizePrefab()
    {
      // TODO: Remove fuel from Fireplace components, populate itemstand and ArmorStand
    }

  }

  internal class Ruins
  {
    List<Blueprint> blueprints = new List<Blueprint>();

    // Replacement stage replaces objects that should be inaccessible with objects 
    private void ReplaceObjects()
    {

    }
  }
}

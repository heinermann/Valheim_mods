
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  internal static class Prefabs
  {
    // Refs:
    //  - PieceTable
    //  - PieceTable.m_pieces
    private static HashSet<string> GetPieceNames()
    {
      var result = Resources.FindObjectsOfTypeAll<PieceTable>()
        .SelectMany(pieceTable => pieceTable.m_pieces)
        .Select(piece => piece.name);

      return new HashSet<string>(result);
    }

    private static readonly HashSet<string> IgnoredPrefabs = new HashSet<string>() {
      "Player", "Valkyrie", "odin", "CargoCrate", "CastleKit_pot03", "Pickable_Item"
    };

    private static bool ShouldIgnorePrefab(GameObject prefab)
    {
      HashSet<string> prefabsToSkip = GetPieceNames();

      return
        prefab.HasAnyComponent("ItemDrop", "Projectile", "TimedDestruction", "Ragdoll", "Plant", "Fish", "FishingFloat", "RandomFlyingBird", "DungeonGenerator", "ZSFX", "MusicLocation", "LocationProxy", "MineRock5", "LootSpawner", "TombStone") ||
        (prefab.GetComponent("Aoe") && prefab.GetComponent("WearNTear") == null) ||
        (prefab.GetComponent("TerrainModifier") && prefab.GetComponent("Destructible") == null) ||
        prefab.name.StartsWith("vfx_") ||
        prefab.name.StartsWith("sfx_") ||
        prefab.name.StartsWith("fx_") ||
        prefab.name.StartsWith("_") ||
        IgnoredPrefabs.Contains(prefab.name) ||
        prefabsToSkip.Contains(prefab.name);
    }

    private static string GetPrefabCategory(GameObject prefab)
    {
      string category = "Other";
      if (prefab.GetComponent("Location"))
      {
        category = "Locations";
      }
      else if (prefab.HasAnyComponent("Pickable", "PickableItem"))
      {
        category = "Pickable";
      }
      else if (prefab.HasAnyComponent("Humanoid", "Character", "Leviathan"))
      {
        category = "Monsters";
      }
      else if (prefab.HasAnyComponent("CreatureSpawner", "SpawnArea"))
      {
        category = "Spawners";
      }
      else if (prefab.name.ContainsAny("Tree", "Oak", "Pine", "Beech", "Birch", "Bush", "Root", "root", "shrub", "stubbe") ||
        prefab.HasAnyComponent("TreeBase", "TreeLog"))
      {
        category = "Vegetation";
      }
      else if (prefab.GetComponent("WearNTear"))
      {
        category = "Building 2";
      }
      else if (prefab.HasAnyComponent("Destructible", "MineRock"))
      {
        category = "Destructible";
      }
      return category;
    }

    private static readonly HashSet<string> unrestrictedExceptions = new HashSet<string>()
    {
      "GlowingMushroom", "Flies", "horizontal_web", "tunnel_web", "rockformation1", "StatueCorgi", "StatueDeer", "StatueEvil", "StatueHare", "StatueSeed"
    };

    // Refs:
    // - Tons of members of Piece
    private static void ModifyPiece(Piece piece, bool new_piece)
    {
      if (piece == null) return;

      piece.m_enabled = true;
      piece.m_canBeRemoved = true;

      if (BetterCreative.UnrestrictedPlacement.Value ||
        piece.gameObject.HasAnyComponent("Humanoid", "Character", "Destructible", "TreeBase", "MeshCollider", "LiquidVolume", "Pickable", "PickableItem") ||
        unrestrictedExceptions.Contains(piece.name))
      {
        piece.m_clipEverything = piece.GetComponent("Floating") == null && new_piece;
      }
      
      if (BetterCreative.UnrestrictedPlacement.Value)
      {
        piece.m_groundPiece = false;
        piece.m_groundOnly = false;
        piece.m_noInWater = false;
        piece.m_notOnWood = false;
        piece.m_notOnTiltingSurface = false;
        piece.m_notOnFloor = false;
        piece.m_allowedInDungeons = true;
        piece.m_onlyInTeleportArea = false;
        piece.m_inCeilingOnly = false;
        piece.m_cultivatedGroundOnly = false;
        piece.m_onlyInBiome = Heightmap.Biome.None;
        piece.m_allowRotatedOverlap = true;
      }
    }

    private static void InitPieceData(GameObject prefab)
    {
      Piece piece = prefab.GetComponent<Piece>();
      bool is_new_piece = false;
      if (piece == null)
      {
        piece = prefab.AddComponent<Piece>();
        is_new_piece = true;
      }
      ModifyPiece(piece, is_new_piece);
    }

    // Refs:
    //  - CreatureSpawner.m_creaturePrefab
    //  - PickableItem.m_randomItemPrefabs
    //  - PickableItem.RandomItem.m_itemPrefab
    private static Sprite CreatePrefabIcon(GameObject prefab)
    {
      Sprite result = RenderManager.Instance.Render(prefab, RenderManager.IsometricRotation);
      if (result == null)
      {
        GameObject spawnedCreaturePrefab = prefab.GetComponent<CreatureSpawner>()?.m_creaturePrefab;
        if (spawnedCreaturePrefab != null)
          result = RenderManager.Instance.Render(spawnedCreaturePrefab, RenderManager.IsometricRotation);
      }

      if (result == null)
      {
        PickableItem.RandomItem[] randomItemPrefabs = prefab.GetComponent<PickableItem>()?.m_randomItemPrefabs;
        if (randomItemPrefabs != null && randomItemPrefabs.Length > 0)
        {
          GameObject item = randomItemPrefabs[0].m_itemPrefab?.gameObject;
          if (item != null)
            result = RenderManager.Instance.Render(item, RenderManager.IsometricRotation);
        }
      }
      return result;
    }

    // Refs:
    //  - DestroyComponents calls below
    //  - 
    private static void CreateGhostPrefab(GameObject prefab)
    {
      GameObject ghost = PrefabManager.Instance.CreateClonedPrefab(prefab.name + "_ghostfab", prefab);

      ghost.DestroyComponent<TreeLog>();
      ghost.DestroyComponent<TreeBase>();
      ghost.DestroyComponent<BaseAI>();
      ghost.DestroyComponent<MineRock>();
      ghost.DestroyComponent<CharacterDrop>();
      ghost.DestroyComponent<Character>();
      ghost.DestroyComponent<CharacterAnimEvent>();
      ghost.DestroyComponent<Humanoid>();
      ghost.DestroyComponent<HoverText>();
      ghost.DestroyComponent<FootStep>();
      ghost.DestroyComponent<VisEquipment>();
      ghost.DestroyComponent<ZSyncAnimation>();
      ghost.DestroyComponent<TerrainModifier>();
      ghost.DestroyComponent<GuidePoint>();
      ghost.DestroyComponent<Light>();
      ghost.DestroyComponent<LightFlicker>();
      ghost.DestroyComponent<LightLod>();
      ghost.DestroyComponent<LevelEffects>();
      ghost.DestroyComponent<AudioSource>();
      ghost.DestroyComponent<ZSFX>();
      ghost.DestroyComponent<Windmill>();
      ghost.DestroyComponent<ParticleSystem>();
      ghost.DestroyComponent<Tameable>();
      ghost.DestroyComponent<Procreation>();
      ghost.DestroyComponent<Growup>();
      ghost.DestroyComponent<SpawnArea>();
      ghost.DestroyComponent<CreatureSpawner>();
      ghost.DestroyComponent<Aoe>();
      ghost.DestroyComponent<ZSyncTransform>();
      ghost.DestroyComponent<RandomSpawn>();
      ghost.DestroyComponent<Animator>();

      // Not sure how to resolve the issue where you can't place stuff on structures.
      // So let's do some jank ass hack to work around it :)
      var chair = GameObject.Instantiate(PrefabManager.Instance.GetPrefab("piece_chair"), ghost.transform, false);
      chair.DestroyComponent<MeshRenderer>();
      chair.DestroyComponent<ZNetView>();
      chair.DestroyComponent<Piece>();
      chair.DestroyComponent<Chair>();
      chair.DestroyComponent<WearNTear>();

      PrefabManager.Instance.AddPrefab(ghost);
    }

    private static string GetPrefabFriendlyName(GameObject prefab)
    {
      HoverText hover = prefab.GetComponent<HoverText>();
      if (hover) return hover.m_text;

      ItemDrop item = prefab.GetComponent<ItemDrop>();
      if (item) return item.m_itemData.m_shared.m_name;

      Character chara = prefab.GetComponent<Character>();
      if (chara) return chara.m_name;

      RuneStone runestone = prefab.GetComponent<RuneStone>();
      if (runestone) return runestone.m_name;

      ItemStand itemStand = prefab.GetComponent<ItemStand>();
      if (itemStand) return itemStand.m_name;

      MineRock mineRock = prefab.GetComponent<MineRock>();
      if (mineRock) return mineRock.m_name;

      Pickable pickable = prefab.GetComponent<Pickable>();
      if (pickable) return GetPrefabFriendlyName(pickable.m_itemPrefab);

      CreatureSpawner creatureSpawner = prefab.GetComponent<CreatureSpawner>();
      if (creatureSpawner) return GetPrefabFriendlyName(creatureSpawner.m_creaturePrefab);

      SpawnArea spawnArea = prefab.GetComponent<SpawnArea>();
      if (spawnArea && spawnArea.m_prefabs.Count > 0) {
        return GetPrefabFriendlyName(spawnArea.m_prefabs[0].m_prefab);
      }

      Piece piece = prefab.GetComponent<Piece>();
      if (piece && !string.IsNullOrEmpty(piece.m_name)) return piece.m_name;

      return prefab.name;
    }

    private static void CreatePrefabPiece(GameObject prefab)
    {
      InitPieceData(prefab);

      var pieceConfig = new PieceConfig
      {
        Name = prefab.name,
        Description = GetPrefabFriendlyName(prefab),
        PieceTable = "_HammerPieceTable",
        Category = GetPrefabCategory(prefab),
        AllowedInDungeons = true,
        Icon = CreatePrefabIcon(prefab)
      };

      var piece = new CustomPiece(prefab, true, pieceConfig);
      PieceManager.Instance.AddPiece(piece);
    }

    // Refs:
    //  - ZNetScene.m_prefabs
    public static void RegisterPrefabs(ZNetScene scene)
    {
      foreach (GameObject prefab in scene.m_prefabs)
      {
        if (ShouldIgnorePrefab(prefab)) continue;
        CreatePrefabPiece(prefab);
        CreateGhostPrefab(prefab);
      }
    }

    // Refs:
    //  - ZNetScene.m_prefabs
    //  - Piece
    public static void ModifyExistingPieces(ZNetScene scene)
    {
      foreach (GameObject prefab in scene.m_prefabs)
      {
        var piece = prefab.GetComponent<Piece>();
        if (piece)
          ModifyPiece(piece, false);
      }
    }

  }
}

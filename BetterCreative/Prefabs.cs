
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  internal static class Prefabs
  {
    private static HashSet<string> GetPieceNames()
    {
      HashSet<string> result = new HashSet<string>();

      foreach (var item in ObjectDB.instance.m_items)
      {
        var table = item.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_buildPieces;
        if (table == null) continue;

        foreach (var piece in table.m_pieces)
        {
          result.Add(piece.name);
        }
      }

      return result;
    }

    private static readonly HashSet<string> IgnoredPrefabs = new HashSet<string>() {
      "Player", "Valkyrie", "odin", "CargoCrate", "CastleKit_pot03", "Pickable_Item"
    };

    private static bool IsAnyComponent(GameObject prefab, params string[] components)
    {
      foreach (string component in components)
      {
        if (prefab.GetComponent(component) != null) return true;
      }
      return false;
    }

    private static bool ShouldIgnorePrefab(GameObject prefab)
    {
      HashSet<string> prefabsToSkip = GetPieceNames();

      return
        IsAnyComponent(prefab, "ItemDrop", "Projectile", "TimedDestruction", "Ragdoll", "Plant", "Fish", "FishingFloat", "RandomFlyingBird", "DungeonGenerator", "ZSFX", "MusicLocation", "LocationProxy", "MineRock5", "LootSpawner", "TombStone") ||
        (prefab.GetComponent("Aoe") != null && prefab.GetComponent("WearNTear") == null) ||
        (prefab.GetComponent("TerrainModifier") != null && prefab.GetComponent("Destructible") == null) ||
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
      if (prefab.GetComponent("Location") != null)
      {
        category = "Locations";
      }
      else if (prefab.GetComponent("Pickable") || prefab.GetComponent("PickableItem"))
      {
        category = "Pickable";
      }
      else if (prefab.GetComponent("Humanoid") || prefab.GetComponent("Character") || prefab.GetComponent("Leviathan"))
      {
        category = "Monsters";
      }
      else if (prefab.GetComponent("CreatureSpawner") || prefab.GetComponent("SpawnArea"))
      {
        category = "Monster Spawner";
      }
      else if (prefab.name.Contains("Tree") ||
        prefab.name.Contains("Oak") ||
        prefab.name.Contains("Pine") ||
        prefab.name.Contains("Beech") ||
        prefab.name.Contains("Birch") ||
        prefab.name.Contains("Bush") ||
        prefab.name.Contains("Root") ||
        prefab.name.Contains("root") ||
        prefab.name.Contains("shrub") ||
        prefab.name.Contains("stubbe") ||
        prefab.GetComponent("TreeBase") ||
        prefab.GetComponent("TreeLog"))
      {
        category = "Vegetation";
      }
      else if (prefab.GetComponent("WearNTear") != null)
      {
        category = "Building 2";
      }
      else if (prefab.GetComponent("Destructible") != null)
      {
        category = "Misc Destructible";
      }
      return category;
    }

    private static void ModifyPiece(Piece piece)
    {
      if (piece == null) return;

      piece.m_enabled = true;
      piece.m_canBeRemoved = true;
      piece.m_allowAltGroundPlacement = true;

      if (BetterCreative.NoPieceDrops.Value)
      {
        piece.m_destroyedLootPrefab = null;
      }

      if (BetterCreative.UnrestrictedPlacement.Value)
      {
        //piece.m_clipGround = true;
        piece.m_noClipping = false;
        piece.m_clipEverything = true;
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
        piece.m_spaceRequirement = 0;
      }
    }

    private static void InitPieceData(GameObject prefab)
    {
      Piece piece = prefab.GetComponent<Piece>();
      if (piece == null)
      {
        piece = prefab.AddComponent<Piece>();
      }
      ModifyPiece(piece);
    }

    private static Sprite CreatePrefabIcon(GameObject prefab)
    {
      //prefab = PrefabManager.Instance.GetPrefab(prefab.name + "_ghostfab") ?? prefab;

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

    private static void DestroyComponents<T>(GameObject go)
    {
      var components = go.GetComponentsInChildren<T>();
      foreach (var component in components)
      {
        Object.Destroy(component as UnityEngine.Object);
      }
    }

    private static void CreateGhostPrefab(GameObject prefab)
    {
      GameObject ghost = PrefabManager.Instance.CreateClonedPrefab(prefab.name + "_ghostfab", prefab);

      DestroyComponents<TreeLog>(ghost);
      DestroyComponents<TreeBase>(ghost);
      DestroyComponents<BaseAI>(ghost);
      DestroyComponents<MineRock>(ghost);
      DestroyComponents<CharacterDrop>(ghost);
      DestroyComponents<Humanoid>(ghost);
      DestroyComponents<HoverText>(ghost);
      DestroyComponents<FootStep>(ghost);
      DestroyComponents<VisEquipment>(ghost);
      DestroyComponents<ZSyncAnimation>(ghost);
      DestroyComponents<TerrainModifier>(ghost);
      DestroyComponents<GuidePoint>(ghost);
      DestroyComponents<Light>(ghost);
      DestroyComponents<AudioSource>(ghost);
      DestroyComponents<ZSFX>(ghost);
      DestroyComponents<Windmill>(ghost);
      DestroyComponents<ParticleSystem>(ghost);
      DestroyComponents<Tameable>(ghost);
      DestroyComponents<Procreation>(ghost);
      DestroyComponents<Growup>(ghost);

      var pickableItems = ghost.GetComponentsInChildren<PickableItem>();
      foreach (var pickable in pickableItems)
      {
        if (pickable.m_randomItemPrefabs == null || pickable.m_randomItemPrefabs.Length == 0) continue;

        pickable.m_itemPrefab = pickable.m_randomItemPrefabs[0].m_itemPrefab;
      }

      PrefabManager.Instance.AddPrefab(ghost);
    }

    private static void CreatePrefabPiece(GameObject prefab)
    {
      InitPieceData(prefab);

      var pieceConfig = new PieceConfig
      {
        Name = prefab.name,
        Description = prefab.name,
        PieceTable = "_HammerPieceTable",
        Category = GetPrefabCategory(prefab),
        AllowedInDungeons = true,
        Icon = CreatePrefabIcon(prefab)
      };

      var piece = new CustomPiece(prefab, true, pieceConfig);
      PieceManager.Instance.AddPiece(piece);
    }

    public static void RegisterPrefabs(ZNetScene scene)
    {
      foreach (GameObject prefab in scene.m_prefabs)
      {
        if (ShouldIgnorePrefab(prefab)) continue;
        CreatePrefabPiece(prefab);
        CreateGhostPrefab(prefab);
      }
    }

    public static void ModifyExistingPieces(ZNetScene scene)
    {
      foreach (GameObject prefab in scene.m_prefabs)
      {
        var piece = prefab.GetComponent<Piece>();
        if (piece)
          ModifyPiece(piece);
      }
    }

  }
}

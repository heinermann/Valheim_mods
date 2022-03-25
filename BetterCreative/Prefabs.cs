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
    private static HashSet<string> pieceNameCache = null;
    private static HashSet<string> GetPieceNames()
    {
      if (pieceNameCache == null)
      {
        var result = Resources.FindObjectsOfTypeAll<PieceTable>()
          .SelectMany(pieceTable => pieceTable.m_pieces)
          .Select(piece => piece.name);

        pieceNameCache = new HashSet<string>(result);
      }
      return pieceNameCache;
    }

    private static readonly HashSet<string> IgnoredPrefabs = new HashSet<string>() {
      "Player", "Valkyrie", "HelmetOdin", "CapeOdin", "CastleKit_pot03", "Ravens", "TERRAIN_TEST"
    };

    private static bool ShouldIgnorePrefab(GameObject prefab)
    {
      HashSet<string> prefabsToSkip = GetPieceNames();

      if (!prefab.HasAnyComponentInChildren(typeof(Collider), typeof(Renderer), typeof(CreatureSpawner), typeof(SpawnArea)) ||
        prefab.HasAnyComponentInChildren(typeof(CanvasRenderer)))
      {
        return true;
      }

      return
        prefab.HasAnyComponent(
          "Projectile",
          "TimedDestruction",
          "Ragdoll",
          "LocationProxy",
          "ItemDrop",
          "Gibber",
          "MineRock5",
          "FishingFloat") ||
        (prefab.HasAnyComponent("Aoe") && !prefab.HasAnyComponent("Collider")) ||
        prefab.name.StartsWith("vfx_") ||
        prefab.name.StartsWith("sfx_") ||
        prefab.name.StartsWith("fx_") ||
        prefab.name.StartsWith("_") ||
        prefab.name.EndsWith("_aoe") ||
        IgnoredPrefabs.Contains(prefab.name) ||
        prefabsToSkip.Contains(prefab.name);
    }

    //private static int numExtended = 0;
    private static string GetPrefabCategory(GameObject prefab)
    {
      string category = "Extended";
      if (prefab.GetComponent("Location"))
      {
        category = "Locations";
      }
      else if (prefab.HasAnyComponent("Pickable", "PickableItem"))
      {
        category = "Pickable";
      }
      else if (prefab.HasAnyComponent("Humanoid", "Character", "Leviathan", "RandomFlyingBird", "Fish", "Trader", "Odin", "Valkyrie", "Player"))
      {
        category = "NPCs";
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
      else if (prefab.HasAnyComponent("ArmorStand", "Container", "Fireplace") ||
        prefab.name.ContainsAny("groundtorch", "brazier", "cloth_hanging", "banner", "table", "chair", "sign", "bed"))
      {
        category = "Furniture 2";
      }
      else if (prefab.HasAnyComponent("WearNTear", "Door"))
      {
        category = "Building 2";
      }
      else if (prefab.HasAnyComponent("Destructible", "MineRock"))
      {
        category = "Destructible";
      }
      else if (prefab.GetComponent("ZNetView"))
      {
        category = "Other";
      }
      return category;
    }

    private static readonly HashSet<string> unrestrictedExceptions = new HashSet<string>()
    {
      "horizontal_web", "tunnel_web", "dragoneggcup", "SmokeBall"
    };

    private static readonly HashSet<string> restrictedExceptions = new HashSet<string>()
    {
      "Pickable_SurtlingCoreStand"
    };

    // Refs:
    // - Members of Piece
    private static void ModifyPiece(Piece piece, bool new_piece)
    {
      if (piece == null) return;

      piece.m_enabled = true;
      piece.m_canBeRemoved = true;

      if (piece.gameObject.HasAnyComponent("Character", "Pickable", "PickableItem", "Odin", "RandomFlyingBird", "Fish", "TombStone") ||
        unrestrictedExceptions.Contains(piece.name))
      {
        piece.m_clipEverything = new_piece && !restrictedExceptions.Contains(piece.name);
      }
      
      if (Configs.UnrestrictedPlacement.Value)
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

    private static bool SpriteIsBlank(Sprite sprite)
    {
      Color[] pixels = sprite.texture.GetPixels();
      foreach (var color in pixels)
      {
        if (color.a != 0) return false;
      }
      return true;
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

      if (result == null || SpriteIsBlank(result))
      {
        // TODO: Do something if there's still no image
      }
      return result;
    }

    public static void PrepareGhostPrefab(GameObject ghost)
    {
      ghost.DestroyComponent<CharacterDrop>();

      // Only keep components that are part of a whitelist
      var components = ghost.GetComponentsInChildren<Component>(true);
      foreach (Component component in components)
      {
        // Rigidbody, MeshFilter
        if (component is Piece ||
          component is Collider ||
          component is Renderer ||
          component is Transform ||
          component is ZNetView ||
          component is Rigidbody ||
          component is MeshFilter ||
          component is LODGroup ||
          component is PickableItem)
        {
          continue;
        }

        Object.DestroyImmediate(component);
      }

      Bounds desiredBounds = new Bounds();
      foreach (Renderer renderer in ghost.GetComponentsInChildren<Renderer>())
      {
        desiredBounds.Encapsulate(renderer.bounds);
      }
      var collider = ghost.AddComponent<BoxCollider>();
      collider.center = desiredBounds.center;
      collider.size = desiredBounds.size;
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

    public static void PrintObjectTree(GameObject go, int indent)
    {
      string indentStr = new string(' ', indent);
      Jotunn.Logger.LogInfo(indentStr + go.name);

      foreach (Transform transform in go.transform)
      {
        PrintObjectTree(transform.gameObject, indent + 2);
      }
    }

    public static void FindAndRegisterPrefabs()
    {
      var objects = Resources.FindObjectsOfTypeAll<GameObject>();

      objects.Where(go => go.transform.parent == null && !ShouldIgnorePrefab(go))
        .OrderBy(go => go.name)
        .ToList()
        .ForEach(CreatePrefabPiece);
    }

    // Refs:
    //  - ZNetScene.m_prefabs
    //  - Piece
    public static void ModifyExistingPieces()
    {
      var pieces = Resources.FindObjectsOfTypeAll<Piece>();
      foreach (Piece piece in pieces)
      {
        ModifyPiece(piece, false);
      }
    }

  }
}

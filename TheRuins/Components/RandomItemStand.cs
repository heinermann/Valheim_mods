using Jotunn.Managers;
using System;
using UnityEngine;

namespace Heinermann.TheRuins
{
  public class RandomItemStand : MonoBehaviour
  {
    public void OnEnable()
    {
      ItemStand itemStand = GetComponent<ItemStand>();
      if (itemStand == null)
      {
        Jotunn.Logger.LogWarning($"RandomItemStand attached to {gameObject.name} which is not an ItemStand");
        return;
      }

      AssignRandomItemStandItem(itemStand);
      GameObject.Destroy(this);
    }

    private static string ChooseOne(params string[] items)
    {
      return items[UnityEngine.Random.Range(0, items.Length)];
    }

    private static string GetRandomBiomeTrophy(Heightmap.Biome biome)
    {
      switch (biome)
      {
        case Heightmap.Biome.Meadows:
          return ChooseOne("TrophyNeck", "TrophyBoar", "TrophyGreydwarf");
        case Heightmap.Biome.Swamp:
          return ChooseOne("TrophySkeleton", "TrophyLeech", "TrophyDraugr", "TrophyBlob");
        case Heightmap.Biome.Mountain:
          return ChooseOne("TrophySkeleton", "TrophyDraugr");
        case Heightmap.Biome.BlackForest:
          return ChooseOne("TrophyGreydwarf", "TrophyGreydwarfBrute", "TrophyGreydwarfShaman", "TrophySkeleton");
        case Heightmap.Biome.Plains:
          return ChooseOne("TrophyDeathsquito", "TrophyLox", "TrophyGrowth", "TrophyGoblin");
        case Heightmap.Biome.AshLands:
          return ChooseOne("TrophySurtling");
      }
      return null;
    }

    private static string GetRandomShieldType()
    {
      return ChooseOne("ShieldWood", "ShieldWoodTower");
    }

    private static int GetRandomVariant(string itemName)
    {
      var item = PrefabManager.Instance.GetPrefab(itemName)?.GetComponent<ItemDrop>();

      if (item == null || item.m_itemData.m_shared.m_variants < 2) return 0;

      return UnityEngine.Random.Range(1, item.m_itemData.m_shared.m_variants);
    }

    private static void SetItem(ItemStand stand, string itemName, int variant)
    {
      var view = stand.GetComponent<ZNetView>();
      view.GetZDO().Set("item", itemName);
      view.GetZDO().Set("variant", variant);
      view.InvokeRPC(ZNetView.Everybody, "SetVisualItem", itemName, variant);
    }

    private static void AssignRandomItemStandItem(ItemStand stand)
    {
      var view = stand.GetComponent<ZNetView>();
      if (!view.IsValid() || !String.IsNullOrWhiteSpace(view.GetZDO().GetString("item")))
        return;

      int rng = UnityEngine.Random.Range(0, 100);

      if (rng < 5)
      {
        string shield = GetRandomShieldType();
        SetItem(stand, shield, GetRandomVariant(shield));
      }
      else if (rng < 30)
      {
        Heightmap.Biome biome = Heightmap.FindBiome(stand.transform.position);
        string itemName = GetRandomBiomeTrophy(biome);

        SetItem(stand, itemName, 0);
      }
    }
  }
}

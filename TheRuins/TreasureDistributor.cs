using Jotunn.Managers;
using System.Linq;
using UnityEngine;

namespace Heinermann.TheRuins
{
  internal class TreasureDistributor
  {
    float maxBuildRadius;
    GameObject prefab;

    public TreasureDistributor(GameObject locationPrefab, float maximumBuildRadius)
    {
      this.maxBuildRadius = maximumBuildRadius;
      this.prefab = locationPrefab;
    }

    private float GetTreasureDistributionChance(int numObjects)
    {
      float buildRadius = maxBuildRadius;
      return (100f / numObjects) * Mathf.Sqrt(buildRadius);
    }

    private void DistributeTreasureChestProbabilities()
    {
      var treasureChests = prefab.GetComponentsInChildren<Container>();
      float chestSpawnChance = GetTreasureDistributionChance(treasureChests.Length) / 3f;
      foreach (Container treasureChest in treasureChests)
      {
        var spawn = treasureChest.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = chestSpawnChance;
      }
    }

    private void AddFlies(Vector3 position, string name, float spawnChance)
    {
      GameObject fliesPrefab = PrefabManager.Instance.GetPrefab("Flies");
      if (fliesPrefab == null) return;

      GameObject flies = GameObject.Instantiate(fliesPrefab, prefab.transform, false);
      flies.transform.position = position;
      flies.name += $"Flies_{name}";
      flies.AddComponent<RandomSpawn>().m_chanceToSpawn = spawnChance;
    }

    private void DistributePickableProbabilities()
    {
      var pickableTreasures = prefab.GetComponentsInChildren<PickableItem>();
      float pickableSpawnChance = GetTreasureDistributionChance(pickableTreasures.Length) / 2f;

      foreach (PickableItem pickable in pickableTreasures)
      {
        var spawn = pickable.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = pickableSpawnChance;

        // Randomly add flies for food
        if (pickable.name.StartsWith("Pickable_RandomFood"))
          AddFlies(pickable.transform.position, pickable.name, pickableSpawnChance);
      }
    }

    private void DistributeItemStandProbabilities()
    {
      var itemStands = prefab.GetComponentsInChildren<ItemStand>();
      float standSpawnChance = GetTreasureDistributionChance(itemStands.Length);
      foreach (ItemStand stand in itemStands)
      {
        var spawn = stand.gameObject.GetOrAddComponent<RandomSpawn>();
        spawn.m_chanceToSpawn = standSpawnChance;
      }
    }

    private void DistributePileProbabilities()
    {
      var components = prefab.GetComponentsInChildren<WearNTear>()
        .Where(piece => piece.name.ContainsAny("pile", "stack"))
        .GroupBy(piece => piece.name);

      foreach (var piles in components)
      {
        float chance = GetTreasureDistributionChance(piles.Count());
        foreach (var piece in piles)
        {
          var spawn = piece.gameObject.GetOrAddComponent<RandomSpawn>();
          spawn.m_chanceToSpawn = chance;
        }
      }
    }

    // Applies random spawn chances to pickable treasures, and adds flies to pickable food spawns
    public void DistributeTreasures()
    {
      DistributeTreasureChestProbabilities();
      DistributePickableProbabilities();
      DistributeItemStandProbabilities();
      DistributePileProbabilities();
    }

  }
}

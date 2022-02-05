using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.TheRuins
{
  static internal class Structural
  {
    private struct MaterialProperties
    {
      public float maxSupport;
      public float minSupport;
      public float horizontalLoss;
      public float verticalLoss;
    }

    private static Dictionary<WearNTear.MaterialType, MaterialProperties> materialLookup = new Dictionary<WearNTear.MaterialType, MaterialProperties>();

    public static void InitMaterialLookup()
    {
      var tempWear = new WearNTear();
      foreach (WearNTear.MaterialType material in Enum.GetValues(typeof(WearNTear.MaterialType)))
      {
        tempWear.m_materialType = material;

        MaterialProperties props = default(MaterialProperties);
        tempWear.GetMaterialProperties(out props.maxSupport, out props.minSupport, out props.horizontalLoss, out props.verticalLoss);
        materialLookup.Add(material, props);
      }
    }

    // This is a modified partial copy of the original support algorithm.
    private static Collider[] tempColliders = new Collider[128];
    private static void UpdateSupport(WearNTear wear)
    {
      if (wear.m_colliders == null)
        wear.SetupColliders();

      var materialProperties = materialLookup[wear.m_materialType];
      Vector3 cOM = wear.GetCOM();
      float maxSupport = 0f;

      foreach (WearNTear.BoundData bound in wear.m_bounds)
      {
        int numColliders = Physics.OverlapBoxNonAlloc(bound.m_pos, bound.m_size, tempColliders, bound.m_rot, WearNTear.m_rayMask);
        for (int i = 0; i < numColliders; i++)
        {
          Collider collider = tempColliders[i];
          if (wear.m_colliders.Contains(collider) || collider.attachedRigidbody != null || collider.isTrigger)
            continue;

          WearNTear touchingWear = collider.GetComponentInParent<WearNTear>();
          if (touchingWear == null)
          {
            wear.m_support = materialProperties.maxSupport;
            return;
          }

          if (!touchingWear.m_supports)
            continue;

          float distanceToTouching = Vector3.Distance(cOM, touchingWear.transform.position) + 0.1f;
          float support = touchingWear.m_support;

          maxSupport = Mathf.Max(maxSupport, support - materialProperties.horizontalLoss * distanceToTouching * support);
          Vector3 vector = wear.FindSupportPoint(cOM, touchingWear, collider);
          if (vector.y < cOM.y + 0.05f)
          {
            Vector3 normalized = (vector - cOM).normalized;
            if (normalized.y < 0f)
            {
              float angle = Mathf.Acos(1f - Mathf.Abs(normalized.y)) / ((float)Math.PI / 2f);
              float loss = Mathf.Lerp(materialProperties.horizontalLoss, materialProperties.verticalLoss, angle);
              float angledSupport = support - loss * distanceToTouching * support;
              maxSupport = Mathf.Max(maxSupport, angledSupport);
            }
          }
        }
      }
      wear.m_support = Mathf.Min(maxSupport, materialProperties.maxSupport);
    }

    private static Dictionary<WearNTear, float> InitSettlingObjects(GameObject location, WearNTear.MaterialType material)
    {
      var result = new Dictionary<WearNTear, float>();
      var integrityObjects = location.GetComponentsInChildren<WearNTear>(includeInactive: true);

      foreach (WearNTear wear in integrityObjects)
      {
        if (wear.m_materialType != material) continue;

        UpdateSupport(wear);

        float support = wear.m_support;
        if (support < wear.GetMaxSupport())
        {
          result.Add(wear, support);
        }
      }
      return result;
    }

    private static void SettleIntegrityForMaterial(GameObject location, WearNTear.MaterialType material)
    {
      Dictionary<WearNTear, float> remainingObjects = InitSettlingObjects(location, material);

      int numTotalObjectsDeleted = 0;
      var objectsToRemoveInIteration = new HashSet<WearNTear>();
      var objectsToDelete = new HashSet<WearNTear>();
      while (remainingObjects.Count > 0)
      {
        objectsToRemoveInIteration.Clear();
        foreach (var wearAndOldSupport in remainingObjects)
        {
          UpdateSupport(wearAndOldSupport.Key);
        }
        foreach (var wearAndOldSupport in remainingObjects)
        {
          WearNTear wear = wearAndOldSupport.Key;
          float oldSupport = wearAndOldSupport.Value;

          UpdateSupport(wear);

          float newSupport = wear.m_support;
          if (newSupport == oldSupport)
          {
            objectsToRemoveInIteration.Add(wear);
          }
          else
          {
            remainingObjects[wear] = newSupport;
          }

          if (!wear.HaveSupport())
          {
            objectsToDelete.Add(wear);
          }
        }

        foreach (var removeMe in objectsToRemoveInIteration)
        {
          remainingObjects.Remove(removeMe);
        }

        foreach (var deleteMe in objectsToDelete)
        {
          remainingObjects.Remove(deleteMe);
          UnityEngine.Object.DestroyImmediate(deleteMe.gameObject);
        }
        numTotalObjectsDeleted += objectsToDelete.Count;
      }

      if (numTotalObjectsDeleted > 0)
        Jotunn.Logger.LogInfo($"Settling {location.name} - Deleted {numTotalObjectsDeleted} objects in {material} material pass");
    }

    private static List<WearNTear.MaterialType> GetMaterialIterationOrder()
    {
      // We need to iterate materials twice for edge cases such as placing stone on top of core wood or iron/stone on each other.
      List<Tuple<WearNTear.MaterialType, float>> materialsToIterate = new List<Tuple<WearNTear.MaterialType, float>>();
      foreach (var material in materialLookup)
      {
        materialsToIterate.Add(Tuple.Create(material.Key, material.Value.minSupport));
        materialsToIterate.Add(Tuple.Create(material.Key, material.Value.maxSupport));
      }

      // Sort materials in descending order according to their support values
      materialsToIterate.Sort((mat1, mat2) => mat2.Item2.CompareTo(mat1.Item2));
      return materialsToIterate.Select((mat) => mat.Item1).ToList();
    }

    public static void SettleIntegrity(GameObject location)
    {
      // Settle the location by tier of structural integrity
      foreach (var material in GetMaterialIterationOrder())
      {
        SettleIntegrityForMaterial(location, material);
      }
    }
  }
}

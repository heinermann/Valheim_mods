using UnityEngine;

namespace Heinermann.PassiveLanterns
{
  public class LightTracker : SlowUpdate
  {
    private Light m_light;
    private ItemStand m_itemStand;

    private string lastItem = "";

    public override void Awake()
    {
      m_itemStand = gameObject.GetComponent<ItemStand>();
      m_light = gameObject.GetComponentInChildren<Light>(true);

      base.Awake();
    }

    private void DisableLight()
    {
      m_light.range = 0;
      m_light.intensity = 0;

      Destroy(m_light.GetComponent<LightLod>());

      m_light.gameObject.SetActive(false);
    }

    private void CopyLight(Light other)
    {
      m_light.range = other.range * Configs.RangeMultiplier.Value;
      m_light.intensity = other.intensity;
      m_light.color = other.color;

      m_light.gameObject.GetOrAddComponent<LightLod>();

      m_light.gameObject.SetActive(true);
    }

    private Light GetItemLight(string itemName)
    {
      GameObject itemObj = ObjectDB.instance.GetItemPrefab(itemName);
      return itemObj?.GetComponentInChildren<Light>();
    }

    public override void SUpdate()
    {
      string currentItem = m_itemStand.GetAttachedItem();
      
      if (currentItem == lastItem) return;
      lastItem = currentItem;

      if (string.IsNullOrEmpty(currentItem))
      {
        DisableLight();
        return;
      }

      Light light = GetItemLight(currentItem);
      if (light != null)
      {
        CopyLight(light);
      }
    }
  }
}

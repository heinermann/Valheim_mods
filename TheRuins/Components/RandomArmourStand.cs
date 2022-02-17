using UnityEngine;

namespace Heinermann.TheRuins
{
  public class RandomArmourStand : MonoBehaviour
  {
    public void OnEnable()
    {
      ArmorStand stand = GetComponent<ArmorStand>();
      if (stand == null)
      {
        Jotunn.Logger.LogWarning($"RandomArmourStand attached to {gameObject.name} which is not an ArmorStand");
        return;
      }

      // TODO
    }
  }
}

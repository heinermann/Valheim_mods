using UnityEngine;

namespace Heinermann.TheRuins
{
  public class LocationSettling : MonoBehaviour
  {
    public void Start()
    {
      Jotunn.Logger.LogInfo($"Settling {this.name} with {this.transform.childCount} items");
      Structural.SettleIntegrity(this.gameObject);
      GameObject.Destroy(this);
    }
  }
}

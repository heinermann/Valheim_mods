using UnityEngine;

namespace Heinermann.BetterCreative
{
  public class Murderer : MonoBehaviour
  {
    void Start()
    {
      ZNetScene.instance.Destroy(gameObject);
    }
  }
}

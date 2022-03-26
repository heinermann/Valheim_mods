using UnityEngine;

namespace Heinermann.TheRuins.Components
{
  public class RandomDoor : MonoBehaviour
  {
    public void OnEnable()
    {
      Door door = GetComponent<Door>();
      if (door == null)
      {
        Jotunn.Logger.LogWarning($"RandomDoor attached to {gameObject.name} which is not Door");
        return;
      }

      int doorState = Random.Range(-1, 1);
      door.GetComponent<ZNetView>()?.GetZDO()?.Set("state", doorState);
      door.GetComponent<Animator>()?.SetInteger("state", doorState);

      GameObject.Destroy(this);
    }
  }
}

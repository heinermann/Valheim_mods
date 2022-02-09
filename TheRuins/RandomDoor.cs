using UnityEngine;

namespace Heinermann.TheRuins
{
  // TODO NOT WORKING
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

      Jotunn.Logger.LogWarning($"Randomizing {gameObject.name}");
      //door.SetState(UnityEngine.Random.Range(-1, 1));
      door.m_animator.SetInteger("state", Random.Range(-1, 1));
    }
  }
}

using UnityEngine;

namespace Heinermann.TheRuins
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
      door.m_nview.GetZDO().Set("state", doorState);
      door.m_animator.SetInteger("state", doorState);

      GameObject.Destroy(this);
    }
  }
}

using UnityEngine;

namespace Heinermann.TheRuins
{
  public class RandomDoor : RandomObjectState
  {
    public override void OnSpawned()
    {
      Door door = GetComponent<Door>();
      if (door == null)
      {
        Jotunn.Logger.LogWarning($"RandomDoor attached to {gameObject.name} which is not Door");
        return;
      }

      door.SetState(Random.Range(-1, 1));
      //door.m_animator.SetInteger("state", Random.Range(-1, 1));
    }
  }
}

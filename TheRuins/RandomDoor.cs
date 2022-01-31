using UnityEngine;

namespace Heinermann.TheRuins
{
  public class RandomDoor : RandomObjectState
  {
    public override void OnSpawned()
    {
      Door door = GetComponent<Door>();
      if (door == null) return;

      door.SetState(Random.Range(-1, 1));
      //door.m_animator.SetInteger("state", Random.Range(-1, 1));
    }
  }
}

using UnityEngine;

namespace Heinermann.ImmersiveTransitions
{
  public class PlayerTraveller : PortalTraveller
  {
    Player player;

    void Awake()
    {
      player = GetComponent<Player>();
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
      player.TeleportTo(pos, rot, false);
    }
  }
}

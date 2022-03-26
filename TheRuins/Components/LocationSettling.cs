using System.Collections;
using UnityEngine;

namespace Heinermann.TheRuins.Components
{
  public class LocationSettling : MonoBehaviour
  {
    private void Settle()
    {
      var pieces = transform.GetComponentsInChildren<StructuralPiece>();
      Jotunn.Logger.LogInfo($"Settling {name} with {transform.childCount} items ({pieces.Length} valid)");
      //StructuralPiece.SettleIntegrity(gameObject);
    }

    IEnumerator Start()
    {
      yield return 60;
      Settle();
    }

    /*
    public void Update()
    {
      if (!m_settled && m_canSettle)
      {
        Settle();
      }
    }*/
  }
}

namespace Heinermann.TheRuins
{
  public class RandomArmorStand : RandomObjectState
  {
    public override void OnSpawned()
    {
      var armorStand = GetComponent<ArmorStand>();
      if (armorStand == null)
      {
        Jotunn.Logger.LogWarning($"RandomArmorStand attached to {gameObject.name} which is not ArmorStand");
        return;
      }
      // TODO
    }
  }
}

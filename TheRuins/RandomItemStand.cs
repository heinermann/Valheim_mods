namespace Heinermann.TheRuins
{
  public class RandomItemStand : RandomObjectState
  {
    public override void OnSpawned()
    {
      var itemStand = GetComponent<ItemStand>();
      if (itemStand == null)
      {
        Jotunn.Logger.LogWarning($"RandomItemStand attached to {gameObject.name} which is not ItemStand");
        return;
      }
      // TODO
    }
  }
}

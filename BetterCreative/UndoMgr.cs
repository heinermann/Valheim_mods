using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  internal static class UndoMgr
  {
    public static void Remove(IEnumerable<GameObject> objects)
    {
      var toDelete = objects.ToList();
      var components = toDelete.Select(obj => obj.GetComponent<ZNetView>()?.GetZDO()).Where(zdo => zdo != null).ToList();
      if (components.Any())
      {
        Jotunn.Logger.LogInfo($"Removing {components.Count} objects");
        var removeAction = new UndoActions.UndoRemove(components);
        UndoManager.Instance.Add(BetterCreative.PluginName, removeAction);
      }
      else if (toDelete.Any())
      {
        Jotunn.Logger.LogWarning($"No ZDOs associated with {toDelete.Count} deleted objects");
      }
    }

    public static void Remove(GameObject obj)
    {
      ZDO zdo = obj.GetComponent<ZNetView>()?.GetZDO();
      if (zdo != null)
      {
        Jotunn.Logger.LogInfo($"Removing {obj.name}");
        var removeAction = new UndoActions.UndoRemove(new[] { zdo });
        UndoManager.Instance.Add(BetterCreative.PluginName, removeAction);
      }
      else
      {
        Jotunn.Logger.LogWarning($"No ZDO associated with {obj.name}");
      }
    }

    public static void Create(IEnumerable<GameObject> objects)
    {
      var toCreate = objects.ToList();
      var components = toCreate.Select(obj => obj.GetComponent<ZNetView>()?.GetZDO()).Where(zdo => zdo != null).ToList();
      if (components.Any())
      {
        Jotunn.Logger.LogInfo($"Creating {components.Count} objects");
        var createAction = new UndoActions.UndoCreate(components);
        UndoManager.Instance.Add(BetterCreative.PluginName, createAction);
      }
      else if (toCreate.Any())
      {
        Jotunn.Logger.LogWarning($"No ZDOs associated with {toCreate.Count} created objects");
      }
    }

    public static void Create(GameObject obj)
    {
      ZDO zdo = obj.GetComponent<ZNetView>()?.GetZDO();
      if (zdo != null)
      {
        Jotunn.Logger.LogInfo($"Creating {obj.name}");
        var createAction = new UndoActions.UndoCreate(new[] { zdo });
        UndoManager.Instance.Add(BetterCreative.PluginName, createAction);
      }
      else
      {
        Jotunn.Logger.LogWarning($"No ZDO associated with {obj.name}");
      }
    }

    public static void Undo()
    {
      UndoManager.Instance.Undo(BetterCreative.PluginName);
    }

    public static void Redo()
    {
      UndoManager.Instance.Redo(BetterCreative.PluginName);
    }
  }
}

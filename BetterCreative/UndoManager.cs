using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.BetterCreative
{
  public interface IUndo
  {
    void Undo();
    void Redo();
  }

  public class CreateObjectAction : IUndo
  {
    protected UndoEntry piece;

    public CreateObjectAction(GameObject obj)
    {
      piece = new UndoEntry(obj, isCreated: true);
    }

    public virtual void Redo()
    {
      piece.Create();
      BetterCreative.ShowHUDMessage($"Created 1 {piece.prefab.name}");
    }

    public virtual void Undo()
    {
      piece.Destroy();
      BetterCreative.ShowHUDMessage($"Destroyed 1 {piece.prefab.name}");
    }
  }

  public class DestroyObjectAction : CreateObjectAction
  {
    public DestroyObjectAction(GameObject obj) : base(obj)
    {
      piece.createdEntity = null;
    }

    public override void Redo()
    {
      base.Undo();
    }

    public override void Undo()
    {
      base.Redo();
    }
  }

  class DeleteObjectsAction : IUndo
  {
    protected List<UndoEntry> pieces;

    public DeleteObjectsAction(List<GameObject> deletedPieces)
    {
      pieces = deletedPieces.Select(piece => new UndoEntry(piece, isCreated: false)).ToList();
    }

    public void Redo()
    {
      foreach (UndoEntry piece in pieces)
      {
        piece.Destroy();
      }

      foreach (var group in pieces.GroupBy(piece => piece.prefab.name))
      {
        BetterCreative.ShowHUDMessage($"Destroyed {group.Count()} {group.Key}");
      }
    }

    public void Undo()
    {
      foreach (UndoEntry piece in pieces)
      {
        piece.Create();
      }

      foreach (var group in pieces.GroupBy(piece => piece.prefab.name))
      {
        BetterCreative.ShowHUDMessage($"Created {group.Count()} {group.Key}");
      }
    }
  }

  public class UndoEntry
  {
    public GameObject prefab;
    public Vector3 position;
    public Quaternion orientation;
    public GameObject createdEntity;

    public UndoEntry(GameObject modified, bool isCreated)
    {
      if (isCreated) createdEntity = modified;

      position = modified.transform.position;
      orientation = modified.transform.rotation;
      prefab = PrefabManager.Instance.GetPrefab(modified.name) ?? PrefabManager.Instance.GetPrefab(modified.name.Replace("(Clone)", ""));

      if (prefab == null)
        Jotunn.Logger.LogError($"Failed to find prefab in UndoEntry: {modified.name}");
    }

    public void Create()
    {
      if (createdEntity != null)
      {
        Jotunn.Logger.LogError($"Trying to create in UndoEntry when an object already exists, overwriting");
      }
      createdEntity = BetterCreative.PlacePiece(prefab, position, orientation);
    }

    public void Destroy()
    {
      if (createdEntity == null)
      {
        Jotunn.Logger.LogError($"Trying to destroy a null object in UndoEntry");
        return;
      }

      ZNetScene.instance.Destroy(createdEntity);
      createdEntity = null;
    }
  }

  public class UndoManager
  {
    int undoPosition = 0;
    List<IUndo> undoBuffer = new List<IUndo>();

    void ClearFuture()
    {
      undoBuffer.RemoveRange(undoPosition, undoBuffer.Count - undoPosition);
    }

    public void Undo()
    {
      if (undoPosition > 0)
      {
        undoPosition--;
        undoBuffer[undoPosition].Undo();
      }
    }

    public void Redo()
    {
      if (undoPosition < undoBuffer.Count)
      {
        undoBuffer[undoPosition].Redo();
        undoPosition++;
      }
    }

    public void AddItem(IUndo item)
    {
      ClearFuture();
      undoBuffer.Add(item);
      undoPosition++;
    }
  }
}

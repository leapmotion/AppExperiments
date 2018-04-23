using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class StrokePolyMeshManager_UndoRedo : MonoBehaviour {

    public StrokePolyMeshManager strokePolyMeshManager;

    private void Reset() {
      if (strokePolyMeshManager == null) {
        strokePolyMeshManager = GetComponent<StrokePolyMeshManager>();
      }
    }

    private void Start() {
      strokePolyMeshManager.Temp_OnNewStrokeAdded += onNewStrokeAdded;
    }

    private void onNewStrokeAdded() {
      // This is the gotcha: If a new stroke is created and a new stroke is drawn,
      // the undo stack is cleared, because we've branched the timeline.

      _undoStack.Clear();

      // TODO: This will leave hidden strokes lying around! Need to add functionality to
      // _actually delete_ strokes. Should probably be a threaded clean, really need
      // to investigate using the "modify" functionality of KeyedPolyMeshObjects to
      // _delete_ positions and polygons.
    }
    
    private Stack<StrokeObject> _undoStack = new Stack<StrokeObject>();
    
    public void Undo() {
      // Mark the most recent non-hidden StrokeObject as hidden and add a reference to it
      // to an internal stack.
      var childStrokeObjects = Pool<List<StrokeObject>>.Spawn(); childStrokeObjects.Clear();
      try {
        strokePolyMeshManager.gameObject.GetComponentsInChildren(childStrokeObjects);

        for (int i = childStrokeObjects.Count - 1; i >= 0; i--) {
          var toUndo = childStrokeObjects[i];
          if (!toUndo.isHidden) {
            // Just hide the stroke. This will make it disappear, but we can redo the
            // stroke easily.
            toUndo.HideStroke();

            _undoStack.Push(toUndo);

            break;
          }
        }
      }
      finally {
        childStrokeObjects.Clear(); Pool<List<StrokeObject>>.Recycle(childStrokeObjects);
      }
    }

    public void Redo() {
      // Pop the most recent "undone" stroke (really just hidden) and unhide it.
      if (_undoStack.Count > 0) {
        var redoObject = _undoStack.Pop();
        redoObject.UnhideStroke();
      }
    }

  }

}

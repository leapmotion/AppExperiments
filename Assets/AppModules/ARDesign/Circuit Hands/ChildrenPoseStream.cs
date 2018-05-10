using Leap.Unity.Attributes;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Streams {
  
  [ExecuteInEditMode]
  public class ChildrenPoseStream : MonoBehaviour,
                                    IStream<Pose> {

    public bool useNameRequirement = false;
    [DisableIf("useNameRequirement", isEqualTo:false)]
    public string requireNameContains = "Control Point";

    public enum UpdateMode {
      Update, LateUpdate
    }
    [QuickButton("Send Now", "doUpdate")]
    public UpdateMode updateMode = UpdateMode.Update;

    [EditTimeOnly]
    public bool periodicEditTimeRefresh = false;

    public bool includeRecursiveChildren = true;

    [Disable]
    public List<Transform> children = new List<Transform>();

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    private void OnValidate() {
      updateChildren();
    }

    private void Update() {
      if (!Application.isPlaying && !periodicEditTimeRefresh) return;

      if (updateMode != UpdateMode.Update) return;

      doUpdate();
    }

    private void LateUpdate() {
      if (!Application.isPlaying && !periodicEditTimeRefresh) return;

      if (updateMode != UpdateMode.LateUpdate) return;

      doUpdate();
    }

    private void doUpdate() {
      updateChildren();

      updateStream();
    }

    private void updateChildren() {
      children.Clear();

      if (includeRecursiveChildren) {
        this.transform.GetAllChildren(children);
      }
      else {
        foreach (var child in this.transform.GetChildren()) {
          children.Add(child);
        }
      }

      if (useNameRequirement) {
        var filteredList = Pool<List<Transform>>.Spawn(); filteredList.Clear();
        try {
          foreach (var child in children.Query()
                     .Where(t => t.name.Contains(requireNameContains))) {
            filteredList.Add(child);
          }
        }
        finally {
          Utils.Swap(ref filteredList, ref children);

          filteredList.Clear();
          Pool<List<Transform>>.Recycle(filteredList);
        }
      }
    }

    private void updateStream() {
      OnOpen();

      foreach (var child in children) {
        OnSend(child.ToPose());
      }

      OnClose();
    }

  }

}

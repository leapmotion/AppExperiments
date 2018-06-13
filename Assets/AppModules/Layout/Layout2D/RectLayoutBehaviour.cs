using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  [ExecuteInEditMode]
  public abstract class RectLayoutBehaviour : RectTransformBehaviour {

    public Transform[] layoutTransforms;

    protected virtual void Reset() {
      initChildren();
    }

    protected virtual void OnValidate() {
      initChildren();
    }

    protected virtual void Start() {
      if (!Application.isPlaying) {
        initChildren();
      }

      _updatePhaseOffset = Random.Range(0, LAYOUT_UPDATE_PERIOD - 1);
    }

    private const int LAYOUT_UPDATE_PERIOD = 5;
    private int _updateCount = 0;
    private int _updatePhaseOffset = 0;
    protected virtual void Update() {
      if (!Application.isPlaying) {
        initChildren();
      }

      _updateCount += 1;
      _updateCount %= LAYOUT_UPDATE_PERIOD;
      if (_updateCount == _updatePhaseOffset || !Application.isPlaying) {
        UpdateLayoutTargets();
      }

      UpdateLayoutVisuals();
    }

    /// <summary>
    /// Called every few frames (every update in edit-mode). Update target positions here.
    /// </summary>
    protected abstract void UpdateLayoutTargets();

    /// <summary>
    /// Called every frame. Move elements to their target positions here.
    /// </summary>
    protected abstract void UpdateLayoutVisuals();

    private void initChildren() {
      var childrenList = Pool<List<Transform>>.Spawn();
      childrenList.Clear();
      try {
        foreach (var child in this.transform.GetChildren()) {
          childrenList.Add(child);
        }

        layoutTransforms = childrenList.Query().ToArray();
      }
      finally {
        childrenList.Clear();
        Pool<List<Transform>>.Recycle(childrenList);
      }
    }

  }

}

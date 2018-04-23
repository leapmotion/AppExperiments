using Leap.Unity.Attributes;
using Leap.Unity.Splines;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  public class SplineControlPointEditor : MonoBehaviour {

    // We'll construct a PrefabPool for each type of ControlPointTangentEditor that
    // any ControlPointEditor needs.
    private static Dictionary<SplineControlPointTangentEditor, PrefabPool> s_tangentEditorPools = new Dictionary<SplineControlPointTangentEditor, PrefabPool>();

    /// <summary> The Spline containing the control point this editor edits. </summary>
    public zzOldSplineObject spline;

    /// <summary> The index in the spline of the control point this editor edits. </summary>
    [Tooltip("The index in the spline of the control point this editor edits.")]
    [Disable]
    public int controlPointIdx;

    [Header("Tangent Editor Prefab (pooled)")]
    [EditTimeOnly]
    public SplineControlPointTangentEditor tangentEditorPrefab;

    /// <summary>
    /// The forward and backward tangent vector handles for this control point. Currently
    /// only smooth tangents are supported; modifying the the forward and backward handles will
    /// always point in opposite directions and be the same length.
    /// </summary>
    private SplineControlPointTangentEditor[] _tangentHandles = new SplineControlPointTangentEditor[2];

    private InteractionBehaviour _intObj;

    void Awake() {
      if (!s_tangentEditorPools.ContainsKey(tangentEditorPrefab)) {
        PrefabPool controlPointTangentEditorPool = new GameObject().AddComponent<PrefabPool>();
        controlPointTangentEditorPool.prefab = tangentEditorPrefab.gameObject;
        s_tangentEditorPools[tangentEditorPrefab] = controlPointTangentEditorPool;
      }
    }

    void Start() {
      _intObj = GetComponent<InteractionBehaviour>();
      if (_intObj != null) _intObj.OnGraspStay += onGraspStay;
    }

    private void onGraspStay() {
      this.spline.SetControlPosition(controlPointIdx, this.transform.position);
    }

    /// <summary>
    /// Moves this control point editor to match the current position of the
    /// spline's control point this editor edits. Also refreshes any tangent
    /// vector handles owned by this editor.
    /// </summary>
    public void RefreshHandle() {
      this.transform.position = spline.GetControlPosition(controlPointIdx);

      // Make sure tangent handles exist.
      for (int i = 0; i < _tangentHandles.Length; i++) {
        if (_tangentHandles[i] == null) {
          _tangentHandles[i] = s_tangentEditorPools[tangentEditorPrefab].Spawn<SplineControlPointTangentEditor>();
          _tangentHandles[i].controlPointEditor = this;
          _tangentHandles[i].isForwardTangent = i == 0 ? true : false;
          _tangentHandles[i].transform.parent = this.transform;
        }
      }

      foreach (var tangentHandle in _tangentHandles) {
        tangentHandle.RefreshHandle();
      }
    }
  }

}
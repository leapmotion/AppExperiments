using Leap.Unity.Attributes;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  public class SplineEditor : MonoBehaviour {

    // We'll construct a PrefabPool for each type of ControlPointEditor that
    // any SplineEditor needs.
    private static Dictionary<SplineControlPointEditor, PrefabPool> s_controlPointEditorPools = new Dictionary<SplineControlPointEditor, PrefabPool>();

    public zzOldSplineObject spline;

    [Header("Control Point Prefab (pooled)")]
    [EditTimeOnly]
    public SplineControlPointEditor controlPointEditorPrefab;

    private List<SplineControlPointEditor> _controlPointHandles = new List<SplineControlPointEditor>();

    void Awake() {
      if (!s_controlPointEditorPools.ContainsKey(controlPointEditorPrefab)) {
        PrefabPool controlPointEditorPool = new GameObject().AddComponent<PrefabPool>();
        controlPointEditorPool.prefab = controlPointEditorPrefab.gameObject;
        s_controlPointEditorPools[controlPointEditorPrefab] = controlPointEditorPool;
      }
    }

    void Start() {
      spline.OnSplineModified += onSplineModified;
    }

    private void onSplineModified() {
      refreshEditorHandles();
    }

    private void refreshEditorHandles() {
      // Make sure enough control point editors exist.
      for (int i = 0; i < spline.numControlPoints - _controlPointHandles.Count; i++) {
        var handle = s_controlPointEditorPools[controlPointEditorPrefab].Spawn<SplineControlPointEditor>();
        handle.spline = spline;
        // Control point index will be set in the last section of the method.
        handle.gameObject.SetActive(true);
        _controlPointHandles.Add(handle);
      }

      // Recycle any extra control point editors.
      for (int i =  _controlPointHandles.Count - spline.numControlPoints; i > 0; i--) {
        var handle = _controlPointHandles[i];
        handle.spline = null;
        handle.controlPointIdx = -1;
        handle.gameObject.SetActive(false);
        s_controlPointEditorPools[controlPointEditorPrefab].Recycle<SplineControlPointEditor>(handle);
      }

      // Refresh the spline's control point editors.
      for (int i = 0; i < _controlPointHandles.Count; i++) {
        var handle = _controlPointHandles[i];
        handle.controlPointIdx = i;
        handle.RefreshHandle();
      }
    }

  }

}
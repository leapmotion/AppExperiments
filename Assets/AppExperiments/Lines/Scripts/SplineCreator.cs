using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  public class SplineCreator : MonoBehaviour, ILineCreator {

    private static Dictionary<zzOldSplineObject, PrefabPool> s_splinePrefabPools = new Dictionary<zzOldSplineObject, PrefabPool>();

    [Header("Spline Prefab")]
    public zzOldSplineObject splinePrefab;

    private zzOldSplineObject _curSpline;

    private bool _isCreatingLine = false;
    public bool isCreatingLine { get { return _isCreatingLine; } }

    void Awake() {
      if (!s_splinePrefabPools.ContainsKey(splinePrefab)) {
        s_splinePrefabPools[splinePrefab] = new GameObject().AddComponent<PrefabPool>();
        s_splinePrefabPools[splinePrefab].prefab = splinePrefab.gameObject;
      }
    }

    public void BeginLine() {
      _isCreatingLine = true;
      _curSpline = s_splinePrefabPools[splinePrefab].Spawn<zzOldSplineObject>();
      _curSpline.gameObject.SetActive(true);
      _curSpline.transform.parent = null;

      _curSpline.AddControlPoint(Vector3.zero, Vector3.zero);
      _curSpline.AddControlPoint(Vector3.zero, Vector3.zero);
    }

    public void UpdateLine(Vector3 a, Vector3 b) {
      _curSpline.SetControlPosition(0, a);
      _curSpline.SetControlPosition(1, b);
    }

    public void FinishLine() {
      _isCreatingLine = false;
    }

    public void CancelLine() {
      _isCreatingLine = false;
      _curSpline.Clear();
      _curSpline.gameObject.SetActive(false);

      s_splinePrefabPools[splinePrefab].Recycle(_curSpline);
    }

  }

}

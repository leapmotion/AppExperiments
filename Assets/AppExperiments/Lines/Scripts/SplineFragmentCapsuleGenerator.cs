using Leap.Unity.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  [RequireComponent(typeof(zzOldSplineObject))]
  public class SplineFragmentCapsuleGenerator : MonoBehaviour {

    private static Dictionary<SplineFragmentCapsule, PrefabPool> s_fragmentCapsulePools = new Dictionary<SplineFragmentCapsule, PrefabPool>();

    public SplineFragmentCapsule fragmentCapsulePrefab;

    private zzOldSplineObject _spline;
    private bool _splineDirty;

    private List<SplineFragmentCapsule> _splineCapsules = new List<SplineFragmentCapsule>();

    public Action OnRefreshSplineCapsules = () => { };

    void Awake() {
      if (!s_fragmentCapsulePools.ContainsKey(fragmentCapsulePrefab)) {
        s_fragmentCapsulePools[fragmentCapsulePrefab] = new GameObject().AddComponent<PrefabPool>();
        s_fragmentCapsulePools[fragmentCapsulePrefab].prefab = fragmentCapsulePrefab.gameObject;
      }
    }

    void Start() {
      _spline = GetComponent<zzOldSplineObject>();
      _spline.OnSplineModified += onSplineModified;
    }

    private void onSplineModified() {
      _splineDirty = true;
    }

    void FixedUpdate() {
      if (_splineDirty) {
        refreshSplineCapsules();
        _splineDirty = false;
      }
    }

    private void refreshSplineCapsules() {
      int capsuleIdx = 0;

      foreach (SplineFragment fragment in _spline.TraverseFragments()) {
        SplineFragmentCapsule capsule;
        bool addNew = false;

        if (capsuleIdx > _splineCapsules.Count - 1) {
          // Add a new capsule segment.
          capsule = s_fragmentCapsulePools[fragmentCapsulePrefab].Spawn<SplineFragmentCapsule>();
          capsule.transform.parent = this.transform;
          addNew = true;
        }
        else {
          // Modify existing capsule segment.
          capsule = _splineCapsules[capsuleIdx];
          addNew = false;
        }

        capsule.collider.enabled = true;
        capsule.splineFragment = fragment;
        capsule.collider.gameObject.layer = _spline.gameObject.layer;
        capsule.RefreshCapsulePoints();

        if (addNew) { _splineCapsules.Add(capsule); }

        capsuleIdx++;
      }

      // Recycle older segments if they aren't needed anymore.
      for (int i = _splineCapsules.Count - 1; i > capsuleIdx; i--) {
        _splineCapsules[i].collider.enabled = false;
        s_fragmentCapsulePools[fragmentCapsulePrefab].Recycle<SplineFragmentCapsule>(_splineCapsules[i]);
        _splineCapsules.RemoveAt(i);
      }

      OnRefreshSplineCapsules();
    }

  }
  
}
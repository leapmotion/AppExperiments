using Leap.Unity;
using Leap.Unity.Splines;
using UnityEngine;

namespace Leap.Unity.Apps.Lines {

  /// <summary>
  /// Provides a CapsuleCollider for a SplineFragment. The capsule must
  /// be explicitly refreshed by calling RefreshCapsulePoints()!
  /// 
  /// Each segment of a spline, defined as the portion of a spline between
  /// two spline control points, contains one or more spline fragments 
  /// depending on the curvature of the spline.
  /// </summary>
  [RequireComponent(typeof(CapsuleCollider))]
  public class SplineFragmentCapsule : MonoBehaviour {

    public SplineFragment splineFragment;

    public new CapsuleCollider collider;

    void OnValidate() {
      if (collider == null) {
        collider = GetComponent<CapsuleCollider>();
      }
    }

    void Awake() {
      collider = GetComponent<CapsuleCollider>();
    }

    public void RefreshCapsulePoints() {
      collider.SetCapsulePoints(splineFragment.a, splineFragment.b);
    }

  }

}
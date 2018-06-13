using InteractionEngineUtility;
using Leap.Unity;
using Leap.Unity.Gestures;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Splines;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Lines {

  public class PullSplineGesture : OneHandedGesture, IRuntimeGizmoComponent {

    [Header("Todo: Delete this reference (exclusivity)")]
    public CreateLineGesture createLineGesture;

    public const float PINCH_DEACTIVATION_STRENGTH = 0.75F;

    //private bool _readyToActivate = false;
    private zzOldSplineObject _targetSpline = null;
    private int _targetSplineClosestSegmentA = 0;
    private int _targetSplineClosestSegmentB = 0;
    private Vector3 _targetSplineClosestPoint;
    private float _targetSplineDist;
    //private SplineFragmentCapsule _targetSplineFragmentCapsule;

    private Vector3 _pinchPos;

    protected override void WhileHandTracked(Hand hand) {
      base.WhileHandTracked(hand);

      _pinchPos = hand.GetPredictedPinchPosition();
    }


    private float debugActivationAmount = 0F;

    private float debugWaitTime = 0.05F;
    private float debugWaitTimer = 0.05F;

    private float debugActivationDist = 0.025F;

    protected override bool ShouldGestureActivate(Hand hand) {

      if (createLineGesture.isActive) {
        return false;
      }

      if (hand.PinchStrength > 0.85F && _targetSpline != null && _targetSplineDist < debugActivationDist) {
        debugWaitTimer -= Time.deltaTime;
      }
      else {
        debugWaitTimer = debugWaitTime;
      }

      debugActivationAmount = ((debugWaitTime - debugWaitTimer) / debugWaitTime);

      if (debugWaitTimer <= 0F) {
        debugWaitTimer = debugWaitTime;
        return true;
      }

      return false;
    }

    protected override bool ShouldGestureDeactivate(Hand hand, out DeactivationReason? deactivationReason) {
      deactivationReason = DeactivationReason.FinishedGesture; // only used if we return true
      return hand.PinchStrength < PINCH_DEACTIVATION_STRENGTH;
    }

    private Collider[] _possibleSplineColliders = new Collider[64];
    protected override void WhileGestureInactive(Hand maybeNullHand) {
      base.WhileGestureInactive(maybeNullHand);

      Hand hand = maybeNullHand;
      if (hand == null) {
        _targetSpline = null;
      }
      else {
        int numCollidersHit = Physics.OverlapSphereNonAlloc(_pinchPos, 0.3F, _possibleSplineColliders);

        zzOldSplineObject closestSpline = null;
        Vector3 closestPointOnSpline = Vector3.zero;
        int closestSegmentA = 0, closestSegmentB = 0;
        float closestDist = float.PositiveInfinity;
        //SplineFragmentCapsule closestFragmentCapsule = null;
        Vector3 closestPinchPosOnSpline = Vector3.zero;

        for (int i = 0; i < numCollidersHit; i++) {
          Collider collider = _possibleSplineColliders[i];
          SplineFragmentCapsule fragmentCapsule = collider.GetComponent<SplineFragmentCapsule>();
          if (fragmentCapsule == null) continue;

          SplineFragment fragment = fragmentCapsule.splineFragment;
          zzOldSplineObject spline = fragment.spline as zzOldSplineObject;
          Vector3 pinchPosOnFragment = _pinchPos.ConstrainToSegment(fragment.a, fragment.b);

          float testDist = Vector3.Distance(pinchPosOnFragment, _pinchPos);
          if (closestSpline == null || testDist < closestDist) {
            closestSpline = spline;
            closestSegmentA = fragment.controlPointAIdx;
            closestSegmentB = fragment.controlPointBIdx;
            closestPointOnSpline = pinchPosOnFragment;
            //closestFragmentCapsule = fragmentCapsule;
            closestDist = testDist;
          }
        }

        if (closestSpline != null) {
          _targetSpline = closestSpline;
          _targetSplineClosestSegmentA = closestSegmentA;
          _targetSplineClosestSegmentB = closestSegmentB;
          _targetSplineClosestPoint = closestPointOnSpline;
          _targetSplineDist = closestDist;
          //_targetSplineFragmentCapsule = closestFragmentCapsule;
        }
        else {
          _targetSpline = null;
          _targetSplineClosestSegmentA = 0;
          _targetSplineClosestSegmentB = 0;
          _targetSplineClosestPoint = Vector3.zero;
          _targetSplineDist = float.PositiveInfinity;
          //_targetSplineFragmentCapsule = null;
        }
      }
    }

    protected override void WhenGestureActivated(Hand hand) {
      base.WhenGestureActivated(hand);

      //_readyToActivate = false;

      Vector3 midControlPointPosition = _targetSplineClosestPoint;
      Vector3 midControlPointTangent = (_targetSpline[_targetSplineClosestSegmentB].position
                                        - _targetSpline[_targetSplineClosestSegmentA].position) / 2F;

      _targetSpline.AddControlPointBetween(_targetSplineClosestSegmentA,
                                           _targetSplineClosestSegmentB,
                                           midControlPointPosition, midControlPointTangent);

      // _targetSplineClosestSegmentB now references the 'midControlPoint'
    }

    protected override void WhileGestureActive(Hand hand) {
      base.WhileGestureActive(hand);

      _targetSpline.SetControlPosition(_targetSplineClosestSegmentB, _pinchPos);
    }

    protected override void WhenGestureDeactivated(Hand maybeNullHand, DeactivationReason reason) {
      base.WhenGestureDeactivated(maybeNullHand, reason);

      _targetSpline = null;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {

      if (!isActive) {
        drawer.color = Color.white;

        Vector3 pos = _pinchPos;
        Vector3 normal = (pos - Camera.main.transform.position).normalized;

        Vector3 startDir = Vector3.Cross(Vector3.Cross(normal, Vector3.down), normal).normalized;
        drawer.DrawWireArc(pos, -normal, startDir, 0.03F, debugActivationAmount, 32);
        drawer.DrawWireArc(pos, -normal, startDir, 0.025F, debugActivationAmount, 32);
      }

    }

  }

}
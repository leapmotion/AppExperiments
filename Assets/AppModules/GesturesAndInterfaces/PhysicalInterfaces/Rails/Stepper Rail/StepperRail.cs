using Leap.Unity.Animation;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.PhysicalInterfaces {

  using IPositionSpline = ISpline<Vector3, Vector3>;

  [ExecuteInEditMode]
  public class StepperRail : MonoBehaviour, IRuntimeGizmoComponent,
                                            IPositionSpline {

    [Header("Center Point Velocity Source")]
    public Transform centerPointVelocity;

    [Header("Rail Edge Point (Mirrored on X) - child for Velocity")]
    public Transform railEdgePoint;

    [Header("Stack Edge Point (Mirrored on X) - child for Velocity")]
    public Transform stackEdgePoint;

    [Header("Stack Edge Point 2 (Mirrored on X) - child for Velocity")]
    public Transform stackEdgePoint2;

    [Header("Panel Objects Test")]
    public Transform panelObjectsParent;

    public float tSpacing = 0.75f;
    public float speedMod = 1f;
    public float testTCenter = 0f;

    public float targetTCenter = 0f;

    [Header("Debug")]

    public bool drawSegments = false;
    public bool drawPoses = false;
    public bool organizeEditTime = false;

    private List<Transform> _panelObjectsBuffer = new List<Transform>();

    public Pose centerPose {
      get { return this.transform.ToPose(); }
    }

    public PoseSplineSequence? maybePoseSplines = null;

    private HermitePoseSpline[] _backingPoseSplinesArr = null;
    private HermitePoseSpline[] _poseSplinesArr {
      get {
        if (_backingPoseSplinesArr == null || _backingPoseSplinesArr.Length != 6) {
          _backingPoseSplinesArr = new HermitePoseSpline[6];
        }
        return _backingPoseSplinesArr;
      }
    }

    private void Update() {
      if (stackEdgePoint != null && railEdgePoint != null) {
        var poseN3 = stackEdgePoint2.parent.transform.ToPose().Then(stackEdgePoint2.ToLocalPose().MirroredX());
        var poseN2 = stackEdgePoint.parent.transform.ToPose().Then(stackEdgePoint.ToLocalPose().MirroredX());
        var poseN1 = railEdgePoint.parent.transform.ToPose().Then(railEdgePoint.ToLocalPose().MirroredX());
        var pose0 = this.transform.ToPose();
        var pose1 = railEdgePoint.ToPose();
        var pose2 = stackEdgePoint.ToPose();
        var pose3 = stackEdgePoint2.ToPose();

        var pose1Child = railEdgePoint.transform.GetFirstChild();
        var pose2Child = stackEdgePoint.transform.GetFirstChild();
        var pose3Child = stackEdgePoint2.transform.GetFirstChild();

        var pose0Movement = new Movement(transform.ToPose(), centerPointVelocity.ToPose(), 0.1f);
        Movement pose1Movement = Movement.identity;
        Movement poseN1Movement = Movement.identity;
        if (pose1Child != null) {
          pose1Movement = new Movement(pose1, pose1Child.ToPose(), 0.1f);
          poseN1Movement = new Movement(poseN1, poseN1.Then(pose1Child.ToLocalPose().Negated().MirroredX()), 0.1f);
        }
        Movement pose2Movement = Movement.identity;
        Movement poseN2Movement = Movement.identity;
        if (pose2Child != null) {
          pose2Movement = new Movement(pose2, pose2Child.ToPose(), 0.1f);
          poseN2Movement = new Movement(poseN2, poseN2.Then(pose2Child.ToLocalPose().Negated().MirroredX()), 0.1f);
        }
        Movement pose3Movement = Movement.identity;
        Movement poseN3Movement = Movement.identity;
        if (pose3Child != null) {
          pose3Movement = new Movement(pose3, pose3Child.ToPose(), 0.1f);
          poseN3Movement = new Movement(poseN3, poseN3.Then(pose3Child.ToLocalPose().Negated().MirroredX()), 0.1f);
        }

        var time0 = 0;
        var time1 = railEdgePoint.transform.localScale.x;
        var time2 = time1 + stackEdgePoint.transform.localScale.x;
        var time3 = time2 + stackEdgePoint2.transform.localScale.x;

        _poseSplinesArr[0] = new HermitePoseSpline(-time3, -time2, poseN3, poseN2, poseN3Movement, poseN2Movement);
        _poseSplinesArr[1] = new HermitePoseSpline(-time2, -time1, poseN2, poseN1, poseN2Movement, poseN1Movement);
        _poseSplinesArr[2] = new HermitePoseSpline(-time1,  time0, poseN1,  pose0, poseN1Movement,  pose0Movement);
        _poseSplinesArr[3] = new HermitePoseSpline( time0,  time1,  pose0,  pose1,  pose0Movement,  pose1Movement);
        _poseSplinesArr[4] = new HermitePoseSpline( time1,  time2,  pose1,  pose2,  pose1Movement,  pose2Movement);
        _poseSplinesArr[5] = new HermitePoseSpline( time2,  time3,  pose2,  pose3,  pose2Movement,  pose3Movement);

        maybePoseSplines = new PoseSplineSequence(_poseSplinesArr,
                                                  allowExtrapolation: true);


        // Panel Objects Test

        if (organizeEditTime || Application.isPlaying) {
          if (panelObjectsParent != null && panelObjectsParent.childCount > 0) {
            _panelObjectsBuffer.Clear();
            foreach (var panelObject in panelObjectsParent.GetChildren()) {
              _panelObjectsBuffer.Add(panelObject);
            }

            if (Application.isPlaying) {
              testTCenter += _momentumT;
              _momentumT = Mathf.Lerp(_momentumT, 0f, 5f * Time.deltaTime);
            }

            if (Application.isPlaying) {
              testTCenter = Mathf.Lerp(testTCenter, targetTCenter, 5f * Time.deltaTime);
            }

            var splines = maybePoseSplines.Value;
            for (int i = 0; i < _panelObjectsBuffer.Count; i++) {
              var t = testTCenter + ((-2 + i) * tSpacing);
              var objPose = splines.PoseAt(t);
              _panelObjectsBuffer[i].transform.SetPose(objPose);
            }
          }
        }

      }
    }

    public Vector3 FindNearestPosition(Vector3 position, out float tOfPos) {
      var closestSqrDist = float.PositiveInfinity;
      float? closestT = null;
      Vector3? closestPos = null;
      int numDivisions = 256;
      var tStep = (maxT - minT) / numDivisions;
      var spline = maybePoseSplines.Value.AsPositionSpline();
      for (int i = 0; i <= numDivisions; i++) {
        var t = minT + tStep * i;

        var testPos = spline.ValueAt(t);

        var testSqrDist = (position - testPos).sqrMagnitude;
        if (testSqrDist < closestSqrDist) {
          closestT = t;
          closestSqrDist = testSqrDist;
          closestPos = testPos;
        }
      }

      tOfPos = closestT.Value;
      return closestPos.Value;
    }

    private float _momentumT = 0f;

    public void MoveT(float newT, int handleIdx) {
      var origT = testTCenter + (-2 + handleIdx) * tSpacing;
      var finalT = Mathf.Lerp(origT, newT, 15f * Time.deltaTime);

      _momentumT = finalT - origT;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!gameObject.activeInHierarchy || !this.enabled) return;

      drawer.color = LeapColor.brown;

      if (maybePoseSplines != null) {
        drawer.DrawPoseSplineSequence(maybePoseSplines.Value,
                                      drawPoses: drawPoses,
                                      drawSegments: drawSegments);
      }
    }

    #region IPositionSpline - ISpline<Vector3, Vector3>

    public float minT {
      get { return maybePoseSplines.HasValue ? maybePoseSplines.Value.minT : 0f; }
    }

    public float maxT {
      get { return maybePoseSplines.HasValue ? maybePoseSplines.Value.maxT : 0f; }
    }

    public Vector3 ValueAt(float t) {
      return maybePoseSplines.HasValue ? maybePoseSplines.Value.ValueAt(t).position
                                       : Vector3.zero;
    }

    public Vector3 DerivativeAt(float t) {
      return maybePoseSplines.HasValue ? maybePoseSplines.Value.DerivativeAt(t).velocity
                                       : Vector3.zero;
    }

    public void ValueAndDerivativeAt(float t, out Vector3 value, out Vector3 deltaValuePerT) {
      value = Vector3.zero;
      deltaValuePerT = Vector3.zero;

      if (maybePoseSplines.HasValue) {
        Pose pose;
        Movement movement;
        maybePoseSplines.Value.ValueAndDerivativeAt(t, out pose, out movement);

        value = pose.position;
        deltaValuePerT = movement.velocity;
      }
    }

    #endregion

  }

  public static class StepperRailExtensions {
    public static Transform GetFirstChild(this Transform t) {
      if (t.childCount == 0) { return null; }
      return t.GetChild(0);
    }
  }

}

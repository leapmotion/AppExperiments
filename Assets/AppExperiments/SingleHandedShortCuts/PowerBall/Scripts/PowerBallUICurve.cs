using Leap.Unity.Animation;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity;
using Leap.Unity.Attachments;
using System;
using System.Linq;
using UnityEngine.Events;


namespace LeapSingleHandedShortcuts {

  public class PowerBallUICurve : MonoBehaviour {
    public List<Transform> CurvePoints = new List<Transform>();
    public int ArcSegmentCount;
    public Transform CurvePointTemplate;
    public Transform CurveParent;
    public Transform[] ControlPoints;

    private void Start() {
      BuildCurve();
    }

    //public PowerBallUICurve(int arcSegmentCount, Transform curvePointTemplate, Transform powerBallUI) {
    //  ArcSegmentCount = arcSegmentCount;
    //  CurvePointTemplate = curvePointTemplate;
    //  CurveParent = powerBallUI;
    //  BuildCurve();
    //}


    public void BuildCurve() {
      for (int i = 0; i < ArcSegmentCount + 1; i++) {
        Transform newPoint = GameObject.Instantiate(CurvePointTemplate);
        CurvePoints.Add(newPoint);
        newPoint.gameObject.SetActive(true);
        newPoint.parent = transform;
      }
    }

    public void ActivateCurve(bool showHide) {
      for (int i = 0; i < ArcSegmentCount; i++) {
        CurvePoints[i].gameObject.SetActive(showHide);
      }
      if (showHide) {
        DrawShortCutCurve();
      }
    }
    public Transform target;
    public void DrawShortCutCurve() {
      for (int i = 0; i <= ArcSegmentCount; i++) {
        float t = i / (float)ArcSegmentCount;
        Vector3 pos = CalculateCubicBezierPoint(t, ControlPoints[0].position, ControlPoints[1].position, ControlPoints[2].position, ControlPoints[3].position);
        Vector3 velocity = GetFirstDerivative(t, ControlPoints[0].position, ControlPoints[1].position, ControlPoints[3].position, ControlPoints[2].position);
        CurvePoints[i].position = pos;
        CurvePoints[i].localRotation = Quaternion.identity;
        CurvePoints[i].rotation = Quaternion.LookRotation(target.position - CurvePoints[i].position);
      }
    }

    public  Transform NearestDotOnCurve(Vector3 originalPosition) {
      Transform nearestPos = CurvePoints[0];
      float closestDistance = (originalPosition - CurvePoints[0].position).magnitude;
      for (int i = 1; i <= ArcSegmentCount; i++) {
        float testDistance = (originalPosition - CurvePoints[i].position).magnitude;
        if (testDistance < closestDistance) {
          closestDistance = testDistance;
          nearestPos = CurvePoints[i];
        }
      }
      return nearestPos;
    }


    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
      float u = 1 - t;
      float tt = t * t;
      float uu = u * u;
      float uuu = uu * u;
      float ttt = tt * t;

      Vector3 p = uuu * p0;
      p += 3 * uu * t * p1;
      p += 3 * u * tt * p2;
      p += ttt * p3;

      //return (1 - t) * (1 - t) * (1 - t) * p0 + 3 * (1 - t) * (1 - t) * t * p1 + 3 * (1 - t) * t * t * p2 + t * t * t * p3;
      return p;
    }
    public static Vector3 GetFirstDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
      t = Mathf.Clamp01(t);
      float oneMinusT = 1f - t;
      return
        3f * oneMinusT * oneMinusT * (p1 - p0) +
        6f * oneMinusT * t * (p2 - p1) +
        3f * t * t * (p3 - p2);
    }
  }
}

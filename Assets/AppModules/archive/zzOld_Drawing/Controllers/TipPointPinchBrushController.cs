using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity;

namespace Leap.Unity.zzOldDrawing {

  public class TipPointPinchBrushController : MonoBehaviour, IRuntimeGizmoComponent {

    public Brush brush;
    public Chirality tipHandChirality = Chirality.Left;
    public Leap.Finger.FingerType tipFingerType;
    public float tipRadius = 0.07f;
    public float tipDistance = 0.07f;

    public Chirality brushHandChirality = Chirality.Right;

    private bool _isTipHandTracked = false;
    private bool _isBrushHandTracked = false;
    private Vector3 _pointBeginPosition;

    private void Update() {
      Hand tipHand = Hands.Get(tipHandChirality);
      Hand brushHand = Hands.Get(brushHandChirality);

      _isTipHandTracked = tipHand != null;
      _isBrushHandTracked = brushHand != null;

      if (_isTipHandTracked) {
        var tipFinger = tipHand.Fingers[(int)tipFingerType];

        _pointBeginPosition = tipFinger.bones[1].PrevJoint.ToVector3()
                              + tipFinger.bones[1].Direction.ToVector3()
                                * (tipFinger.Length + tipDistance);
      }

      var brushHandIsPinching = _isBrushHandTracked && brushHand.PinchStrength > 0.65f;
      if (_isBrushHandTracked) {
        var pinchPosition = brushHand.GetPredictedPinchPosition();

        brush.transform.position = pinchPosition;

        if (brushHandIsPinching
            && Vector3.Distance(pinchPosition, _pointBeginPosition)
               < tipRadius
            && !brush.IsBrushing()) {
          brush.Begin();
        }
      }

      if ((!_isBrushHandTracked || !brushHandIsPinching) && brush.IsBrushing()) {
        brush.End();
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      Color pinchOriginColor = Color.blue;

      if (_isTipHandTracked && !brush.IsBrushing()) {
        drawer.color = pinchOriginColor;
        drawer.DrawWireSphere(_pointBeginPosition, tipRadius);
      }
    }
  }

}

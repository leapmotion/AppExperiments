using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  public class DevCommandGesture_Recenter : TwoHandedHeldGesture {

    #region Static Gesture Registration

    /// <summary>
    /// The DevCommand "Recenter" is included by default; for simplicity, we'll also
    /// register the gesture defined in this file with the DevCommandGesture system.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      DevCommandGesture.Register<DevCommandGesture_Recenter>("Recenter");
    }

    #endregion

    #region TwoHandedHeldGesture

    public override bool IsGesturePoseHeld(Hand leftHand, Hand rightHand,
                                           out Vector3 positionOfInterest) {

      var leftHandStraightAmount  = 1f - leftHand.GetFistStrength();
      var rightHandStraightAmount = 1f - rightHand.GetFistStrength();

      var isLeftHandStraight  = leftHandStraightAmount > 0.80f;
      var isRightHandStraight = rightHandStraightAmount > 0.80f;

      if (drawHeldPoseDebug) {
        RuntimeGizmos.BarGizmo.Render(leftHandStraightAmount,
                                      Vector3.down * 0.2f + Vector3.left * 0.20f,
                                      Vector3.up,
                                      isLeftHandStraight ?
                                        LeapColor.white
                                      : LeapColor.amber,
                                      scale: 0.2f);

        RuntimeGizmos.BarGizmo.Render(rightHandStraightAmount,
                                      Vector3.down * 0.2f + Vector3.left * 0.10f,
                                      Vector3.up,
                                      isRightHandStraight ?
                                        LeapColor.white
                                      : LeapColor.brown,
                                      scale: 0.2f);
      }

      var areThumbsParallel = Vector3.Angle(leftHand.RadialAxis(),
                                            rightHand.RadialAxis()) < MAX_ALIGNED_ANGLE;

      var leftMiddleTip = leftHand.Fingers[2].TipPosition.ToVector3();
      var rightMiddleTip = rightHand.Fingers[2].TipPosition.ToVector3();
      var areMiddleFingersTouching = (leftMiddleTip - rightMiddleTip)
                                     .sqrMagnitude < MAX_TOUCHING_DISTANCE_SQR;

      var handsAngle = Vector3.SignedAngle(rightHand.DistalAxis(),
                                           leftHand.DistalAxis(),
                                           rightHand.RadialAxis());
      var areHandsInUpwardTriangle = handsAngle.IsBetween(30f, 135f);

      positionOfInterest = Vector3.zero;
      bool isGesturePoseHeld = isLeftHandStraight
                            && isRightHandStraight
                            && areThumbsParallel
                            && areMiddleFingersTouching
                            && areHandsInUpwardTriangle;
      if (isGesturePoseHeld) {
        positionOfInterest = (leftMiddleTip + rightMiddleTip) / 2f;
      }

      return isGesturePoseHeld;

    }

    #endregion

  }

}

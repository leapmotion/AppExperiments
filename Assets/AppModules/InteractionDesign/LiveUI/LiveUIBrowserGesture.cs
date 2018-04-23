using Leap.Unity.Gestures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LiveUI {

  public class LiveUIBrowserGesture : TwoHandedHeldGesture {

    public override bool IsGesturePoseHeld(Hand leftHand, Hand rightHand,
                                           out Vector3 positionOfInterest) {

      var lThumb = leftHand.GetThumb();
      var lThumbDir = lThumb.Direction.ToVector3();
      var lThumbTip = lThumb.TipPosition.ToVector3();

      var rThumb = rightHand.GetThumb();
      var rThumbDir = rThumb.Direction.ToVector3();
      var rThumbTip = rThumb.TipPosition.ToVector3();

      var thumbsTouching = (lThumbTip - rThumbTip).sqrMagnitude < MAX_TOUCHING_DISTANCE_SQR;

      // Align thumbs; they tend to track at incorrect angles so be more lenient than usual.
      var thumbsAligned = Vector3.Angle(lThumbDir, -rThumbDir) < 150f;

      var lIndex = leftHand.GetIndex();
      var lIndexDir = lIndex.Direction.ToVector3();
      var lIndexTip = lIndex.TipPosition.ToVector3();

      var rIndex = rightHand.GetIndex();
      var rIndexDir = rIndex.Direction.ToVector3();
      var rIndexTip = rIndex.TipPosition.ToVector3();

      var indexesAligned = Vector3.Angle(lIndexDir, rIndexDir) < MAX_ALIGNED_ANGLE;

      positionOfInterest = (lIndexTip + rIndexTip) / 2f;

      return thumbsTouching && thumbsAligned && indexesAligned;

    }

  }

}

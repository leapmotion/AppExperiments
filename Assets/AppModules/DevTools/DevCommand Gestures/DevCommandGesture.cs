using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  public class DevCommandGesture : TwoHandedHeldGesture {

    #region Static API

    /// <summary>
    /// Only use this method if there is already a DevCommand action associated with the 
    /// provided command name. (By default, only the "Recenter" command exists in the
    /// DevCommand system.) Otherwise, supply an Action to take as well.
    /// </summary>
    public static void Register<GestureType>(string commandName)
                         where GestureType : IGesture {
      DevCommandGesturesManager.RegisterCommand<GestureType>(commandName);
    }

    /// <summary>
    /// Registers the command name and the associated Action in the DevCommand system,
    /// and also handles gesture detection for the provided GestureType in the
    /// DevCommandGesture system.
    /// </summary>
    public static void Register<GestureType>(string commandName,
                                             Action commandAction)
                         where GestureType : IGesture {
      DevCommand.Register(commandName, commandAction);
      DevCommandGesturesManager.RegisterCommand<GestureType>(commandName);
    }

    /// <summary>
    /// Registers the command name and the associated Action in the DevCommand system,
    /// and also handles gesture detection for the provided GestureType in the
    /// DevCommandGesture system. TwoHandedHeldGestures provide position data, so
    /// you can provide a command Action that takes a Vector3 as input.
    /// </summary>
    public static void Register<GestureType>(string commandName,
                                             Action<Vector3> actionWithGesturePosition)
                         where GestureType : TwoHandedHeldGesture {
      DevCommand.Register(commandName, actionWithGesturePosition);
      DevCommandGesturesManager.RegisterPositionCommand<GestureType>(commandName);
    }

    #endregion

    #region Gesture Implementation

    public override bool IsGesturePoseHeld(Hand leftHand, Hand rightHand,
                                          out Vector3 positionOfInterest) {

      var leftThumb = leftHand.GetThumb();
      var leftThumbTip = leftThumb.TipPosition.ToVector3();
      var leftThumbDir = leftThumb.Direction.ToVector3();

      var rightIndex = rightHand.GetIndex();
      var rightIndexTip = rightIndex.TipPosition.ToVector3();
      var rightIndexDir = rightIndex.Direction.ToVector3();

      var tipsTouching = (leftThumbTip - rightIndexTip).sqrMagnitude
                         < MAX_TOUCHING_DISTANCE_SQR;
      if (drawHeldPoseDebug) {
        var touchingAmount = ((leftThumbTip - rightIndexTip).sqrMagnitude
                             - MAX_TOUCHING_DISTANCE_SQR).Map(0, -MAX_TOUCHING_DISTANCE_SQR, 0, 1);
        RuntimeGizmos.BarGizmo.Render(touchingAmount,
                                      Vector3.down * 0.2f + Vector3.right * 0.20f
                                        + Vector3.forward * 0.1f,
                                      Vector3.up,
                                      tipsTouching ?
                                        LeapColor.white
                                      : LeapColor.teal,
                                      scale: 0.2f);
      }

      var tipsAligned = Vector3.Dot(leftThumbDir, rightIndexDir) < -0.70f;
      if (drawHeldPoseDebug) {
        RuntimeGizmos.BarGizmo.Render(Vector3.Dot(leftThumbDir, rightIndexDir)
                                        .Map(-1, 1, 1, 0),
                                      Vector3.down * 0.2f + Vector3.right * 0.20f,
                                      Vector3.up,
                                      tipsAligned ?
                                        LeapColor.white
                                      : LeapColor.periwinkle,
                                      scale: 0.2f);
      }

      var leftIndexPointAmount = leftHand.GetIndexPointAmount();
      var rightIndexPointAmount = rightHand.GetIndexPointAmount();

      var leftIsIndexPointing = leftIndexPointAmount > 0.80f;
      var rightIsIndexPointing = rightIndexPointAmount > 0.80f;
      
      if (drawHeldPoseDebug) {
        RuntimeGizmos.BarGizmo.Render(leftIndexPointAmount,
                                      Vector3.down * 0.2f, Vector3.up,
                                      leftIsIndexPointing ?
                                        LeapColor.white
                                      : LeapColor.lavender,
                                      scale: 0.2f);
        RuntimeGizmos.BarGizmo.Render(rightIndexPointAmount,
                                      Vector3.down * 0.2f + Vector3.right * 0.10f,
                                      Vector3.up,
                                      rightIsIndexPointing ?
                                        LeapColor.white
                                      : LeapColor.red,
                                      scale: 0.2f);
      }

      positionOfInterest = Vector3.zero;
      bool isGesturePoseHeld = tipsTouching
                            && tipsAligned
                            && leftIsIndexPointing
                            && rightIsIndexPointing;
      if (isGesturePoseHeld) {

        var gesturePlaneDir = Vector3.Cross(
          leftHand.Fingers[0].Direction.ToVector3(),
          leftHand.Fingers[1].Direction.ToVector3());
        var upPlaneDir = Vector3.Cross(gesturePlaneDir,
          leftHand.Fingers[0].Direction.ToVector3()).normalized;

        positionOfInterest = (leftThumbTip + rightIndexTip) / 2f
                        + upPlaneDir * (leftHand.GetIndex().bones[1].PrevJoint
                                        - leftHand.GetIndex().TipPosition)
                                       .ToVector3().magnitude;
      }

      return isGesturePoseHeld;
    }

    #endregion

  }

  #region Hand Extensions

  public static class HandExtensions {

    //public static float GetFistAmount(this Hand hand, int fingerId) {
    //  return Vector3.Dot(hand.Fingers[fingerId].Direction.ToVector3(),
    //                     -hand.DistalAxis()).Map(-1, 1, 0, 1);
    //}

    public static float GetIndexPointAmount(this Hand hand) {
      return Vector3.Dot(hand.Fingers[1].Direction.ToVector3(),
                         hand.DistalAxis()).Map(-1, 1, 0, 1)
             * ((Vector3.Dot(hand.Fingers[2].Direction.ToVector3(),
                            -hand.DistalAxis())
                 + Vector3.Dot(hand.Fingers[3].Direction.ToVector3(),
                              -hand.DistalAxis())
                 + Vector3.Dot(hand.Fingers[4].Direction.ToVector3(),
                              -hand.DistalAxis()))
                / 3).Map(-1, 1, 0, 1);
    }

  }

  #endregion

}

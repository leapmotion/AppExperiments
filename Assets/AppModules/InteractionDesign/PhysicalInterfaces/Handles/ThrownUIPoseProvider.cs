using Leap.Unity.Attributes;
using Leap.Unity.Layout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class ThrownUIPoseProvider : MonoBehaviour, IPoseProvider {

    [SerializeField, ImplementsInterface(typeof(IKinematicStateProvider))]
    private MonoBehaviour _handleKinematicStateProvider;
    public IKinematicStateProvider handleKinematicStateProvider {
      get {
        return _handleKinematicStateProvider as IKinematicStateProvider;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IAttachmentProvider))]
    private MonoBehaviour _handleAttachmentPoseProvider;
    public IAttachmentProvider handleAttachmentPoseProvider {
      get {
        return _handleAttachmentPoseProvider as IAttachmentProvider;
      }
    }

    public float optimalHeightFromHead = 0.20f;

    public float optimalDistance = 0.60f;

    public bool flip180 = false;

    [Header("Debug")]
    public bool drawDebug = false;

    public Pose GetPose() {
      var handleKinematicState = handleKinematicStateProvider.GetKinematicState();

      var handlePose = handleKinematicState.pose;

      var handleToAttachedUIPose = handleAttachmentPoseProvider.GetHandleToAttachmentPose();
      
      var layoutPos = LayoutUtils.LayoutThrownUIPosition2(
                                    Camera.main.transform.ToPose(),

                                    //handlePose.position,

                                    handlePose.Then(handleToAttachedUIPose).position,

                                    handleKinematicState.movement.velocity,
                                    optimalHeightFromHead: optimalHeightFromHead,
                                    optimalDistance: optimalDistance);

      if (drawDebug) {
        DebugPing.Ping(handlePose, LeapColor.red, 0.2f);
        DebugPing.PingCapsule(handlePose.position, layoutPos, LeapColor.purple, 0.2f);
        DebugPing.Ping(layoutPos, LeapColor.blue, 0.2f);
      }

      var solvedHandlePose =
        new Pose(layoutPos,
                 Utils.FaceTargetWithoutTwist(layoutPos,
                                              Camera.main.transform.position,
                                              flip180))
            .Then(handleToAttachedUIPose.inverse);

      return solvedHandlePose;
    }

  }

}

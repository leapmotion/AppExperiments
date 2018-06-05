using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class FaceMainCameraPoseFilter : MonoBehaviour, IPoseFilter {

    public bool flip180 = false;

    public Pose Filter(Pose inputPose) {
      if (!this.enabled) return inputPose;

      return inputPose.WithRotation(
        Utils.FaceTargetWithoutTwist(inputPose.position,
                                     Camera.main.transform.position,
                                     flip180));
    }

  }

}

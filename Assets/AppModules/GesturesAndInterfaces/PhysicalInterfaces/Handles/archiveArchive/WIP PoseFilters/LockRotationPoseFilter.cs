using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class LockRotationPoseFilter : MonoBehaviour, IPoseFilter {

    private Quaternion _lockedRotation = Quaternion.identity;

    void Start() {
      _lockedRotation = this.transform.rotation;
    }

    public Pose Filter(Pose inputPose) {
      if (!this.enabled) return inputPose;

      return inputPose.WithRotation(_lockedRotation);
    }

  }

}

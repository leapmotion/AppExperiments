using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public struct KinematicState {

    public Pose pose;
    public Movement movement;

    public KinematicState(Pose pose, Movement movement) {
      this.pose = pose;
      this.movement = movement;
    }


    public void Integrate(float deltaTime) {
      pose = pose.Integrated(movement, deltaTime);
    }

    public void Integrate(Vector3 linearAcceleration,
                          float deltaTime) {
      movement.Integrate(linearAcceleration, deltaTime);
      pose = pose.Integrated(movement, deltaTime);
    }

    public void Integrate(Vector3 linearAcceleration,
                          Vector3 angularAcceleration,
                          float deltaTime) {
      movement.Integrate(linearAcceleration, angularAcceleration, deltaTime);
      pose = pose.Integrated(movement, deltaTime);
    }

  }

  public static class PoseExtensions {

    public static Pose Integrated(this Pose thisPose, Movement movement, float deltaTime) {
      thisPose.position = movement.velocity * deltaTime + thisPose.position;

      if (movement.angularVelocity.sqrMagnitude > 0.00001f) {
        var angVelMag = movement.angularVelocity.magnitude;
        thisPose.rotation = Quaternion.AngleAxis(angVelMag * deltaTime,
                                                 movement.angularVelocity / angVelMag)
                            * thisPose.rotation;
      }

      return thisPose;
    }

  }

}

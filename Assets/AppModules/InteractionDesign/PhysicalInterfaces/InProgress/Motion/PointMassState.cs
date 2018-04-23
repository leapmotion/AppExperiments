

using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public struct PointMassState {
    public KinematicState kinematicState;
    public float mass;
    public Vector3 acceleration;
    public Vector3 angularAcceleration;

    public void Accelerate(Vector3 linearAcceleration) {
      acceleration += linearAcceleration;
    }

    public void Torque(Vector3 angleAxisAcceleration) {
      angularAcceleration += angleAxisAcceleration;
    }

    public void IntegrateAccelerations(float deltaTime) {
      kinematicState.Integrate(acceleration,
                               angularAcceleration,
                               deltaTime);
    }
  }

}
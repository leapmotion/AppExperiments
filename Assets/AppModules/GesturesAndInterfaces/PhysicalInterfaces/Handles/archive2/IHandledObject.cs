using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {
  
  public interface IHandledObject {

    void MoveByHandle(zz2Old_IHandle attachedHandle,
                      Pose toPose,
                      Vector3 aroundPivot,
                      out Pose newHandleTargetPose);

  }

}

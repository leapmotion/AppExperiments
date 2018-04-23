
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IPoseFilter {

    Pose Filter(Pose inputPose);

  }

}

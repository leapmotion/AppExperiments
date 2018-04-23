using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  /// <summary>
  /// A basic implementation of the HandleBase handle using the Transform as the Pose
  /// source.
  /// </summary>
  public class zzOld_TransformHandle : zzOld_HandleBase {

    public override Pose pose {
      get { return transform.ToPose(); }
      protected set {
        transform.SetWorldPose(value);
      }
    }

  }

}

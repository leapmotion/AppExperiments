using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IAttachmentProvider {

    ///// <summary>
    ///// Gets the pose of whichever object is considered the "handle", or "base."
    ///// </summary>
    //Pose GetHandlePose();

    ///// <summary>
    ///// Gets the pose of whichever object is considered "attached" to the handle.
    ///// </summary>
    //Pose GetAttachmentPose();

    /// <summary>
    /// Gets the delta pose that transforms from the handle pose to the attachment pose.
    /// </summary>
    Pose GetHandleToAttachmentPose();

  }

}

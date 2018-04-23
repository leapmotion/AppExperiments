using System;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IMoveToPose {

    /// <summary>
    /// Gets or sets the target pose.
    /// </summary>
    Pose targetPose { get; set; }

    /// <summary>
    /// Begins movement to the target pose, which can optionally be specified as an
    /// argument. The duration of movement can also optionally be specified.
    /// </summary>
    void MoveToTarget(Pose? targetPose = null, float? duration = null);

    /// <summary>
    /// Cancels any more movement to the target position.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Called every time the object has progressed in moving to its target pose.
    /// </summary>
    event Action OnMovementUpdate;

    /// <summary>
    /// Called when the object has reached its target pose.
    /// </summary>
    event Action OnReachTarget;

  }


}
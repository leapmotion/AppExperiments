using System;
using UnityEngine;

namespace Leap.Unity.Animation {

  public interface IMoveToTarget {

    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    Vector3 targetPosition { get; set; }

    /// <summary>
    /// Begins movement to the target position.
    /// </summary>
    void MoveToTarget(Vector3? target = null, float? duration = null);

    /// <summary>
    /// Cancels any more movement to the target position.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Called when the object has reached its target position.
    /// </summary>
    event Action OnReachTarget;

  }


}
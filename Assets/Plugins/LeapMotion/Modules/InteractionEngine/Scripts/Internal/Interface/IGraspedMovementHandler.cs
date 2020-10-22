/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Interaction {

  /// <summary>
  /// An IGraspedMovementHandler takes in a target position and rotation (a "pose",
  /// usually provided by an IGraspedPoseHandler) and is responsible for attempting to
  /// move an interaction object to that target pose.
  /// </summary>
  public interface IGraspedMovementHandler {

    /// <summary>
    /// Called by an interaction object when its grasped pose handler has determined a
    /// target pose; this method should attempt to move the interaction object to match
    /// that pose.
    /// </summary>
    void MoveTo(Vector3 solvedPosition, Quaternion solvedRotation,
                InteractionBehaviour interactionObj, bool justGrasped);

  }

}

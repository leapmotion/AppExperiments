using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public static class PhysicalInterfaceUtils {

    #region Constants

    /// <summary>
    /// The minimum speed past which a released object should be considered thrown,
    /// and beneath which a released object should be considered placed.
    /// </summary>
    public const float MIN_THROW_SPEED = 0.70f;
    public const float MIN_THROW_SPEED_SQR = MIN_THROW_SPEED * MIN_THROW_SPEED;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 10 m/s represents
    /// about a quarter of the speed of the world's fastest fastball.
    /// </summary>
    public const float MID_THROW_SPEED = 10.00f;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 40 m/s is about the
    /// speed of the fastest fast-ball. (~90 mph.)
    /// </summary>
    public const float MAX_THROW_SPEED = 40.00f;

    /// <summary>
    /// A standard speed for calculating e.g. how much time it should take for an
    /// element to move a given distance.
    /// </summary>
    public const float STANDARD_SPEED = 1.00f;

    /// <summary>
    /// A standard minimum speed under which an object is considered to be standing
    /// still.
    /// </summary>
    public const float MIN_MOVING_SPEED = 0.001f;

    /// <summary>
    /// As MIN_MOVING_SPEED, but squared, for checks against sqrMagnitude.
    /// </summary>
    public const float MIN_MOVING_SPEED_SQR = MIN_MOVING_SPEED * MIN_MOVING_SPEED;

    /// <summary>
    /// A distance representing being well within arms-reach without being too close to
    /// the head.
    /// </summary>
    public const float OPTIMAL_UI_DISTANCE = 0.60f;

    #endregion

    #region Motion

    public static Pose SmoothMove(Pose prev, Pose current, Pose target,
                                  float rigidness = 0f) {
      var prevSqrDist = (current.position - prev.position).sqrMagnitude;
      var lerpFilter = prevSqrDist.Map(0.0f, 0.4f, 0.2f, 1f);

      //lerpFilter = Vector3.Dot((current.position - prev.position),
      //                         (target.position - current.position))
      //                    .Map(0f, 1f, 0f, 1f);

      var prevAngle = Quaternion.Angle(current.rotation, prev.rotation);
      var slerpFilter = prevAngle.Map(0.0f, 16f, 0.01f, 1f);

      //slerpFilter = Vector3.Dot(current.rotation.From(prev.rotation).ToAngleAxisVector(),
      //                          target.rotation.From(current.rotation).ToAngleAxisVector())
      //                     .Map(0f, 1f, 0f, 1f);

      var sqrDist = (target.position - current.position).sqrMagnitude;
      float angle = Quaternion.Angle(current.rotation, target.rotation);

      var smoothLerpCoeff = sqrDist.Map(0.00001f, 0.0004f, 0.2f, 0.8f) * lerpFilter;
      var rigidLerpCoeff = 1f;
      var effLerpCoeff = Mathf.Lerp(smoothLerpCoeff, rigidLerpCoeff, rigidness.Clamp01());

      var smoothSlerpCoeff = angle.Map(0.3f, 4f, 0.01f, 0.8f) * slerpFilter;
      var rigidSlerpCoeff = 1f;
      var effSlerpCoeff = Mathf.Lerp(smoothSlerpCoeff, rigidSlerpCoeff, rigidness.Clamp01());

      var smoothedPose = new Pose(Vector3.Lerp(current.position,
                                               target.position,
                                               effLerpCoeff),
                                  Quaternion.Slerp(current.rotation,
                                                   target.rotation,
                                                   effSlerpCoeff));

      return smoothedPose;
    }

    #endregion

    #region Interface Positioning

    /// <summary>
    /// IE Workstation Example-style UI layout.
    /// </summary>
    public static Vector3 LayoutThrownUIPosition(Pose userHeadPose,
                                                 Vector3 thrownUIInitPosition,
                                                 Vector3 thrownUIInitVelocity,
                                                 float thrownUIRadius,
                                                 List<Vector3> otherUIPositions,
                                                 List<float> otherUIRadii) {
      // Push velocity away from the camera if necessary.
      Vector3 towardsCamera = (userHeadPose.position - thrownUIInitPosition).normalized;
      float towardsCameraness = Mathf.Clamp01(Vector3.Dot(towardsCamera, thrownUIInitVelocity.normalized));
      thrownUIInitVelocity = thrownUIInitVelocity + Vector3.Lerp(Vector3.zero, -towardsCamera * 2.00F, towardsCameraness);

      // Calculate velocity direction on the XZ plane.
      Vector3 groundPlaneVelocity = Vector3.ProjectOnPlane(thrownUIInitVelocity, Vector3.up);
      float groundPlaneDirectedness = groundPlaneVelocity.magnitude.Map(0.003F, 0.40F, 0F, 1F);
      Vector3 groundPlaneDirection = groundPlaneVelocity.normalized;

      // Calculate camera "forward" direction on the XZ plane.
      Vector3 cameraGroundPlaneForward = Vector3.ProjectOnPlane(userHeadPose.rotation * Vector3.forward, Vector3.up);
      float cameraGroundPlaneDirectedness = cameraGroundPlaneForward.magnitude.Map(0.001F, 0.01F, 0F, 1F);
      Vector3 alternateCameraDirection = (userHeadPose.rotation * Vector3.forward).y > 0F ? userHeadPose.rotation * Vector3.down : userHeadPose.rotation * Vector3.up;
      cameraGroundPlaneForward = Vector3.Slerp(alternateCameraDirection, cameraGroundPlaneForward, cameraGroundPlaneDirectedness);
      cameraGroundPlaneForward = cameraGroundPlaneForward.normalized;

      // Calculate a placement direction based on the camera and throw directions on the XZ plane.
      Vector3 placementDirection = Vector3.Slerp(cameraGroundPlaneForward, groundPlaneDirection, groundPlaneDirectedness);

      // Calculate a placement position along the placement direction between min and max placement distances.
      float minPlacementDistance = 0.25F;
      float maxPlacementDistance = 0.51F;
      Vector3 placementPosition = userHeadPose.position + placementDirection * Mathf.Lerp(minPlacementDistance, maxPlacementDistance,
                                                                                    (groundPlaneDirectedness * thrownUIInitVelocity.magnitude)
                                                                                    .Map(0F, 1.50F, 0F, 1F));

      // Don't move far if the initial velocity is small.
      float overallDirectedness = thrownUIInitVelocity.magnitude.Map(0.00F, 3.00F, 0F, 1F);
      placementPosition = Vector3.Lerp(thrownUIInitPosition, placementPosition, overallDirectedness * overallDirectedness);

      // Enforce placement height.
      float placementHeightFromCamera = -0.30F;
      placementPosition.y = userHeadPose.position.y + placementHeightFromCamera;

      // Enforce minimum placement away from user.
      Vector2 cameraXZ = new Vector2(userHeadPose.position.x, userHeadPose.position.z);
      Vector2 stationXZ = new Vector2(placementPosition.x, placementPosition.z);
      float placementDist = Vector2.Distance(cameraXZ, stationXZ);
      if (placementDist < minPlacementDistance) {
        float distanceLeft = (minPlacementDistance - placementDist) + thrownUIRadius;
        Vector2 xzDisplacement = (stationXZ - cameraXZ).normalized * distanceLeft;
        placementPosition += new Vector3(xzDisplacement[0], 0F, xzDisplacement[1]);
      }

      return placementPosition;
    }

    /// <summary>
    /// Original LeapPaint-style UI layout.
    /// </summary>
    public static Vector3 LayoutThrownUIPosition2(Pose userHeadPose,
                                                  Vector3 initPosition,
                                                  Vector3 initVelocity,
                                                  float optimalHeightFromHead = 0f,
                                                  float optimalDistance = OPTIMAL_UI_DISTANCE) {
      Vector3 headPosition = userHeadPose.position;
      Quaternion headRotation = userHeadPose.rotation;

      Vector3 workstationPosition = Vector3.zero;
      //bool modifyHeight = true;
      if (initVelocity.magnitude < MIN_THROW_SPEED) {
        // Just use current position as the position to choose.
        workstationPosition = initPosition;
        //modifyHeight = false;
      }
      else {
        // Find projection direction
        Vector3 projectDirection;
        Vector3 groundAlignedInitVelocity = new Vector3(initVelocity.x, 0F, initVelocity.z);
        Vector3 effectiveLookDirection = headRotation * Vector3.forward;
        effectiveLookDirection = new Vector3(effectiveLookDirection.x, 0F, effectiveLookDirection.z);
        if (effectiveLookDirection.magnitude < 0.01F) {
          if (effectiveLookDirection.y > 0F) {
            projectDirection = headRotation * -Vector3.up;
          }
          else {
            projectDirection = headRotation * -Vector3.up;
          }
        }
        if (initVelocity.magnitude < 0.5F || groundAlignedInitVelocity.magnitude < 0.01F) {
          projectDirection = effectiveLookDirection;
        }
        else {
          projectDirection = groundAlignedInitVelocity;
        }

        // Add a little bit of the effective look direction to the projectDirection to
        // skew towards winding up in front of the user unless they really throw it hard
        // behind them.
        float forwardSkewAmount = 1F;
        projectDirection += effectiveLookDirection * forwardSkewAmount;
        projectDirection = projectDirection.normalized;

        // Find good workstation position based on projection direction
        Vector3 workstationDirection = (initPosition + (projectDirection * 20F) - headPosition);
        Vector3 groundAlignedWorkstationDirection = new Vector3(workstationDirection.x, 0F, workstationDirection.z).normalized;
        workstationPosition = headPosition
          + Vector3.down * optimalHeightFromHead
          + optimalDistance * groundAlignedWorkstationDirection;

        // Allow the WearableManager to pick a new location if the target location overlaps with another workstation
        //workstationPosition = _manager.ValidateTargetWorkstationPosition(workstationPosition, this);
      }

      return workstationPosition;

      // Find a good workstation orientation
      //Vector3 optimalLookVector = GetOptimalOrientationLookVector(centerEyeAnchor, workstationPosition);

      // Set the workstation target transform.
      //toSet.position = new Vector3(workstationPosition.x,
      //  (modifyHeight ? centerEyeAnchor.position.y + GetOptimalWorkstationVerticalOffset() : workstationPosition.y),
      //  workstationPosition.z);
      //toSet.rotation = Quaternion.LookRotation(optimalLookVector);
    }

    #endregion

  }

  public static class FloatExtensions {
    public static float Clamp01(this float f) {
      return Mathf.Clamp01(f);
    }
  }

}

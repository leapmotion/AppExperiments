using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  public static class LayoutUtils {

    public static Vector3 LayoutThrownUIPosition(Pose userHeadPose,
                                                 Vector3 thrownUIInitPosition,
                                                 Vector3 thrownUIInitVelocity) {
      List<Vector3> otherUIPositions = Pool<List<Vector3>>.Spawn();
      List<float> otherUIRadii = Pool<List<float>>.Spawn();
      try {
        return LayoutThrownUIPosition(userHeadPose,
                                      thrownUIInitPosition,
                                      thrownUIInitVelocity,
                                      0.1f, otherUIPositions, otherUIRadii);
      }
      finally {
        otherUIPositions.Clear();
        Pool<List<Vector3>>.Recycle(otherUIPositions);

        otherUIRadii.Clear();
        Pool<List<float>>.Recycle(otherUIRadii);
      }
    }

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
                                                  float optimalDistance = PhysicalInterfaceUtils.OPTIMAL_UI_DISTANCE) {
      Vector3 headPosition = userHeadPose.position;
      Quaternion headRotation = userHeadPose.rotation;

      Vector3 workstationPosition = Vector3.zero;
      //bool modifyHeight = true;
      if (initVelocity.magnitude < PhysicalInterfaceUtils.MIN_THROW_SPEED) {
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

  }

}

using System.Collections;
using System.Collections.Generic;
using Leap.Unity.PhysicalInterfaces;
using UnityEngine;
using Leap.Unity.Attributes;

namespace Leap.Unity.Layout {

  public class UIPoseProvider : MonoBehaviour,
                                IPoseProvider {

    #region Inspector

    [SerializeField, ImplementsInterface(typeof(zzOld_IHandle))]
    private MonoBehaviour _uiHandle;
    public zzOld_IHandle uiHandle {
      get {
        return _uiHandle as zzOld_IHandle;
      }
      set {
        _uiHandle = value as MonoBehaviour;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IWorldPositionProvider))]
    private MonoBehaviour _uiLookPositionProvider;
    public  IWorldPositionProvider uiLookPositionProvider {
      get {
        return _uiLookPositionProvider as IWorldPositionProvider;
      }
    }

    public bool flip180 = false;

    public float layoutDistanceMultiplier = 1f;

    [Header("Runtime Gizmo Debugging")]
    public bool drawDebug = false;

    #endregion

    public Pose GetPose() {
      return new Pose(GetTargetPosition(), GetTargetRotation());
    }

    public Vector3 GetTargetPosition() {
      Vector3 layoutPos;

      if (!uiHandle.wasThrown) {
        layoutPos = uiHandle.pose.position;

        if (drawDebug) {
          DebugPing.Ping(layoutPos, Color.white);
        }
      }
      else {
        // When the UI is thrown, utilize the static thrown UI util to calculate a decent
        // final position relative to the user's head given the position and velocity of
        // the throw.
        layoutPos = LayoutUtils.LayoutThrownUIPosition2(Camera.main.transform.ToPose(),
                                                       uiHandle.pose.position,
                                                       uiHandle.movement.velocity,
                                                       layoutDistanceMultiplier);

        // However, UIs whose central "look" anchor is in a different position than their
        // grabbed/thrown anchor shouldn't be placed directly at the determined position.
        // Rather, we need to adjust this position so that the _look anchor,_ not the
        // thrown handle, winds up in the calculated position from the throw.

        // Start with the "final" pose as it would currently be calculated.
        // We need to know the target rotation of the UI based on the target position in
        // order to adjust the final position properly.
        Pose finalUIPose = new Pose(layoutPos, GetTargetRotationForPosition(layoutPos));

        // We assume the uiAnchorHandle and the uiLookAnchor are rigidly connected.
        Vector3 curHandleToLookAnchorOffset = (uiLookPositionProvider.GetTargetWorldPosition()
                                               - uiHandle.pose.position);

        // We undo the current rotation of the UI handle and apply that rotation
        // on the current world-space offset between the handle and the look anchor.
        // Then we apply the final rotation of the UI to this unrotated offset vector,
        // giving us the expected final offset between the position that was calculated
        // by the layout function and the handle.
        Vector3 finalRotatedLookAnchorOffset =
          finalUIPose.rotation
            * (Quaternion.Inverse(uiHandle.pose.rotation)
               * curHandleToLookAnchorOffset);

        // We adjust the layout position by this offset, so now the UI should wind up
        // with its lookAnchor at the calculated location instead of the handle.
        layoutPos = layoutPos - finalRotatedLookAnchorOffset;

        // We also adjust any interface positions down a bit.
        layoutPos += (Camera.main.transform.parent != null ?
                      -Camera.main.transform.parent.up
                      : Vector3.down) * 0.19f;

        if (drawDebug) {
          DebugPing.Ping(layoutPos, Color.red);
        }
      }

      return layoutPos;
    }

    public Quaternion GetTargetRotation() {
      return GetTargetRotationForPosition(uiLookPositionProvider.GetTargetWorldPosition());
    }

    private Quaternion GetTargetRotationForPosition(Vector3 worldPosition) {
      return Utils.FaceTargetWithoutTwist(worldPosition,
                                             Camera.main.transform.position,
                                             flip180: flip180);
    }

  }

}

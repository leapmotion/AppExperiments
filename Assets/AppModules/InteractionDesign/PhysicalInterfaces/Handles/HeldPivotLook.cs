using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Leap.Unity.Layout;
using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class HeldPivotLook : MonoBehaviour, IAttachmentProvider {

    public IntObj intObj;

    public Transform lookFrom;
    public Transform lookTarget;

    public bool flip180;

    [SerializeField, Disable]
    private Pose _handleToPanelPose;
    public Pose GetHandleToAttachmentPose() {
      return _handleToPanelPose;
    }

    void OnValidate() {
      if (lookFrom != null) {
        _handleToPanelPose = lookFrom.ToPose().From(intObj.transform.ToPose());
      }
    }

    void OnEnable() {
      intObj.OnGraspedMovement -= onGraspedMovement;
      intObj.OnGraspedMovement += onGraspedMovement;
    }

    void OnDisable() {
      intObj.OnGraspedMovement -= onGraspedMovement;
    }

    void Reset() {
      if (intObj == null) intObj = GetComponent<IntObj>();
    }

    private void onGraspedMovement(Vector3 oldPos, Quaternion oldRot,
                                   Vector3 newPos, Quaternion newRot,
                                   List<InteractionController> controllers) {

      if (!gameObject.activeInHierarchy || !this.enabled) return;

      // An important reason this method works is that even though the various rigidbodies
      // have constantly-fluctuating poses with respect to one another, there are rigid
      // "ideal" poses and relative poses stored on Start() that allow target positions
      // to always be rigidly calculatable.

      // Introduce any sort of dynamic "relative pose" calculation INSIDE this method,
      // and you'll see a ton of erratic behaviour and terrible instability.

      var newPose = new Pose(newPos, newRot);

      var newPanelPose = newPose.Then(_handleToPanelPose);

      var graspingCentroid = controllers.Query()
                                        .Select(c => c.GetGraspPoint())
                                        .Fold((sum, p) => p + sum)
                             / controllers.Count;

      var targetLookFromPose = PivotLook.Solve(newPanelPose,
                                               graspingCentroid,
                                               lookTarget.position,
                                               Vector3.up,
                                               flip180: flip180);
      var targetHandlePose = targetLookFromPose.Then(_handleToPanelPose.inverse);

      intObj.rigidbody.MovePosition(targetHandlePose.position);
      intObj.rigidbody.MoveRotation(targetHandlePose.rotation);
      intObj.rigidbody.position = targetHandlePose.position;
      intObj.rigidbody.rotation = targetHandlePose.rotation;
    }

  }

}

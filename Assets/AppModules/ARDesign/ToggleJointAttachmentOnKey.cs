using System;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class ToggleJointAttachmentOnKey : MonoBehaviour {

    public Joint attachmentJoint;

    public Rigidbody bodyToAttach;

    public KeyCode toggleAttachmentKey = KeyCode.A;

    private void Reset() {
      if (attachmentJoint == null) attachmentJoint = GetComponent<Joint>();
    }

    private void Start() {
      refreshConnectedAnchor();
    }

    private void Update() {
      if (Input.GetKeyDown(toggleAttachmentKey)) {
        ToggleAttachment();
      }
    }

    public void ToggleAttachment() {
      if (attachmentJoint == null) return;
      if (bodyToAttach == null) return;

      if (attachmentJoint.connectedBody == null) {
        attachmentJoint.connectedBody = bodyToAttach;
      }
      else {
        attachmentJoint.connectedBody = null;
      }

      refreshConnectedAnchor();
    }

    private void refreshConnectedAnchor() {
      attachmentJoint.autoConfigureConnectedAnchor = false;

      if (attachmentJoint == null) return;
      if (attachmentJoint.connectedBody == null) return;

      if (attachmentJoint.connectedBody != null
          && attachmentJoint.connectedBody == bodyToAttach) {
        attachmentJoint.connectedAnchor
          = (bodyToAttach.GetPose().inverse
             * attachmentJoint.GetComponent<Rigidbody>().GetPose()).position;
      }
    }

  }

  public static class JointRigidbodyExtensions {
    
    /// <summary>
    /// Gets the pose defined by body.position and body.rotation.
    /// </summary>
    public static Pose GetPose(this Rigidbody body) {
      return new Pose(body.position, body.rotation);
    }

    /// <summary>
    /// Sets body.position and body.rotation using the argument Pose.
    /// </summary
    public static void SetPose(this Rigidbody body, Pose pose) {
      body.position = pose.position;
      body.rotation = pose.rotation;
    }

    /// <summary>
    /// Calls body.MovePosition() and body.MoveRotation() using the argument Pose.
    /// </summary
    public static void MovePose(this Rigidbody body, Pose pose) {
      body.MovePosition(pose.position);
      body.MoveRotation(pose.rotation);
    }

  }

}
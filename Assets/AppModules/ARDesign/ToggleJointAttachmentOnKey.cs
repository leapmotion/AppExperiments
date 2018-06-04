using System;
using UnityEngine;

namespace Leap.Unity.AR.Testing {

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

}
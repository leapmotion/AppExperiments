/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using Leap.Unity;
using Leap.Unity.Attachments;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Attachments {

  public class AttachmentHandEnableDisable : MonoBehaviour {

    public AttachmentHand attachmentHand;

    void Update() {
      // Deactivation trigger
      if (!attachmentHand.isTracked && attachmentHand.gameObject.activeSelf) {
        attachmentHand.gameObject.SetActive(false);
      }

      // Reactivation trigger
      if (attachmentHand.isTracked && !attachmentHand.gameObject.activeSelf) {
        attachmentHand.gameObject.SetActive(true);
      }
    }

  }

}

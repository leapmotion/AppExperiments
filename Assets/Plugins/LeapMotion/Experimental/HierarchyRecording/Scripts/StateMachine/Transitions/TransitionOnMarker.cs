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

namespace Leap.Unity.Recording {

  public class TransitionOnMarker : TransitionBehaviour {

    public MarkerController controller;
    public string markerName;
    public MarkerController.MarkerTest condition = MarkerController.MarkerTest.AfterMarkerStarts;

    private void OnEnable() {
      controller.updateMarkersIfNeeded();
    }

    private void Update() {
      if (controller.TestMarker(markerName, condition)) {
        Transition();
      }
    }
  }
}

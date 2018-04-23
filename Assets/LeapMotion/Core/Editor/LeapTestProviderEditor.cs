/******************************************************************************
  * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
  * Leap Motion proprietary and  confidential.                                 *
  *                                                                            *
  * Use subject to the terms of the Leap Motion SDK Agreement available at     *
  * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
  * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Leap.Unity {

  [CustomEditor(typeof(LeapTestProvider))]
  public class LeapTestProviderEditor : CustomEditorBase<LeapTestProvider> {

    protected override void OnEnable() {
      base.OnEnable();

      // DELETEME old LeapTestProvider code
      //specifyConditionalDrawing("testPoseMode",
      //                          (int)LeapTestProvider.TestPoseMode.CapturedPose,
      //                          "poseFolder",
      //                          "poseName",
      //                          "captureModeEnabled",
      //                          "poseCaptureSource",
      //                          "captureKey");
    }

  }
}
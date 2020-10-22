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
using UnityEditor;
using UnityEngine;
using Leap.Unity;

namespace Leap.Unity {

  [CanEditMultipleObjects]
  [CustomEditor(typeof(XRHeightOffset))]
  public class XRHeightOffsetEditor : CustomEditorBase<XRHeightOffset> {

    protected override void OnEnable() {
      base.OnEnable();

      specifyConditionalDrawing(conditionalName: "recenterOnKey",
                                dependantProperties: "recenterKey");
    }

  }

}

/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEditor;

namespace Leap.Unity.GraphicalRenderer {

  [CustomPropertyDrawer(typeof(LeapBakedRenderer))]
  public class LeapBakedRendererDrawer : LeapMesherBaseDrawer {

    protected override void init(SerializedProperty property) {
      base.init(property);

      drawProperty("_motionType");
      drawProperty("_createMeshRenderers");
    }
  }
}

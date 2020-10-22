/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;

namespace Leap.Unity.Attributes {

  public class InspectorNameAttribute : CombinablePropertyAttribute, IFullPropertyDrawer {

    public readonly string name;

    public InspectorNameAttribute(string name) {
      this.name = name;
    }

#if UNITY_EDITOR
    public void DrawProperty(Rect rect, UnityEditor.SerializedProperty property,
                             GUIContent label) {
      label.text = name;
      UnityEditor.EditorGUI.PropertyField(rect, property, label, includeChildren: true);
    }
#endif
  }
}

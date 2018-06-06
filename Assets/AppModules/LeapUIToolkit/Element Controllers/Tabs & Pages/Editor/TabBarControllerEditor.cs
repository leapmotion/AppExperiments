using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Leap.Unity;

[CustomEditor(typeof(TabBarController))]
public class TabBarControllerEditor : CustomEditorBase<TabBarController> {

  protected override void OnEnable() {
    base.OnEnable();

    specifyCustomDecorator("_localOffsetFromTab", drawSetLocalOffsetButton);
  }

  private void drawSetLocalOffsetButton(SerializedProperty property) {
    if (GUILayout.Button("Set Local Offset to Local Position")) {
      target.SetLocalOffsetToCurrentPosition();
    }
    if (GUILayout.Button("Set Local Position to Local Offset")) {
      target.SetLocalPositionToLocalOffset();
    }
  }

}
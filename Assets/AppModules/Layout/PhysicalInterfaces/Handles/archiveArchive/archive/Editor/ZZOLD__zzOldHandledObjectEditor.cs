using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  [CustomEditor(typeof(ZZOLD__zzOldHandledObject), editorForChildClasses: true)]
  public class ZZOLD__zzOldHandledObjectEditor : CustomEditorBase<ZZOLD__zzOldHandledObject> {

    protected override void OnEnable() {
      base.OnEnable();
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      //EditorGUILayout.LabelField("Attached Handles", EditorStyles.boldLabel);
      //foreach (var handleBehaviour in target.attachedHandles
      //                                      .Query()
      //                                      .Select(h => h as MonoBehaviour)
      //                                      .Where(b => b != null)) {
      //  EditorGUILayout.LabelField(handleBehaviour.name);
      //}
    }

  }

}
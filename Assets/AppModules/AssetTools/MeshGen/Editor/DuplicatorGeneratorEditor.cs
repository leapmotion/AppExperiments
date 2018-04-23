using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Leap.Unity.Query;

namespace Leap.Unity.MeshGen {

  [CustomEditor(typeof(DuplicatorGenerator), true)]
  public class DuplicatorGeneratorEditor
               : MetaMeshGeneratorEditorBase<DuplicatorGenerator> {

    protected override void OnEnable() {
      base.OnEnable();

      //specifyCustomDecorator("inputGeneratorTypeName", decorateInputGeneratorTypeName);
    }

    private Editor _inputMeshGeneratorEditor;

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      EditorGUILayout.Space();
      EditorGUILayout.LabelField(new GUIContent("Duplicated Generator"),
                                 EditorStyles.boldLabel);
      var inputMeshGeneratorProperty = serializedObject.FindProperty("_inputMeshGenerator");
      if (inputMeshGeneratorProperty.objectReferenceValue != null) {
        var generatorEditor = Editor.CreateEditor(inputMeshGeneratorProperty.objectReferenceValue);
        generatorEditor.OnInspectorGUI();
      }
    }

  }

}
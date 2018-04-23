using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Leap.Unity.Query;

namespace Leap.Unity.MeshGen {

  [CustomEditor(typeof(MeshGeneratorBehaviour))]
  public class MeshGeneratorBehaviourEditor : MeshGenEditorBase<MeshGeneratorBehaviour> {

    protected override void OnEnable() {
      base.OnEnable();
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      // Refresh the mesh representation for the target objects.
      //foreach (var target in targets) {
      //  target.RefreshMesh();
      //}
    }

    #region MeshGenEditorBase Implementation

    protected override string getMeshGeneratorPropertyName() { return "_meshGenerator"; }

    protected override void onBeforeAssignNewGenerator(MeshGenerator newGenerator) {
      //AssetDatabase.GetAssetPath(serializedObject);

      //AssetDatabase.CreateFolder()
    }

    #endregion

  }

}
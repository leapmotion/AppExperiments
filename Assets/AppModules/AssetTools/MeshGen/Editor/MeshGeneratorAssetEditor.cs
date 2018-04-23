using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Leap.Unity.Query;

namespace Leap.Unity.MeshGen {

  [CustomEditor(typeof(MeshGeneratorAsset), true)]
  public class MeshGeneratorAssetEditor : MeshGenEditorBase<MeshGeneratorAsset> {

    private Editor _meshEditor;

    protected override void OnEnable() {
      base.OnEnable();
      
      var serializedMesh = serializedObject.FindProperty("_mesh");
      if (serializedMesh.objectReferenceValue != null) {
        _meshEditor = CreateEditor(serializedMesh.objectReferenceValue);
      }
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      // Refresh the mesh representation for the target objects.
      foreach (var target in targets) {
        target.RefreshMesh();
      }
    }

    #region Mesh Preview GUI

    public override bool HasPreviewGUI() {
      if (_meshEditor != null) {
        return _meshEditor.HasPreviewGUI();
      }

      return false;
    }

    public override GUIContent GetPreviewTitle() {
      if (_meshEditor == null) {
        return base.GetPreviewTitle();
      }

      return _meshEditor.GetPreviewTitle();
    }

    public override void DrawPreview(Rect previewArea) {
      base.DrawPreview(previewArea);

      if (_meshEditor != null) {
        _meshEditor.DrawPreview(previewArea);
      }
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
      base.OnPreviewGUI(r, background);

      if (_meshEditor != null) {
        _meshEditor.OnPreviewGUI(r, background);
      }
    }

    #endregion

    #region MeshGenEditorBase Implementation

    protected override string getMeshGeneratorPropertyName() { return "_meshGenerator"; }

    protected override void onBeforeAssignNewGenerator(MeshGenerator newGenerator) {
      // Add the object to the MeshGeneratorAsset.
      AssetDatabase.AddObjectToAsset(newGenerator, target);

      // Save asset database.
      AssetDatabase.SaveAssets();
    }

    #endregion

  }

}
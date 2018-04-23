using Leap.Unity.Attributes;
using Leap.Unity.Meshing;
using System;
using System.Reflection;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public class DuplicatorGenerator : MeshGenerator {
    
    [Header("Duplicator Input Type")]
    [ImplementsTypeNameDropdown(typeof(MeshGenerator))]
    [OnEditorChange("refreshInputMeshGenerator")]
    public string inputGeneratorTypeName = "";

    [SerializeField]
    private MeshGenerator _inputMeshGenerator;
    private void refreshInputMeshGenerator() {
      if (_inputMeshGenerator != null) {
        //Debug.Log("HAVE generator as input already: " + _inputMeshGenerator.GetType().FullName);
        //Debug.Log("refreshing given requested type name: " + inputGeneratorTypeName);

        if (_inputMeshGenerator.GetType().FullName.Equals(inputGeneratorTypeName)) {
          //Debug.Log("Names match, no action taken to refresh.");
          return;
        }
        else {
          //Debug.Log("Names don't match, destroying it and making a new generator.");
          DestroyImmediate(_inputMeshGenerator, allowDestroyingAssets: true);
        }
      }

      if (String.IsNullOrEmpty(inputGeneratorTypeName)) return;
      var generatorType = Assembly.GetExecutingAssembly().GetType(inputGeneratorTypeName);
      if (generatorType == null) {
        //Debug.Log("Type name not found: " + inputGeneratorTypeName);
        return;
      }

      var newGenerator = (MeshGenerator)CreateInstance(generatorType);
      newGenerator.name = generatorType.Name;
      if (newGenerator == null) {
        //Debug.Log("Failed to create generator from name " + generatorType.Name);
      }

      //newGenerator.hideFlags = HideFlags.None;
      #if UNITY_EDITOR
      var path = UnityEditor.AssetDatabase.GetAssetPath(this);
      //Debug.Log("path is: " + path);
      UnityEditor.AssetDatabase.AddObjectToAsset(newGenerator, path);
      //Debug.Log("Added " + newGenerator + " to the asset at that path.");

      _inputMeshGenerator = newGenerator;

      UnityEditor.AssetDatabase.SaveAssets();

      #endif
    }

    //private void OnValidate() {
    //  refreshInputMeshGenerator(); //nope nope! SaveAssets() calls OnValidate, bad loop
    //}

    [MinValue(1), MaxValue(4096)]
    public int numDuplicates = 1;

    [Serializable]
    public struct GeneratorPropertyDriver {
      public string name;
      public float baseValue;
      public float addValuePerDuplication;
    }
    [Header("Driven Input Generator Properties")]
    public GeneratorPropertyDriver[] drivenProperties;

    private FieldInfo[] _backingDrivenPropertyInfos;
    private FieldInfo[] _drivenPropertyInfos {
      get {
        if (_backingDrivenPropertyInfos == null
            || _backingDrivenPropertyInfos.Length != drivenProperties.Length) {
          _backingDrivenPropertyInfos = new FieldInfo[drivenProperties.Length];
        }
        return _backingDrivenPropertyInfos;
      }
    }

    public override void Generate(Mesh mesh) {
      mesh.Clear();
      if (_inputMeshGenerator == null) return;

      // Collect PropertyInfo objects for driving properties via Reflection.
      for (int i = 0; i < drivenProperties.Length; i++) {
        var fieldInfo = _inputMeshGenerator.GetType().GetField(drivenProperties[i].name);
        if (fieldInfo == null
            || fieldInfo.FieldType != typeof(float)) {
          _drivenPropertyInfos[i] = null;
        }
        else {
          _drivenPropertyInfos[i] = fieldInfo;
        }
      }

      // Perform duplication, driving properties along the way and accumulating each
      // duplicate into a single mesh.
      var singleMeshBuffer = Pool<PolyMesh>.Spawn();
      singleMeshBuffer.Clear();
      var accumulatingMesh = Pool<PolyMesh>.Spawn();
      accumulatingMesh.Clear();
      try {
        for (int i = 0; i < numDuplicates; i++) {
          for (int p = 0; p < drivenProperties.Length; p++) {
            var valueInfo = drivenProperties[p];
            var propInfo = _drivenPropertyInfos[p];
            if (propInfo == null) continue;
            else {
              var propValue = valueInfo.baseValue + i * valueInfo.addValuePerDuplication;

              propInfo.SetValue(_inputMeshGenerator, propValue);
            }
          }

          _inputMeshGenerator.Generate(mesh);

          singleMeshBuffer.Clear();
          singleMeshBuffer.FromUnityMesh(mesh);

          accumulatingMesh.Append(singleMeshBuffer);
        }

        mesh.Clear();
        accumulatingMesh.FillUnityMesh(mesh);
      }
      finally {
        singleMeshBuffer.Clear();
        Pool<PolyMesh>.Recycle(singleMeshBuffer);
        accumulatingMesh.Clear();
        Pool<PolyMesh>.Recycle(accumulatingMesh);
      }
    }
  }

}
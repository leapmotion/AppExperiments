using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Query;
using Leap.Unity.GraphicalRenderer;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class ConvertAllLGPCToGPC : MonoBehaviour {

  // Uncomment to enable the tool.

  //[MenuItem("One-Time/Convert LGPC to GPC")]
  //private static void Convert() {
  //  List<LeapGraphicPaletteController> lgpcs = new List<LeapGraphicPaletteController>();
  //  foreach (var lgpcArr in EditorSceneManager
  //                       .GetActiveScene()
  //                       .GetRootGameObjects()
  //                       .Query()
  //                       .Select(g => g.GetComponentsInChildren<LeapGraphicPaletteController>(true))) {
  //    foreach (var lgpc in lgpcArr) {
  //      lgpcs.Add(lgpc);
  //    }
  //  };

  //  foreach (var lgpc in lgpcs) {
  //    ConvertLGPC(lgpc);
  //  }

  //  Debug.Log("Converted!");
  //}

  private static void ConvertLGPC(ZZOLD_LeapGraphicPaletteController lgpc) {
    GameObject obj = lgpc.gameObject;

    LeapGraphic graphic = lgpc.graphic;
    ColorPalette palette = lgpc.palette;
    int idx = lgpc.colorIndex;

    DestroyImmediate(lgpc);

    var gpc = obj.AddComponent<GraphicPaletteController>();
    gpc.graphic = graphic;
    gpc.palette = palette;
    gpc.restingColorIdx = idx;
  }

}

#endif
using Leap.Unity.Apps.Paint6.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMeshCounterText : MonoBehaviour {

  public TextMesh textOutput;

  public StrokePolyMeshManager strokePolyMeshManager;

  void Update() {
    textOutput.text = "Meshes: " + strokePolyMeshManager.addedMeshCount;
  }

}

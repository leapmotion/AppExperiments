using Leap.Unity.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTriangleCounterText : MonoBehaviour {

  public TextMesh textOutput;

  public StrokePolyMeshManager strokePolyMeshManager;

  void Update() {
    textOutput.text = "Triangles: " + strokePolyMeshManager.addedTriangleCount;
  }

}

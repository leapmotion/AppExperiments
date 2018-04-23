using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.MeshGen {

  public class CubeFrameGenerator : MeshGenerator {

    [MinValue(0f)]
    public Vector2 frameSize = new Vector2(1f, 1f);

    [MinValue(0)]
    public float thickness = 0.2F;

    public override void Generate(Mesh mesh) {
      Generators.GenerateCubeFrame(mesh, frameSize, thickness);
    }
  }

}
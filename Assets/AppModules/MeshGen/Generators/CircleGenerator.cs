using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.MeshGen {

  public class CircleGenerator : MeshGenerator {

    [MinValue(0)]
    public float radius = 0.5F;

    [MinValue(3), MaxValue(2047)]
    public int numDivisions = 16;

    public override void Generate(Mesh mesh) {
      Generators.GenerateCircle(mesh, radius, numDivisions);
    }

  }

}
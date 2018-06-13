using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.MeshGen {

  public class RoundedRectGenerator : MeshGenerator {

    [MinValue(0)]
    public Vector3 extents = new Vector3(1F, 1F, 0.5F);

    [MinValue(0)]
    public float cornerRadius = 0.2F;

    [MinValue(0)]
    public int cornerDivisions = 5;

    public bool withBack = true;

    public override void Generate(Mesh mesh) {
      Generators.GenerateRoundedRectPrism(mesh,
                                          extents,
                                          cornerRadius, cornerDivisions,
                                          withBack);
    }
  }

}
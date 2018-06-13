using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public class TorusGenerator : MeshGenerator {

    [MinValue(0)]
    public float majorRadius = 1F;

    [MinValue(3)]
    public int numMajorSegments = 16;

    [MinValue(0)]
    public float minorRadius = 0.25F;

    [MinValue(2)]
    public int numMinorSegments = 16;

    [Range(0f, 360f)]
    public float minorStartAngle = 0f;

    public bool shadeFlat = false;

    [Range(0f, 360f)]
    public float maxMinorArcAngle = 360f;

    public override void Generate(Mesh mesh) {
      Generators.GenerateTorus(mesh,
                               majorRadius, numMajorSegments,
                               minorRadius, numMinorSegments,
                               minorStartAngle: minorStartAngle,
                               maxMinorArcAngle: maxMinorArcAngle,
                               shadeFlat: shadeFlat);
    }

  }

}
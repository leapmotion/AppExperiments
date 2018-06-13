using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public static partial class Generators {

    private static class CircleSupport {

      public static void AddVerts(List<Vector3> outVerts, List<Vector3> outNormals,
                                  List<Vector2> outUVs,
                                  float radius, int numDivisions) {
        var v = outVerts;
        var n = outNormals;
        var uv = outUVs;

        v.Add(Vector3.zero);
        n.Add(Vector3.forward);
        outUVs.Add(Vector2.one * 0.5f);

        var r = Vector3.up;
        var rot = Quaternion.AngleAxis(360f / numDivisions, Vector3.back);
        for (int i = 0; i < numDivisions; i++) {
          v.Add(r * radius);
          n.Add(Vector3.forward);
          uv.Add(Vector2.one * 0.5f + Swizzle.Swizzle.xy(r) * 0.5f);
          r = rot * r;
        }
      }

      public static void AddIndices(List<int> outIndices, int startingVertCount,
                                    int numDivisions) {
        for (int v = 0; v < numDivisions; v++) {
          var a = (v + 1);
          var b = ((v + 1) % numDivisions) + 1;
          outIndices.AddTri(startingVertCount, 0, b, a);
        }

      }

    }

  }

}
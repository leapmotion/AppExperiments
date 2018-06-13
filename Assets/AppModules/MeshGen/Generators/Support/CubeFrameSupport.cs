using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public static partial class Generators {

    // Diagram:
    // https://drive.google.com/open?id=11rqz2I0kX4oJaZ15V3GPit7BLRBL3fiZ

    private static class CubeFrameSupport {

      public static void AddVerts(List<Vector3> outVerts, List<Vector3> outNormals,
                                  float innerWidth, float innerHeight,
                                  float thickness) {
        var v = outVerts;
        var n = outNormals;

        var majW = innerWidth / 2f;
        var majH = innerHeight / 2f;
        var minR = thickness / 2f;

        var left  = Vector3.left;
        var right = Vector3.right;
        var up    = Vector3.up;
        var down  = Vector3.down;
        var forward = Vector3.forward;
        var back  = Vector3.back;


        // Positions

        // Inner Front
        var p0  = majW * left  + majH * up   + minR * forward;
        var p1  = majW * left  + majH * down + minR * forward;
        var p2  = majW * right + majH * down + minR * forward;
        var p3  = majW * right + majH * up   + minR * forward;

        // Outer Front
        var p4  = (majW + minR) * left  + (majH + minR) * up   + minR * forward;
        var p5  = (majW + minR) * left  + (majH + minR) * down + minR * forward;
        var p6  = (majW + minR) * right + (majH + minR) * down + minR * forward;
        var p7  = (majW + minR) * right + (majH + minR) * up   + minR * forward;

        // Outer Back
        var p8  = (majW + minR) * left  + (majH + minR) * up   + minR * back;
        var p9  = (majW + minR) * left  + (majH + minR) * down + minR * back;
        var p10 = (majW + minR) * right + (majH + minR) * down + minR * back;
        var p11 = (majW + minR) * right + (majH + minR) * up   + minR * back;

        // Inner Back
        var p12 = majW * left  + majH * up   + minR * back;
        var p13 = majW * left  + majH * down + minR * back;
        var p14 = majW * right + majH * down + minR * back;
        var p15 = majW * right + majH * up   + minR * back;


        // Faces & Normals

        // Left Bar
        v.Add(p0, p1, p13, p12);
        n.Add4(right);

        v.Add(p4, p5, p1, p0);
        n.Add4(forward);

        v.Add(p8, p9, p5, p4);
        n.Add4(left);

        v.Add(p12, p13, p9, p8);
        n.Add4(back);

        // Bottom Bar
        v.Add(p1, p2, p14, p13);
        n.Add4(up);

        v.Add(p5, p6, p2, p1);
        n.Add4(forward);

        v.Add(p9, p10, p6, p5);
        n.Add4(down);

        v.Add(p13, p14, p10, p9);
        n.Add4(back);

        // Right Bar
        v.Add(p2, p3, p15, p14);
        n.Add4(left);

        v.Add(p6, p7, p3, p2);
        n.Add4(forward);

        v.Add(p10, p11, p7, p6);
        n.Add4(right);

        v.Add(p14, p15, p11, p10);
        n.Add4(back);

        // Top Bar
        v.Add(p3, p0, p12, p15);
        n.Add4(down);

        v.Add(p7, p4, p0, p3);
        n.Add4(forward);

        v.Add(p11, p8, p4, p7);
        n.Add4(up);

        v.Add(p15, p12, p8, p11);
        n.Add4(back);

      }

      public static void AddIndices(List<int> outIndices, int startingVertCount) {
        List<int> ids = outIndices;
        int v0 = startingVertCount;

        for (int i = 0; i < 16 * 4; i += 4) {
          ids.AddQuad(v0, i + 0, i + 1, i + 2, i + 3);
        }
      }
    }

  }

  static class CubeFrameExtensions {
    public static void Add(this List<Vector3> vs, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3) {
      vs.Add(v0); vs.Add(v1); vs.Add(v2); vs.Add(v3);
    }
    public static void Add4(this List<Vector3> vs, Vector3 v) {
      vs.Add(v); vs.Add(v); vs.Add(v); vs.Add(v);
    }
  }

}
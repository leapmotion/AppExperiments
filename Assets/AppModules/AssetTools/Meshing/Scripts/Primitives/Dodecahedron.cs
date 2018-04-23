using Leap.Unity.Query;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Dodecahedron support. A dodecahedron is a regular polyhedron consisting of
  /// 12 pentagons.
  /// </summary>
  public static class Dodecahedron {

    public static PolyMesh CreatePolyMesh() {
      var mesh = new PolyMesh();

      FillPolyMesh(mesh);

      return mesh;
    }

    public static void FillPolyMesh(PolyMesh mesh) {
      mesh.Fill(Positions, Polygons, PolyMesh.PositionMode.Local);
    }

    public static Vector3[] Positions {
      get {
        return new Vector3[] {
        V(0f,          0f,         1.07047f),
        V(0.713644f,   0f,         0.797878f),
        V(-0.356822f,  0.618f,     0.797878f),
        V(-0.356822f, -0.618f,     0.797878f),
        V(0.797878f,   0.618034f,  0.356822f),
        V(0.797878f,  -0.618f,     0.356822f),
        V(-0.934172f,  0.381966f,  0.356822f),
        V(0.136294f,   1.0f,       0.356822f),
        V(0.136294f,  -1.0f,       0.356822f),
        V(-0.934172f, -0.381966f,  0.356822f),
        V(0.934172f,   0.381966f, -0.356822f),
        V(0.934172f,  -0.381966f, -0.356822f),
        V(-0.797878f,  0.618f,    -0.356822f),
        V(-0.136294f,  1.0f,      -0.356822f),
        V(-0.136294f, -1.0f,      -0.356822f),
        V(-0.797878f, -0.618034f, -0.356822f),
        V(0.356822f,   0.618f,    -0.797878f),
        V(0.356822f,  -0.618f,    -0.797878f),
        V(-0.713644f,  0f,        -0.797878f),
        V(0f,          0f,        -1.07047f)
      };
      }
    }

    public static Polygon[] Polygons {
      get {
        return new Polygon[] {
          N(0, 1, 4, 7, 2),
          N(0, 2, 6, 9, 3),
          N(0, 3, 8, 5, 1),
          N(1, 5, 11, 10, 4),
          N(2, 7, 13, 12, 6),
          N(3, 9, 15, 14, 8),
          N(4, 10, 16, 13, 7),
          N(5, 8, 14, 17, 11),
          N(6, 12, 18, 15, 9),
          N(10, 11, 17, 19, 16),
          N(12, 13, 16, 19, 18),
          N(14, 15, 18, 19, 17)
        };
      }
    }

    private static Vector3 V(float x, float y, float z) {
      return new Vector3(x, y, z);
    }

    private static Polygon N(params int[] verts) {
      return new Polygon() { verts = verts.Query().ToList() };
    }

  }

}
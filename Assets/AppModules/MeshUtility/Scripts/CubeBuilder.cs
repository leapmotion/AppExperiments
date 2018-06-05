using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Face {
  None = 0,
  Back = 1,
  Front = 2,
  All = 3
}

public static class CubeBuilder {
  private static List<Vector3> _verts = new List<Vector3>();
  private static List<int> _tris = new List<int>();

  public static void CreateCubeMesh(Mesh mesh,
                              Bounds bounds,
                              Face bottom = Face.None,
                              Face left = Face.None,
                              Face right = Face.None,
                              Face back = Face.None,
                              Face top = Face.None,
                              Face front = Face.None) {
    _verts.Clear();
    _tris.Clear();

    for (int sx = -1; sx <= 1; sx += 2) {
      for (int sy = -1; sy <= 1; sy += 2) {
        for (int sz = -1; sz <= 1; sz += 2) {
          Vector3 delta = bounds.size * 0.5f;
          delta.x *= sx;
          delta.y *= sy;
          delta.z *= sz;

          Vector3 corner = bounds.center + delta;
          _verts.Add(corner);
        }
      }
    }

    {
      addQuad(0, 4, 5, 1, bottom); //bottom
      addQuad(0, 1, 3, 2, left); //left
      addQuad(4, 6, 7, 5, right); //right
      addQuad(1, 5, 7, 3, back); //back
      addQuad(2, 3, 7, 6, top); //top
      addQuad(0, 2, 6, 4, front); //front
    }

    mesh.SetVertices(_verts);
    mesh.SetTriangles(_tris, 0);
    mesh.RecalculateBounds();
  }

  public static void CreateQuadMesh(Mesh mesh, Rect rect, Face face = Face.All) {
    _verts.Clear();
    _tris.Clear();

    _verts.Add(rect.position);
    _verts.Add(rect.position + Vector2.right * rect.width);
    _verts.Add(rect.position + rect.size);
    _verts.Add(rect.position + Vector2.up * rect.height);

    addQuad(0, 1, 2, 3, face);

    mesh.SetVertices(_verts);
    mesh.SetTriangles(_tris, 0);
    mesh.RecalculateBounds();
  }

  private static void addQuad(int a, int b, int c, int d, Face face) {
    if ((face & Face.Front) != 0) {
      _tris.Add(a);
      _tris.Add(b);
      _tris.Add(c);

      _tris.Add(a);
      _tris.Add(c);
      _tris.Add(d);
    }

    if ((face & Face.Back) != 0) {
      _tris.Add(a);
      _tris.Add(c);
      _tris.Add(b);

      _tris.Add(a);
      _tris.Add(d);
      _tris.Add(c);
    }
  }
}

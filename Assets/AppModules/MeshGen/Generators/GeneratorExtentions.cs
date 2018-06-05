using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.MeshGen {
  
  static class GeneratorExtensions {

    public static void AddTri(this List<int> indices, int idx1, int idx2, int idx3, bool flipFacing = false) {
      if (flipFacing) {
        int temp = idx2;
        idx2 = idx3;
        idx3 = temp;
      }
      indices.Add(idx1); indices.Add(idx2); indices.Add(idx3);
    }
    public static void AddTri(this List<int> indices, int vertexOffset, int idx1, int idx2, int idx3, bool flipFacing = false) {
      indices.AddTri(vertexOffset + idx1, vertexOffset + idx2, vertexOffset + idx3, flipFacing);
    }

    public static void AddVert(this List<Vector3> verts, float x, float y, float z) {
      verts.Add(new Vector3(x, y, z));
    }

    public static void AddQuad(this List<int> indices,
                               int idx0, int idx1, int idx2, int idx3,
                               bool flipFacing = false) {
      indices.AddTri(idx0, idx1, idx2, flipFacing);
      indices.AddTri(idx0, idx2, idx3, flipFacing);
    }

    public static void AddQuad(this List<int> indices,
                               int vertexOffset,
                               int idx0, int idx1, int idx2, int idx3,
                               bool flipFacing = false) {
      indices.AddTri(idx0 + vertexOffset, idx1 + vertexOffset, idx2 + vertexOffset, flipFacing);
      indices.AddTri(idx0 + vertexOffset, idx2 + vertexOffset, idx3 + vertexOffset, flipFacing);
    }

  }

}
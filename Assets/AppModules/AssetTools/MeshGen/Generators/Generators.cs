using UnityEngine;

using Vecs = System.Collections.Generic.List<UnityEngine.Vector3>;
using Vec2s = System.Collections.Generic.List<UnityEngine.Vector2>;
using Ints = System.Collections.Generic.List<int>;
using Cols = System.Collections.Generic.List<UnityEngine.Color>;

namespace Leap.Unity.MeshGen {

  public static partial class Generators {

    #region Generator Resources

    private static void borrowGeneratorResources(out Vecs verts,
                                                 out Ints indices,
                                                 out Vecs normals) {
      verts   = Pool<Vecs>.Spawn();
      indices = Pool<Ints>.Spawn();
      normals = Pool<Vecs>.Spawn();
    }

    private static void borrowGeneratorResources(out Vecs verts,
                                                 out Ints indices,
                                                 out Vecs normals,
                                                 out Vec2s uvs) {
      borrowGeneratorResources(out verts, out indices, out normals);

      uvs = Pool<Vec2s>.Spawn();
    }

    private static void returnGeneratorResources(Vecs verts,
                                                 Ints indices,
                                                 Vecs normals) {
      verts.Clear();
      Pool<Vecs>.Recycle(verts);

      indices.Clear();
      Pool<Ints>.Recycle(indices);

      normals.Clear();
      Pool<Vecs>.Recycle(normals);
    }

    private static void returnGeneratorResources(Vecs verts,
                                                 Ints indices,
                                                 Vecs normals,
                                                 Vec2s uvs) {
      returnGeneratorResources(verts, indices, normals);

      uvs.Clear();
      Pool<Vec2s>.Recycle(uvs);
    }

    #endregion

    #region Apply Resources to Mesh

    private static void apply(Mesh mesh, Vecs verts,
                                         Ints indices,
                                         Vecs normals = null,
                                         Cols colors = null,
                                         Vec2s uvs = null) {
      mesh.Clear();

      mesh.SetVertices(verts);
      mesh.SetTriangles(indices, 0, true);

      if (normals != null) {
        mesh.SetNormals(normals);
      }
      else {
        mesh.RecalculateNormals();
      }
      
      if (colors != null) {
        mesh.SetColors(colors);
      }

      if (uvs != null) {
        mesh.SetUVs(0, uvs);
        #if UNITY_EDITOR
        UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
        #endif
      }

    }

    #endregion

    #region Generation Functions

    public static void GenerateTorus(Mesh mesh,
                                     float majorRadius, int numMajorSegments,
                                     float minorRadius, int numMinorSegments,
                                     float minorStartAngle = 0f,
                                     float maxMinorArcAngle = 360f,
                                     bool shadeFlat = false) {
      Vecs verts; Ints indices; Vecs normals; Vec2s uvs;
      borrowGeneratorResources(out verts, out indices, out normals, out uvs);

      TorusSupport.AddIndices(indices, verts.Count,
                              numMajorSegments,
                              numMinorSegments,
                              maxMinorArcAngle);
      TorusSupport.AddVerts(verts, normals, uvs,
                            majorRadius, numMajorSegments,
                            minorRadius, numMinorSegments,
                            minorStartAngle: minorStartAngle,
                            maxMinorArcAngle: maxMinorArcAngle);



      apply(mesh, verts, indices, normals, uvs: uvs);

      returnGeneratorResources(verts, indices, normals, uvs);

      if (shadeFlat) {
        Meshing.PolyMesh.RoundTrip(mesh);
      }
    }

    public static void GenerateRoundedRectPrism(Mesh mesh,
                                                Vector3 extents,
                                                float cornerRadius, int cornerDivisions,
                                                bool withBack = true) {
      Vecs verts; Ints indices; Vecs normals;
      borrowGeneratorResources(out verts, out indices, out normals);

      RoundedRectSupport.AddFrontIndices(indices, verts.Count, cornerDivisions);
      RoundedRectSupport.AddFrontVerts(verts, normals, extents, cornerRadius, cornerDivisions);
      //RoundedRectPrism.AddFrontUVs(); // NYI

      RoundedRectSupport.AddSideIndices(indices, verts.Count, cornerDivisions);
      RoundedRectSupport.AddSideVerts(verts, normals, extents, cornerRadius, cornerDivisions);
      //RoundedRectPrism.AddSideUVs(); // NYI

      if (withBack) {
        Vector3 extentsForBack = new Vector3(extents.x, extents.y, 0F);
        RoundedRectSupport.AddFrontIndices(indices, verts.Count, cornerDivisions, flipFacing: true);
        RoundedRectSupport.AddFrontVerts(verts, normals, extentsForBack, cornerRadius, cornerDivisions, flipNormal: true);
        //RoundedRectPrism.AddBackUVs(); // NYI
      }

      apply(mesh, verts, indices, normals);
      returnGeneratorResources(verts, indices, normals);
    }

    public static void GenerateCubeFrame(Mesh mesh, Vector2 frameSize, float thickness) {
      Vecs verts; Ints indices; Vecs normals;
      borrowGeneratorResources(out verts, out indices, out normals);

      CubeFrameSupport.AddIndices(indices, verts.Count);
      CubeFrameSupport.AddVerts(verts, normals, frameSize.x, frameSize.y, thickness);

      apply(mesh, verts, indices, normals);
      returnGeneratorResources(verts, indices, normals);
    }

    public static void GenerateCircle(Mesh mesh, float radius, int numDivisions = 32) {
      Vecs verts; Ints indices; Vecs normals; Vec2s uvs;
      borrowGeneratorResources(out verts, out indices, out normals, out uvs);

      CircleSupport.AddIndices(indices, verts.Count, numDivisions);
      CircleSupport.AddVerts(verts, normals, uvs, radius, numDivisions);

      apply(mesh, verts, indices, normals, null, uvs);
      returnGeneratorResources(verts, indices, normals, uvs);
    }

    #endregion

  }

}
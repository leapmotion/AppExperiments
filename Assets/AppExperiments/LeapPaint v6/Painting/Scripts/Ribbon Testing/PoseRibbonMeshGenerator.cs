using Leap.Unity.Meshing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class PoseRibbonMeshGenerator : MonoBehaviour {

    public Transform poseSource0;
    public Transform poseSource1;
    public Transform poseSource2;
    
    public MeshFilter outputToFilter;

    public bool usePolyMeshMethod = false;
    private PolyMesh polyMesh = new PolyMesh();

    public float thickness = 0.25f;

    private void Update() {
      refreshMesh();
    }

    private void refreshMesh() {
      if (outputToFilter == null) return;
      if (outputToFilter.sharedMesh == null) {
        outputToFilter.sharedMesh = new Mesh();
        outputToFilter.sharedMesh.name = "PoseRibbonMesh Test";
      }
      var mesh = outputToFilter.sharedMesh;
      mesh.Clear();

      var pose0 = poseSource0.ToLocalPose();
      var pose1 = poseSource1.ToLocalPose();
      var pose2 = poseSource2.ToLocalPose();

      var poses = Pool<List<Pose>>.Spawn();
      poses.Clear();
      try {
        poses.Add(pose0); poses.Add(pose1); poses.Add(pose2);

        if (!usePolyMeshMethod) {
          #region Non-PolyMesh Method
          var verts = Pool<List<Vector3>>.Spawn(); verts.Clear();
          var indices = Pool<List<int>>.Spawn(); indices.Clear();
          try {

            for (int i = 0; i < poses.Count; i++) {
              var p = poses[i];

              verts.Add(left(p));
              verts.Add(right(p));
            }

            int vertsPerPose = 2;
            bool closeLoop = vertsPerPose != 2;

            for (int p = 0; p + 1 < poses.Count; p++) {

              for (int csi = 0; csi < vertsPerPose - (closeLoop ? 0 : 1); csi++) {
                var csRootIndex = p * vertsPerPose;
                var nextCSRootIndex = (p + 1) * vertsPerPose;

                var i0 = csRootIndex + csi;
                var i1 = nextCSRootIndex + csi;
                var i2 = nextCSRootIndex + ((csi + 1) % vertsPerPose);
                var i3 = csRootIndex + ((csi + 1) % vertsPerPose);

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i2);

                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i3);
              }

            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0, true);

            mesh.RecalculateNormals();
          }
          finally {
            verts.Clear(); Pool<List<Vector3>>.Recycle(verts);
            indices.Clear(); Pool<List<int>>.Recycle(indices);
          }
          #endregion
        }
        else {
          polyMesh.Clear();

          // quad 0
          polyMesh.AddPosition(left(pose0));
          polyMesh.AddPosition(right(pose0));
          polyMesh.AddPosition(left(pose1));
          polyMesh.AddPosition(right(pose1));
          var newPoly = Polygon.SpawnQuad(1, 0, 2, 3);
          polyMesh.AddPolygon(newPoly);

          // quad 1
          polyMesh.AddPosition(left(pose2));
          polyMesh.AddPosition(right(pose2));
          var nextPoly = Polygon.SpawnQuad(3, 2, 4, 5);
          polyMesh.AddPolygon(nextPoly);

          // mark the edge between the two quads as smooth.
          polyMesh.MarkEdgeSmooth(new Edge(2, 3));

          polyMesh.FillUnityMesh(mesh, doubleSided: true);
        }

      }
      finally {
        poses.Clear();
        Pool<List<Pose>>.Recycle(poses);
      }


    }

    private Vector3 right(Pose p) {
      return p.position + p.rotation * Vector3.right * thickness / 2f;
    }
    private Vector3 left(Pose p) {
      return p.position + p.rotation * Vector3.left * thickness / 2f;
    }

  }

}

using Leap.Unity.Attributes;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  public class SplineMeshGenerator : MonoBehaviour {

    [OnEditorChange("spline")]
    [SerializeField]
    private zzOldSplineObject _spline;
    public zzOldSplineObject spline {
      get { return _spline; }
      set {
        if (_spline != null) {
          _spline.OnSplineModified -= onSplineModified;
        }

        _spline = value;

        if (_spline != null) {
          _spline.OnSplineModified += onSplineModified;
          onSplineModified();
        }
      }
    }

    [OnEditorChange("onSplineModified")]
    public float thickness = 0.01F;
    public MeshFilter targetMeshFilter;

    private Mesh _splineMesh;
    private bool _isSplineDirty = false;

    private void onSplineModified() {
      _isSplineDirty = true;
    }

    void Awake() {
      // Make sure we're subscribed to Spline changes
      _spline.OnSplineModified -= onSplineModified;
      _spline.OnSplineModified += onSplineModified;
    }

    void LateUpdate() {
      if (_isSplineDirty) {
        refreshSplineMesh();
        _isSplineDirty = false;
      }
    }

    private static List<Vector3> s_verts = new List<Vector3>();
    private static List<int> s_indices = new List<int>();
    private void refreshSplineMesh() {
      s_verts.Clear();
      s_indices.Clear();

      zzOldSplineUtility.MeshGen.GenerateQuadSplineMesh(ref s_verts, ref s_indices,
        spline, thickness, Camera.main.transform.position, (1 - (1 - _spline.smoothness)*(1 - _spline.smoothness)).Map(0F, 1F, 0.85F, 1F));

      if (_splineMesh == null) {
        _splineMesh = new Mesh();
        _splineMesh.name = "Spline Mesh";
      }
      _splineMesh.Clear();
      _splineMesh.SetVertices(s_verts);
      _splineMesh.SetTriangles(s_indices, 0, true);

      if (targetMeshFilter != null) targetMeshFilter.mesh = _splineMesh;
    }

  }

}
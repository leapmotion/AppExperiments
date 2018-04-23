using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMeshObject : MonoBehaviour {

    #region Inspector

    [SerializeField]
    private MeshFilter _meshFilter;
    public MeshFilter meshFilter {
      get {
        if (_meshFilter == null) {
          _meshFilter = this.gameObject.GetComponent<MeshFilter>();
          if (_meshFilter == null) {
            _meshFilter = this.gameObject.AddComponent<MeshFilter>();
          }
        }
        return _meshFilter;
      }
    }

    [SerializeField]
    private MeshRenderer _meshRenderer;
    public MeshRenderer meshRenderer {
      get {
        if (_meshRenderer == null) {
          _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
          if (_meshRenderer == null) {
            _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
          }
        }

        return _meshRenderer;
      }
    }

    [SerializeField]
    private bool _generateDoubleSidedTris = false;
    public bool generateDoubleSidedTris {
      get { return _generateDoubleSidedTris; }
      set { _generateDoubleSidedTris = value; }
    }

    #endregion

    #region PolyMesh Data

    private PolyMesh _polyMesh;
    /// <summary>
    /// Gets the PolyMesh underlying this PolyMeshObject. It can be manipulated
    /// as desired, but changes to it won't be reflected by this PolyMeshObject's Unity
    /// mesh until you call RefreshUnityMesh().
    /// </summary>
    public PolyMesh polyMesh {
      get { return _polyMesh; }
    }

    // Two Unity meshes that are swapped when a given mesh is updated, better when
    // modifying a mesh every frame.
    private Mesh _unityMeshA;
    private Mesh _unityMeshB;

    public int PolygonCount {
      get { return _polyMesh.polygons.Count; }
    }

    #endregion

    #region Unity Events

    protected virtual void Awake() {
      _meshFilter = GetComponent<MeshFilter>();

      _polyMesh = Pool<PolyMesh>.Spawn();
      _polyMesh.DisableEdgeAdjacencyData();
      _polyMesh.Clear();

      _unityMeshA = Pool<Mesh>.Spawn();
      _unityMeshA.Clear();
      _unityMeshA.MarkDynamic();
      _unityMeshB = Pool<Mesh>.Spawn();
      _unityMeshB.Clear();
      _unityMeshB.MarkDynamic();
    }

    protected virtual void OnDestroy() {
      // In clearing the PolyMesh, its data is pooled.
      _polyMesh.Clear();
      Pool<PolyMesh>.Recycle(_polyMesh);

      _unityMeshA.Clear();
      Pool<Mesh>.Recycle(_unityMeshA);

      _unityMeshB.Clear();
      Pool<Mesh>.Recycle(_unityMeshB);
    }

    private bool _refreshMeshOnNextFrame = false;
    private bool _meshBufferSwitch = false;
    protected virtual void LateUpdate() {
      if (_refreshMeshOnNextFrame) {
        using (new ProfilerSample("LivePolyMeshObject: Refresh Unity Mesh")) {
          var bufferMesh = _meshBufferSwitch ? _unityMeshA : _unityMeshB;
          _meshBufferSwitch = !_meshBufferSwitch;

          _polyMesh.FillUnityMesh(bufferMesh, generateDoubleSidedTris);
          meshFilter.sharedMesh = bufferMesh;
        }

        _refreshMeshOnNextFrame = false;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Marks the Unity mesh to be re-uploaded on the next LateUpdate(). You need to call
    /// this in order to have your Unity mesh reflect any changes that might have been
    /// made to it since the last upload.
    /// </summary>
    public void RefreshUnityMesh() {
      _refreshMeshOnNextFrame = true;
    }

    #endregion

  }

}

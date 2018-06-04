using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGridRepeater : MonoBehaviour, IRuntimeGizmoComponent {

  [QuickButton("Generate", "RefreshMeshData", "Generate a mesh containing repetitions of the input mesh.")]
  public Mesh inputMesh;

  public float meshScaleMultiplier = 1f;

  public Vector2 gridSize = new Vector2(1f, 1f);

  [MinValue(1)]
  public int numRows = 6;

  [MinValue(1)]
  public int numCols = 6;

  private Mesh _resultMesh = null;
  public Mesh resultMesh {
    get {
      if (_resultMesh == null) {
        _resultMesh = new Mesh();
      }
      return _resultMesh;
    }
  }

  [SerializeField]
  [Tooltip("Grid ID values will be placed into UV1.x for the mesh.")]
  private bool _includeGridIDInUV1 = true;

  [SerializeField]
  [Tooltip("BlendShape delta vertex values will be placed into UV2.x for the mesh.")]
  private bool _includeBlendShapeDeltasInUV2 = true;

  [SerializeField, EditTimeOnly]
  [Tooltip("Perform the mesh refresh operation on Start at runtime. (Mesh is always refreshed OnValidate.)")]
  private bool _refreshOnStart = true;

  [Header("Output")]
  public MeshFilter outputToFilter;

  public LeapMeshGraphic outputToGraphic;

  [Header("PortalSurface Shader Integration")]
  public Renderer portalSurfaceRenderer;

  [SerializeField, OnEditorChange("portalGridParamsName")]
  private string _portalGridParamsName = "_GridSizeAndRowColCount";
  public string portalGridParamsName {
    get { return _portalGridParamsName; }
    set {
      _portalGridParamsName = value;
      portalGridParamsId = Shader.PropertyToID(value);
    }
  }
  [SerializeField, Disable]
  private int portalGridParamsId = 0;

  [SerializeField, OnEditorChange("portalGridOffsetParamName")]
  private string _portalGridOffsetParamName = "_Offset";
  public string portalGridOffsetParamName {
    get { return _portalGridOffsetParamName; }
    set {
      _portalGridOffsetParamName = value;
      portalGridOffsetParamId = Shader.PropertyToID(value);
    }
  }
  [SerializeField, Disable]
  private int portalGridOffsetParamId = 0;

  [SerializeField]
  //[DisableIf("portalSurfaceRenderer", isEqualTo: null)]
  [QuickButton("Upload Offset Now", "RefreshOffsetVector")]
  private Vector2 _startingOffset = new Vector2(0f, 0f);

  [Header("Debug")]
  public bool drawDebug = false;

  private void OnValidate() {
    if (_resultMesh != null && outputToFilter != null && outputToFilter.sharedMesh == _resultMesh) {
      RefreshPortalMaterialData();
      RefreshOffsetVector();
    }

    portalGridParamsId = Shader.PropertyToID(_portalGridParamsName);
    portalGridOffsetParamId = Shader.PropertyToID(_portalGridOffsetParamName);
  }

  private void Start() {
    if (_refreshOnStart) {
      RefreshMeshData();
    }
  }

  public void RefreshMeshData() {
    if (inputMesh == null) return;

    resultMesh.Clear();
    resultMesh.name = inputMesh.name + " (Repeated Grid: " + numCols + " x " + numRows + ")";

    if (inputMesh == null && Application.isPlaying) {
      Debug.LogError("No mesh given to repeat.", this);
      return;
    }

    var gridIds = Pool<List<int>>.Spawn();
    gridIds.Clear();
    var gridIdUVs = Pool<List<Vector2>>.Spawn();
    gridIdUVs.Clear();
    var blendShapeDeltaVerts = Pool<List<Vector3>>.Spawn();
    blendShapeDeltaVerts.Clear();
    try {
      int inputMeshVertexCount = inputMesh.vertexCount;

      foreach (var gridPoint in new GridPointEnumerator(gridSize, numRows, numCols)) {
        try {
          appendMesh(inputMesh, resultMesh, gridPoint.centerPos, meshScaleMultiplier);


          // Get and load blend shape data.
          if (inputMesh.blendShapeCount > 0) {
            Vector3[] deltaVerts = new Vector3[inputMesh.vertexCount];
            Vector3[] deltaNormals = new Vector3[inputMesh.vertexCount];
            Vector3[] deltaTangents = new Vector3[inputMesh.vertexCount];
            inputMesh.GetBlendShapeFrameVertices(0, 0, deltaVerts, deltaNormals, deltaTangents);
            foreach (var vertIdx in Values.Range(0, inputMeshVertexCount)) {
              blendShapeDeltaVerts.Add(deltaVerts[vertIdx] * meshScaleMultiplier);
            }
          }

          foreach (var vert in Values.Range(0, inputMeshVertexCount)) {
            gridIds.Add(gridPoint.gridId);
          }
        }
        catch (System.Exception e) {
          Debug.LogError("Error during appendMesh operation: " + e, this);
          return;
        }
      }

      if (_includeGridIDInUV1) {
        // After mesh repetition via append utility function, add grid IDs into UV1.
        for (int i = 0; i < resultMesh.vertexCount; i++) {
          gridIdUVs.Add(new Vector2(gridIds[i], 0f));
        }
        resultMesh.SetUVs(1, gridIdUVs);
      }

      if (_includeBlendShapeDeltasInUV2) {
        resultMesh.SetUVs(2, blendShapeDeltaVerts);
      }
    }
    finally {
      gridIds.Clear();
      Pool<List<int>>.Recycle(gridIds);

      gridIdUVs.Clear();
      Pool<List<Vector2>>.Recycle(gridIdUVs);

      blendShapeDeltaVerts.Clear();
      Pool<List<Vector3>>.Recycle(blendShapeDeltaVerts);
    }

    if (outputToGraphic != null) {
      outputToGraphic.SetMesh(_resultMesh);
    } 
    else if (outputToFilter != null) {
      outputToFilter.mesh = _resultMesh;
    } 
    else {
      Debug.LogWarning("[MeshGridRepeater] Generation successful, but no output mesh filter was specified.", this);
    }

    RefreshPortalMaterialData();
    RefreshOffsetVector();
  }

  public void RefreshPortalMaterialData() {
    if (portalSurfaceRenderer != null) {
      portalSurfaceRenderer.sharedMaterial.SetVector(portalGridParamsId,
        new Vector4(gridSize.x,
                    gridSize.y,
                    numRows,
                    numCols));
    }
  }

  public void RefreshOffsetVector() {
    if (portalSurfaceRenderer != null) {
      portalSurfaceRenderer.sharedMaterial.SetVector(portalGridOffsetParamId,
        new Vector4(_startingOffset.x,
                    _startingOffset.y,
                    0f, 0f));
    }
  }

  // TODO: This is a handy utility function, it should probably be moved into a utility class.
  // Although, it really should support color and normals and what-not.
  private void appendMesh(Mesh toAppend, Mesh appendedMesh,
                          Vector3 appendPosOffset = default(Vector3),
                          float meshScaleMultiplier = 1f) {

    if (toAppend.subMeshCount > 1) {
      throw new System.InvalidOperationException(
        "appendMesh only works on meshes with a single submesh.");
    }

    int appendIdxStart = appendedMesh.vertexCount;

    var _newVerts = Pool<List<Vector3>>.Spawn();
    _newVerts.Clear();
    var _newIndices = Pool<List<int>>.Spawn();
    _newIndices.Clear();
    try {
      // Verts
      foreach (var vert in appendedMesh.vertices) {
        _newVerts.Add(vert);
      }
      foreach (var vert in toAppend.vertices) {
        _newVerts.Add((vert * meshScaleMultiplier) + appendPosOffset);
      }

      // Indices
      // TODO: Optimization: Avoid allocation by getting native indices ptr
      foreach (var index in appendedMesh.GetIndices(0)) {
        _newIndices.Add(index);
      }
      foreach (var appendIndex in toAppend.GetIndices(0)) {
        _newIndices.Add(appendIndex + appendIdxStart);
      }

      // Upload!
      appendedMesh.SetVertices(_newVerts);
      appendedMesh.SetTriangles(_newIndices, 0);
      appendedMesh.RecalculateBounds();
    }
    finally {
      _newVerts.Clear();
      Pool<List<Vector3>>.Recycle(_newVerts);

      _newIndices.Clear();
      Pool<List<int>>.Recycle(_newIndices);
    }

  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (!drawDebug || !this.enabled || !this.gameObject.activeInHierarchy) return;

    drawer.PushMatrix();

    drawer.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale);

    foreach (var gridPoint in new GridPointEnumerator(gridSize, numRows, numCols)) {
      drawer.DrawPosition(gridPoint.centerPos);
    }

    drawer.matrix = Matrix4x4.TRS(
      this.transform.position,
      this.transform.rotation,
      this.transform.lossyScale.CompMul(new Vector3(1f, 1f, 0.005f)));

    drawer.color = Color.red;
    drawer.DrawWireCube(Vector3.zero, gridSize);
    drawer.color = Color.green;
    drawer.DrawWireCube(Vector3.zero, gridSize - Vector2.one * 0.001f);
    drawer.color = Color.blue;
    drawer.DrawWireCube(Vector3.zero, gridSize - Vector2.one * 0.002f);

    drawer.PopMatrix();
  }

}

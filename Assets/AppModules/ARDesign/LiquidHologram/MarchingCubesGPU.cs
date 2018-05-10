
// Marching Cubes on the GPU adapted from
// https://github.com/Scrawk/Marching-Cubes-On-The-GPU
// MIT licensed, see LICENSE file in the enclosing folder.

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Leap.Unity.ARTesting {

  public class MarchingCubesGPU : MonoBehaviour {

    /// <summary>
    /// The size of the voxel array in each dimension.
    /// </summary>
    const int MARCHING_CUBES_DIM = 64;

    /// <summary>
    /// The total number of voxels in the marching cubes array.
    /// </summary>
    const int MARCHING_CUBES_TOTAL = MARCHING_CUBES_DIM
                                     * MARCHING_CUBES_DIM
                                     * MARCHING_CUBES_DIM;

    /// <summary>
    /// The size of the buffer that holds the verts. This is the maximum number of verts
    /// that the marching cube can produce, 5 triangles for each voxel.
    /// </summary>
    const int VERT_BUFFER_SIZE = MARCHING_CUBES_TOTAL * 3 * 5;

    /// <summary>
    /// What is this?
    /// </summary>
    public Material m_drawBuffer;
    
    public ComputeShader marchingCubesCompute;
    public ComputeShader normalsCompute;

    [Header("Rendering")]

    public Camera renderCamera;

    private ComputeBuffer _fluidDensityFixedPointBuffer;
    private ComputeBuffer _fluidDensityBuffer;

    private ComputeBuffer _meshBuffer;
    private RenderTexture _normalsBuffer;
    private ComputeBuffer _cubeEdgeFlags;
    private ComputeBuffer _triangleConnectionTable;

    void Start() {
      Camera.onPostRender -= onPostRender;
      Camera.onPostRender += onPostRender;

      #pragma warning disable 0162
      // There are 8 threads run per group, so N must be divisible by 8.
      if (MARCHING_CUBES_DIM % 8 != 0) {
        throw new System.ArgumentException("N must be divisible be 8");
      }
      #pragma warning restore 0162

      // Set up the fluid density buffers. We have one that is fixed-point using uints
      // so that we can atomically sum particle density into the volume.
      _fluidDensityFixedPointBuffer = new ComputeBuffer(MARCHING_CUBES_DIM, sizeof(uint));
      _fluidDensityBuffer = new ComputeBuffer(MARCHING_CUBES_TOTAL, sizeof(float));

      // Set up the normals buffer for the mesh.
      _normalsBuffer = new RenderTexture(MARCHING_CUBES_DIM, MARCHING_CUBES_DIM, 0,
                                         RenderTextureFormat.ARGBHalf,
                                         RenderTextureReadWrite.Linear);
      _normalsBuffer.dimension = TextureDimension.Tex3D;
      _normalsBuffer.enableRandomWrite = true;
      _normalsBuffer.useMipMap = false;
      _normalsBuffer.volumeDepth = MARCHING_CUBES_DIM;
      _normalsBuffer.Create();

      // Initialize the buffer that contains vertex data from marching cubes.
      _meshBuffer = new ComputeBuffer(VERT_BUFFER_SIZE, sizeof(float) * 7);

      // Clear the mesh verts to -1. See the TriangleConnectionTable. Only verts that get
      // generated will then have a value of 1.
      // (Only required if reading back the mesh.)
      // Could also use the ClearMesh compute shader provided.
      float[] zeroArray = new float[VERT_BUFFER_SIZE * 7];
      for (int i = 0; i < VERT_BUFFER_SIZE * 7; i++) {
        zeroArray[i] = -1.0f;
      }
      _meshBuffer.SetData(zeroArray);

      // These two buffers are just some settings needed by the marching cubes.
      _cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
      _cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
      _triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
      _triangleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

      // Compute voxel fixed-point density.
      //_fluidDensityFixedPointBuffer

      // Convert fixed-point density to floating-point density for marching cubes.
      //_fluidDensityBuffer

      // Compute voxel normals.
      normalsCompute.SetInt("_Width", MARCHING_CUBES_DIM);
      normalsCompute.SetInt("_Height", MARCHING_CUBES_DIM);
      normalsCompute.SetBuffer(0, "_Noise", _fluidDensityBuffer);
      normalsCompute.SetTexture(0, "_Result", _normalsBuffer);

      normalsCompute.Dispatch(0, MARCHING_CUBES_DIM / 8, MARCHING_CUBES_DIM / 8, MARCHING_CUBES_DIM / 8);

      // Run marching cubes to generate a surface for the volume.
      marchingCubesCompute.SetInt("_Width", MARCHING_CUBES_DIM);
      marchingCubesCompute.SetInt("_Height", MARCHING_CUBES_DIM);
      marchingCubesCompute.SetInt("_Depth", MARCHING_CUBES_DIM);
      marchingCubesCompute.SetInt("_Border", 1);
      marchingCubesCompute.SetFloat("_Target", 0.0f);
      marchingCubesCompute.SetBuffer(0, "_Voxels", _fluidDensityBuffer);
      marchingCubesCompute.SetTexture(0, "_Normals", _normalsBuffer);
      marchingCubesCompute.SetBuffer(0, "_Buffer", _meshBuffer);
      marchingCubesCompute.SetBuffer(0, "_CubeEdgeFlags", _cubeEdgeFlags);
      marchingCubesCompute.SetBuffer(0, "_TriangleConnectionTable", _triangleConnectionTable);

      marchingCubesCompute.Dispatch(0, MARCHING_CUBES_DIM / 8,
                                       MARCHING_CUBES_DIM / 8,
                                       MARCHING_CUBES_DIM / 8);

      //Reads back the mesh data from the GPU and turns it into a standard unity mesh.
      //ReadBackMesh(m_meshBuffer);

    }

    /// <summary>
    /// Called for every camera in the scene; we render from a single camera specified
    /// in the script by ignoring the post-render events from any other camera.
    /// </summary>
    private void onPostRender(Camera forCamera) {
      if (forCamera != renderCamera) return;

      DrawMesh();
    }

    void Update() {

    }

    /// <summary>
    /// Draws the mesh when cameras OnPostRender called.
    /// </summary>
    void DrawMesh() {
      //Since mesh is in a buffer need to use DrawProcedual called from OnPostRender
      m_drawBuffer.SetBuffer("_Buffer", _meshBuffer);
      m_drawBuffer.SetPass(0);

      Graphics.DrawProcedural(MeshTopology.Triangles, VERT_BUFFER_SIZE);
    }

    void OnDestroy() {
      // MUST release buffers.
      _fluidDensityFixedPointBuffer.Release();
      _fluidDensityBuffer.Release();
      _meshBuffer.Release();
      _cubeEdgeFlags.Release();
      _triangleConnectionTable.Release();
      _normalsBuffer.Release();

      Camera.onPostRender -= onPostRender;
    }

    struct Vert {
      public Vector4 position;
      public Vector3 normal;
    };

    /// <summary>
    /// Reads back the mesh data from the GPU and turns it into a standard unity mesh.
    /// </summary>
    /// <returns></returns>
    List<GameObject> ReadBackMesh(ComputeBuffer meshBuffer) {
      //Get the data out of the buffer.
      Vert[] verts = new Vert[VERT_BUFFER_SIZE];
      meshBuffer.GetData(verts);

      //Extract the positions, normals and indexes.
      List<Vector3> positions = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> index = new List<int>();

      List<GameObject> objects = new List<GameObject>();

      int idx = 0;
      for (int i = 0; i < VERT_BUFFER_SIZE; i++) {
        //If the marching cubes generated a vert for this index
        //then the position w value will be 1, not -1.
        if (verts[i].position.w != -1) {
          positions.Add(verts[i].position);
          normals.Add(verts[i].normal);
          index.Add(idx++);
        }

        int maxTriangles = 65000 / 3;

        if (idx >= maxTriangles) {
          objects.Add(MakeGameObject(positions, normals, index));
          idx = 0;
          positions.Clear();
          normals.Clear();
          index.Clear();
        }
      }

      return objects;
    }

    GameObject MakeGameObject(List<Vector3> positions, List<Vector3> normals, List<int> index) {
      Mesh mesh = new Mesh();
      mesh.vertices = positions.ToArray();
      mesh.normals = normals.ToArray();
      mesh.bounds = new Bounds(new Vector3(0, MARCHING_CUBES_DIM / 2, 0), new Vector3(MARCHING_CUBES_DIM, MARCHING_CUBES_DIM, MARCHING_CUBES_DIM));
      mesh.SetTriangles(index.ToArray(), 0);

      GameObject go = new GameObject("Voxel Mesh");
      go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      go.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
      go.GetComponent<MeshFilter>().mesh = mesh;
      go.isStatic = true;

      MeshCollider collider = go.AddComponent<MeshCollider>();
      collider.sharedMesh = mesh;

      go.transform.parent = transform;

      //Draw mesh next too the one draw procedurally.
      go.transform.localPosition = new Vector3(MARCHING_CUBES_DIM + 2, 0, 0);

      return go;
    }
  }
}

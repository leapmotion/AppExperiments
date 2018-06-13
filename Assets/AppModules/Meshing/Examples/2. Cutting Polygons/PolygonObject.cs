using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class PolygonObject : MonoBehaviour, IRuntimeGizmoComponent {

    public MeshFilter _meshFilter;

    public Color debugVertColor = Color.white;
    public float debugVertRadiusMult = 1.0f;

    [Range(3, 12)]
    public int numVerts = 5;

    public PolyMesh polyMesh;

    private void Reset() {
      if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Polygon";
    }

    public void UpdateUnityMesh() {
      polyMesh.FillUnityMesh(_meshFilter.mesh);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      var mesh = polyMesh;

      if (mesh == null) return;

      // Verts.
      foreach (var poly in mesh.polygons) {
        int vertIdx = 0;
        foreach (var vertPos in poly.verts.Query().Select(vIdx => poly.GetMeshPosition(vIdx))) {
          drawer.color = Color.Lerp(debugVertColor, Color.Lerp(debugVertColor, Color.black, 0.5f),
                                    ((float)vertIdx / poly.verts.Count));
          drawer.DrawWireSphere(vertPos, PolyMath.POSITION_TOLERANCE * debugVertRadiusMult);
          drawer.DrawWireCube(vertPos, Vector3.one * PolyMath.POSITION_TOLERANCE * debugVertRadiusMult);
          vertIdx++;
        }
      }

    }
  }

}
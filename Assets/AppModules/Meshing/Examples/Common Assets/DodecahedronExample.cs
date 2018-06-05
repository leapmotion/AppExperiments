using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronExample : MonoBehaviour, IRuntimeGizmoComponent {

    public MeshFilter _meshFilter;

    public Color debugVertColor = Color.white;
    public float debugVertRadiusMult = 1.0f;

    [HideInInspector]
    public DodecahedronCutManager cutManager;

    public PolyMesh polyMesh;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Dodecahedron";

      if (cutManager == null) {
        InitMesh();
        UpdateMesh();
      }
    }

    public void InitMesh() {
      if (polyMesh == null) {
        polyMesh = new PolyMesh(this.transform);
      }
      Dodecahedron.FillPolyMesh(polyMesh);
    }

    public void UpdateMesh() {
      polyMesh.FillUnityMesh(_meshFilter.mesh);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      var mesh = polyMesh;

      if (mesh == null) return;

      // Verts.
      foreach (var poly in mesh.polygons) {
        int vertIdx = 0;
        drawer.color = debugVertColor;
        foreach (var vertPos in poly.verts.Query().Select(vIdx => poly.GetMeshPosition(vIdx))) {
          drawer.DrawWireSphere(vertPos, PolyMath.POSITION_TOLERANCE * debugVertRadiusMult);
          drawer.DrawWireCube(vertPos, Vector3.one * PolyMath.POSITION_TOLERANCE * debugVertRadiusMult);
          vertIdx++;
        }
      }

    }

  }

}
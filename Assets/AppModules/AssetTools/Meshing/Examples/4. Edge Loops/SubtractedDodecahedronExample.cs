using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class SubtractedDodecahedronExample : MonoBehaviour {

    public MeshFilter _meshFilter;

    private PolyMesh _polyMesh;

    public DodecahedronExample subtractFrom;
    public DodecahedronExample subtractUsing;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Subtracted Dodecahedron";

      _polyMesh = new PolyMesh();
    }

    private void Update() {
      subtractFrom.InitMesh();
      subtractUsing.InitMesh();

      PolyMesh.Ops.Subtract(subtractFrom.polyMesh, subtractUsing.polyMesh, _polyMesh);

      subtractFrom.UpdateMesh();
      subtractUsing.UpdateMesh();

      _polyMesh.FillUnityMesh(_meshFilter.mesh);
    }

  }

}
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class CutManager : MonoBehaviour {

    public PolygonObject polygonToCut;
    public PolygonObject cuttingPolygon;

    private void Start() {
      polygonToCut.polyMesh = new PolyMesh();
      cuttingPolygon.polyMesh = new PolyMesh();

      polygonToCut.polyMesh.useTransform   = polygonToCut.transform;
      cuttingPolygon.polyMesh.useTransform = cuttingPolygon.transform;
    }
    
    private void Update() {
      Polygon.FillPolyMesh(polygonToCut.numVerts,   polygonToCut.polyMesh);
      Polygon.FillPolyMesh(cuttingPolygon.numVerts, cuttingPolygon.polyMesh);

      // Cut!
      PolyMesh.Ops.zzOldDualCut(polygonToCut.polyMesh, cuttingPolygon.polyMesh);

      // Update Unity mesh.
      polygonToCut.UpdateUnityMesh();
      cuttingPolygon.UpdateUnityMesh();
    }

  }

}
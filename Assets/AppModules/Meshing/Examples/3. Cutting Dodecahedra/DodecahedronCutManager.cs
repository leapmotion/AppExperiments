using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronCutManager : MonoBehaviour {

    public DodecahedronExample dA;
    public DodecahedronExample dB;

    void Awake() {
      dA.cutManager = this;
      dB.cutManager = this;
    }

    void Update() {
      dA.InitMesh();
      dB.InitMesh();

      var cutEdges = Pool<List<PolyMesh.Ops.DualEdge>>.Spawn();
      cutEdges.Clear();
      try {
        PolyMesh.Ops.DualCut(dA.polyMesh, dB.polyMesh, cutEdges);
      }
      finally {
        cutEdges.Clear();
        Pool<List<PolyMesh.Ops.DualEdge>>.Recycle(cutEdges);
      }

      dA.UpdateMesh();
      dB.UpdateMesh();
    }

  }

}

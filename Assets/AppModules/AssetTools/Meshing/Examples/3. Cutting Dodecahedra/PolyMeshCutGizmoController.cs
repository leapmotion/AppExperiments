using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMeshCutGizmoController : MonoBehaviour {

    public const string POLYMESH_CUT_GIZMO_CATEGORY = "PolyMesh Cut Runtime Gizmos";

    [DevGui.DevCategory(POLYMESH_CUT_GIZMO_CATEGORY)]
    [DevGui.DevValue]
    public bool colocatedVerts = false;

  }

}

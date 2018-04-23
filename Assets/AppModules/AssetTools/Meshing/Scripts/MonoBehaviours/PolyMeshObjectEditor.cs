using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMeshObjectEditor : MonoBehaviour                                                                                                                                                                                                            {

    #region Inspector

    public PolyMeshObject polyMeshObj;

    #endregion

    #region Unity Events

    private void Reset() {
      polyMeshObj = GetComponent<PolyMeshObject>();
    }

    #endregion

  }
  
}

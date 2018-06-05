using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class TransformPositionProvider : MonoBehaviour, IVector3Provider {

    public Vector3 Get() {
      return this.transform.position;
    }

  }

}

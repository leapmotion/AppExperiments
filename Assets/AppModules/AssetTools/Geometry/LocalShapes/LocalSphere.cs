using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  [System.Serializable]
  public struct LocalSphere {

    public Vector3 center;
    public float radius;

    public Sphere With(Transform t) {
      return new Sphere(this, t);
    }

  }

}

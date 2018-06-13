using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  using UnityRect = UnityEngine.Rect;

  [System.Serializable]
  public struct LocalRect {

    public Vector3 center;
    public Vector2 radii;

    public LocalRect(Vector3 center, Vector2 radii) {
      this.center = center;
      this.radii = radii;
    }

    public Rect With(Transform transform) {
      return new Rect(this, transform);
    }

  }

}

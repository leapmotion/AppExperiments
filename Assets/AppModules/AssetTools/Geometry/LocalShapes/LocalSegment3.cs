using Leap.Unity.Infix;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {
  
  public struct LocalSegment3 {

    public Vector3 a, b;

    public LocalSegment3(Vector3 a, Vector3 b) {
      this.a = a;
      this.b = b;
    }

    /// <summary>
    /// Given a point _on the segment_, parameterizes that point into a value such that
    /// a + (b - a).magnitude * value = b.
    /// </summary>
    public float Parameterize(Vector3 pointOnSegment) {
      if ((a - b).sqrMagnitude < float.Epsilon) return 0f;
      return (pointOnSegment - a).magnitude / (b - a).magnitude;
    }

    public Vector3 Evaluate(float t) {
      var ab = b - a;
      return a + ab * t;
    }

    #region Collision

    /// <summary>
    /// Returns the squared distance between this line segment and the rect.
    /// </summary>
    public float Intersect(Rect rect) {
      return Collision.Intersect(this, rect);
    }

    #endregion

    #region Runtime Gizmos

    public void DrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.DrawLine(a, b);
    }

    #endregion

  }

}

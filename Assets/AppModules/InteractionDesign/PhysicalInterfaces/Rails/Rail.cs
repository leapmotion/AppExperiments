using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Space;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class Rail : MonoBehaviour, IRuntimeGizmoComponent {

    public float length = 0.4f;

    //public LeapSpace space;

    public Vector3 beginPosition {
      get {
        return this.transform.position + this.transform.right * -length / 2f;
      }
    }

    public Vector3 endPosition {
      get {
        return this.transform.position + this.transform.right * length / 2f;
      }
    }

    public Vector3 ConstrainToRail(Vector3 worldPosition) {
      return constrainToSegment(worldPosition, beginPosition, endPosition);
    }

    /// <summary>
    /// Returns this Vector3 clamped to the implicit edge between positions pA and pB.
    /// </summary>
    private Vector3 constrainToSegment(Vector3 pos, Vector3 pA, Vector3 pB, float endSnapTolerance = 0f) {
      var a = pA;
      var b = pB;
      var ab = b - a;
      var mag = ab.magnitude;
      var lineDir = ab / mag;
      var progress = Vector3.Dot((pos - a), lineDir);
      if (progress < endSnapTolerance) progress = 0f;
      else if (progress > (mag - endSnapTolerance)) progress = mag;
      return a + lineDir * progress;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (Application.isPlaying) { return; }

      drawer.color = LeapColor.turquoise;

      drawer.DrawLine(beginPosition, endPosition);
    }

  }

}

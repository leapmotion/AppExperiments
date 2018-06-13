using Leap.Unity;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class RightHandFistStrengthBarGizmo : MonoBehaviour,
    IRuntimeGizmoComponent {

    public Color color;

    float _fistStrength = 0f;

    void Update() {
      var hand = Hands.Right;

      if (hand != null) {
        _fistStrength = hand.GetFistStrength();
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.DrawBar(_fistStrength, this.transform, color, 0.25f);
    }

  }

}
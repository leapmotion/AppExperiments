using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class StepperRailFrameEdge : MonoBehaviour, IRuntimeGizmoComponent {

    public CapsuleCollider capsuleCollider;

    public StepperRail stepperRail;
    
    private void Reset() {
      if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider>();
      if (stepperRail == null) stepperRail = GetComponentInParent<StepperRail>();
    }

    private Rigidbody _capsuleBody;

    private void Update() {

    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!gameObject.activeInHierarchy || !this.enabled) return;

      drawer.color = LeapColor.red.WithAlpha(1f);

      var a = Vector3.zero;
      var b = Vector3.zero;
      var effRadius = capsuleCollider.GetEffectiveRadius();

      capsuleCollider.GetCapsulePoints(out a, out b);

      drawer.DrawWireCapsule(a, b, effRadius);
      drawer.DrawWireCapsule(a, b, effRadius * 1.01f);
    }

  }

}

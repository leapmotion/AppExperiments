using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.RuntimeGizmos {

  public class RuntimePoseGizmo : MonoBehaviour, IRuntimeGizmoComponent {

    public float poseRadius = 0.1f;

    public Color sphereColor = LeapColor.white;

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy) return;

      drawer.color = sphereColor;
      drawer.DrawPose(this.transform.ToWorldPose(), poseRadius);
    }

  }

}

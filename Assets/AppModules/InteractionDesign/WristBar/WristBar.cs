using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class WristBar : MonoBehaviour, IRuntimeGizmoComponent {

    public Vector3 localTabDirection { get { return Vector3.right; } }
    public Vector3 tabDirection { get { return transform.TransformDirection(localTabDirection); } }
    public Vector3 tabPosition { get { return transform.position + tabDirection * tabStubLength; } }

    public float tabStubLength { get { return 0.06f; } }

    public Pose pose { get { return this.transform.ToWorldPose(); } }
    public Pose tabStubPose { get { return new Pose(tabPosition, this.transform.rotation); } }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy) return;

      drawer.color = LeapColor.white;

      drawer.DrawPose(this.pose, radius: 0.015f);

      drawer.DrawPose(tabStubPose, radius: 0.010f);
    }
  
  }

}

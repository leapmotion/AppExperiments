using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Query;
using Leap.Unity.Attributes;
using Leap.Unity.Splines;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Layout;
using Leap.Unity.Streams;

namespace Leap.Unity {

  public class TestStrokeGizmos : MonoBehaviour, IRuntimeGizmoComponent {

    public Color gizmoColor = LeapColor.jade.WithAlpha(0.4f);

    public bool drawPoseGizmos = false;

    [ImplementsInterface(typeof(IIndexable<Pose>))]
    [SerializeField]
    private MonoBehaviour _strokePoses = default;
    public IIndexable<Pose> strokePoses {
      get { return _strokePoses as IIndexable<Pose>; }
    }

    void Start() { }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !gameObject.activeInHierarchy) {
        return;
      }

      drawer.color = gizmoColor;

      foreach (var prevPair in strokePoses.Query().WithPrevious()) {
        var pose = prevPair.value;
        var prevPose = prevPair.prev;

        drawer.DrawLine(prevPose.position, pose.position);
      }

      if (drawPoseGizmos) {
        foreach (var pose in strokePoses.GetEnumerator()) {
          drawer.DrawPose(pose, 0.006f);
        }
      }
    }

  }

}
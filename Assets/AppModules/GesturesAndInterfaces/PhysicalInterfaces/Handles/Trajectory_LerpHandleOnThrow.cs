using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Leap.Unity.Layout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class Trajectory_LerpHandleOnThrow : MonoBehaviour {

    public IntObj handle;

    public TrajectoryLerpToPose trajectoryLerpToPose;

    [SerializeField, ImplementsInterface(typeof(IPoseProvider))]
    private MonoBehaviour _targetPoseProvider;
    public IPoseProvider targetPoseProvider {
      get {
        return _targetPoseProvider as IPoseProvider;
      }
    }

    void Start() {
      handle.OnGraspEnd += onGraspEnd;
    }

    private void onGraspEnd() {
      if (!gameObject.activeInHierarchy || !this.enabled) return; 

      trajectoryLerpToPose.MoveToTarget(targetPoseProvider.GetPose());
    }

  }

}

using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {
  
  using IntObj = InteractionBehaviour;

  public class RailGraspedMovement : MonoBehaviour {

    public IntObj intObj;

    public Rail rail;

    public float snapDistance = 0.1f;
    private float snapDistanceSqr { get { return snapDistance * snapDistance; } }

    private void OnEnable() {
      intObj.OnGraspedMovement -= onGraspedMovement;
      intObj.OnGraspedMovement += onGraspedMovement;
    }

    void OnDisable() {
      intObj.OnGraspedMovement -= onGraspedMovement;
    }

    void Reset() {
      if (intObj == null) intObj = GetComponent<IntObj>();
    }

    private void onGraspedMovement(Vector3 oldPos, Quaternion oldRot,
                                   Vector3 newPos, Quaternion newRot,
                                   List<InteractionController> controllers) {

      if (!gameObject.activeInHierarchy || !this.enabled || rail == null) return;

      var railPose = new Pose(newPos, newRot);

      railPose.position = rail.ConstrainToRail(railPose.position);
      railPose.rotation = rail.transform.rotation;

      var targetPose = new Pose(newPos, newRot);
      if ((targetPose.position - railPose.position).sqrMagnitude < snapDistanceSqr) {
        targetPose = railPose;
      }

      intObj.rigidbody.MovePosition(targetPose.position);
      intObj.rigidbody.MoveRotation(targetPose.rotation);
      intObj.rigidbody.position = targetPose.position;
      intObj.rigidbody.rotation = targetPose.rotation;
    }

  }

}

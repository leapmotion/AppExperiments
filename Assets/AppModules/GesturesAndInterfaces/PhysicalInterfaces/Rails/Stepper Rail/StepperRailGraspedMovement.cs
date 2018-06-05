using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class StepperRailGraspedMovement : MonoBehaviour {

    public StepperRail stepperRail;

    public IntObj intObj;

    public int thisHandleIdx = 0;

    //public Transform parentToMoveInstead;

    private void Reset() {
      if (intObj == null) intObj = GetComponent<IntObj>();
    }

    private void Start() {
      intObj.OnGraspedMovement += onGraspedMovement;
    }

    private float? _newT;

    private void onGraspedMovement(Vector3 oldPos, Quaternion oldRot,
                                   Vector3 newPos, Quaternion newRot,
                                   List<InteractionController> controllers) {

      var tOfPos = 0f;
      //var newPoseOnRail = stepperRail.FindNearestPosition(newPos, out tOfPos);

      intObj.rigidbody.position = oldPos;
      intObj.rigidbody.rotation = oldRot;

      _newT = tOfPos;

      //_newPhysicsPose = newPoseOnRail;

      //intObj.rigidbody.MovePosition(newPoseOnRail.position);
      //intObj.rigidbody.MoveRotation(newPoseOnRail.rotation);

    }

    private void Update() {
      if (_newT.HasValue) {
        //parentToMoveInstead.transform.SetPose(_newPhysicsPose.Value);
        stepperRail.MoveT(_newT.Value, thisHandleIdx);
        _newT = null;
      }
    }

  }

}

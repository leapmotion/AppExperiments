using Leap.Unity.Interaction;
using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Attributes;
using UnityEngine.EventSystems;

namespace Leap.Unity.PhysicalInterfaces {

  public class zzOld_InteractionObjectHandle : zzOld_HandleBase {

    #region Inspector

    [Header("Interaction Object Handle")]

    [SerializeField, OnEditorChange("intObj")]
    private InteractionBehaviour _intObj;
    public InteractionBehaviour intObj {
      get { return _intObj; }
      set {
        if (_intObj != value) {
          if (isHeld) Release();

          if (_intObj != null && Application.isPlaying) {
            unsubscribeIntObjCallbacks();
          }

          if (value != null && Application.isPlaying) {
            subscribeIntObjCallbacks();
          }

          _intObj = value;
        }
      }
    }

    #endregion

    #region Unity Events

    protected virtual void Reset() {
      initInspector();
    }

    protected virtual void OnValidate() {
      initInspector();
    }

    protected override void Awake() {
      base.Awake();

      initInspector();
    }

    private void initInspector() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();

      subscribeIntObjCallbacks();
    }

    #endregion

    #region Interaction Object Handle

    public override Pose pose {
      get { return intObj.transform.ToPose(); }
      protected set {
        if (gameObject.activeInHierarchy) {
          //intObj.rigidbody.MovePosition(value.position);
          //intObj.rigidbody.MoveRotation(value.rotation);
          intObj.rigidbody.position = value.position;
          intObj.rigidbody.rotation = value.rotation;
          intObj.transform.SetPose(value);
        }
        else {
          intObj.transform.SetPose(value);
        }
      }
    }

    private Vector3 _localGraspedPoint = Vector3.zero;
    public override Vector3 localPivot {
      get {
        if (isHeld) {
          return _localGraspedPoint;
        }
        return Vector3.zero;
      }
    }

    private void unsubscribeIntObjCallbacks() {
      intObj.OnGraspBegin -= onGraspBegin;
      intObj.OnGraspEnd -= onGraspEnd;

      intObj.OnGraspedMovement -= onGraspedMovement;
    }

    private void subscribeIntObjCallbacks() {
      intObj.OnGraspBegin += onGraspBegin;
      intObj.OnGraspEnd += onGraspEnd;

      intObj.OnGraspedMovement += onGraspedMovement;
    }

    private void onGraspBegin() {
      if (!isHeld) {
        Hold();

        _localGraspedPoint = (pose.inverse
                              * intObj.GetGraspPoint(intObj.graspingController)).position;
      }
    }

    private void onGraspEnd() {
      if (isHeld) {
        Release();
      }
    }

    public override void Release() {
      base.Release();

      if (intObj.isGrasped) {
        intObj.ReleaseFromGrasp();
      }
    }

    protected override void LateUpdate() {
      base.LateUpdate();

      _numGraspedMovements = 0;
    }

    private int _numGraspedMovements = 0;

    //private Maybe<Pose> _maybeLastGraspedPose = Maybe.None;

    private void onGraspedMovement(Vector3 oldPosition, Quaternion oldRotation,
                                   Vector3 newPosition, Quaternion newRotation,
                                   List<InteractionController> graspingControllers) {

      var newTargetPose = new Pose() {
        position = newPosition,
        rotation = newRotation
      };

      //targetPose = newPose;

      //var lastGraspedPose = new Pose() {
      //  position = oldPosition,
      //  rotation = oldRotation
      //};

      //var lastGraspedPose = _maybeLastGraspedPose.hasValue ?
      //                        _maybeLastGraspedPose.valueOrDefault
      //                      : newPose;

      //var lastGraspedPose = targetPose.Then(movement.inverse * Time.fixedDeltaTime);

      targetPose = newTargetPose;
      //if (_numGraspedMovements == 0) {
      //  targetPose = newTargetPose;
      //}
      _numGraspedMovements += 1;


      //_maybeLastGraspedPose = targetPose;

      //_localGraspedPoint = (targetPose.inverse
      //                      * intObj.GetGraspPoint(intObj.graspingController)).position;


    }

    #endregion

  }

}

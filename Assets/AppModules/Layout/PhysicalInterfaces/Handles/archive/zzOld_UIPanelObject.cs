//using Leap.Unity.Attributes;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Leap.Unity.PhysicalInterfaces {

//  public class zzOld_UIPanelObject : MonoBehaviour {

//    public zzOld_HandledObject handledObj;

//    public bool flip180 = false;

//    void OnEnable() {
//      handledObj.OnUpdateTarget -= _onHandleUpdateTargetAction;
//      handledObj.OnUpdateTarget += _onHandleUpdateTargetAction;
//    }

//    void OnDisable() {
//      handledObj.OnUpdateTarget -= _onHandleUpdateTargetAction;
//    }

//    private Action _backingOnHandleUpdateTargetAction = null;
//    private Action _onHandleUpdateTargetAction {
//      get {
//        if (_backingOnHandleUpdateTargetAction == null) {
//          _backingOnHandleUpdateTargetAction = onHandleUpdateTarget;
//        }
//        return _backingOnHandleUpdateTargetAction;
//      }
//    }
//    private void onHandleUpdateTarget() {
//      if (handledObj.isHeld) {
//        var target = handledObj.targetPose;
//        var handle = handledObj.heldHandle.targetPose;
//        var localPivot = handledObj.heldHandle.localPivot;

//        DebugPing.Ping(target.Then(handle.From(target) * localPivot), LeapColor.amber, 0.2f);

//        handledObj.targetPose = PivotLook.Solve(target,
//                                                (handle.From(target) * localPivot).position,
//                                                Camera.main.transform.position,
//                                                Camera.main.transform.parent.up,
//                                                flip180: flip180);
//      }
//    }

//  }

//}

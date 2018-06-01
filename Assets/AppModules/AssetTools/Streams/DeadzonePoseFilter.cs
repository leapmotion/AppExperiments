using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SCRIPT NOT YET COMPLETE.

//namespace Leap.Unity.ARTesting {

//  public class DeadzonePoseFilter : MonoBehaviour,
//                                    IStreamReceiver<Pose>,
//                                    IStream<Pose> {

//    private Pose? _lastSetPose = null;

//    public event Action OnOpen = () => { };
//    public event Action<Pose> OnSend = (pose) => { };
//    public event Action OnClose = () => { };

//    public void Close() {

//    }

//    public void Open() {
//      _lastSetPose = null;
//    }

//    public void Receive(Pose pose) {
//      if (!_lastSetPose.HasValue) {
//        OnSend(pose);

//        _lastSetPose = pose;
//      }

//      // Compare delta position.
//      if ()
//    }

//  }

//}
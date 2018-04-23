using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Streams {

  public class TransformPoseStream : MonoBehaviour, IStream<Pose> {

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    void OnEnable() {
      OnOpen();
    }

    void Update() {
      OnSend(this.transform.ToWorldPose());
    }

    void OnDisable() {
      OnClose();
    }

  }


}
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Paint_GraspedPositionStream : MonoBehaviour,
                                             IStream<Vector3> {

    public InteractionBehaviour intObj;

    [Header("Debug")]
    [Tooltip("If enabled, forcibly streams position data as if the attached interaction "
           + "object were grasped.")]
    public bool spoofGrasp = false;

    public event Action OnOpen = () => { };
    public event Action<Vector3> OnSend = (position) => { };
    public event Action OnClose = () => { };

    private bool _isStreamOpen = false;

    private void Reset() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }

    private void Update() {
      bool shouldStream = intObj != null && (intObj.isGrasped || spoofGrasp);
      
      if (shouldStream && !_isStreamOpen) {
        OnOpen();
        _isStreamOpen = true;
      }
      if (!shouldStream && _isStreamOpen) {
        OnClose();
      }
      if (shouldStream) {
        OnSend(intObj.transform.position);
      }
    }

  }

}

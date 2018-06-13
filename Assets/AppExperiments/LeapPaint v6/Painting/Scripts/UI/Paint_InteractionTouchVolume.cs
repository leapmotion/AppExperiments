using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Leap.Unity.Apps.Paint6 {

  public class Paint_InteractionTouchVolume : InteractionBehaviour {

    public const float MAX_TOUCHING_DISTANCE = 0.015f;
    
    public float touchDistance {
      get { return primaryHoverDistance; }
    }

    [Header("Touch Interaction")]

    [SerializeField, Disable]
    private bool _isTouched = false;
    public bool isTouched { get { return _isTouched; } }

    [FormerlySerializedAs("OnTapEvent")]
    public UnityEvent OnTouchBeginEvent;
    public UnityEvent OnTouchEndEvent;

    void FixedUpdate() {
      bool currentlyTouched = false;
      if (isPrimaryHovered) {
        currentlyTouched = touchDistance < MAX_TOUCHING_DISTANCE;
      }

      if (currentlyTouched && !_isTouched) {
        _isTouched = true;

        OnTouchBeginEvent.Invoke();
      }

      if (!currentlyTouched && _isTouched) {
        _isTouched = false;

        OnTouchEndEvent.Invoke();
      }
    }

  }

}

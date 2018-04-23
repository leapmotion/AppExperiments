using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.LeapPaint {

  public class Paint_InteractionTouchVolume : InteractionBehaviour {

    public const float MAX_TOUCHING_DISTANCE = 0.015f;
    
    public float touchDistance {
      get { return primaryHoverDistance; }
    }

    [Header("Touch Interaction")]

    [SerializeField, Disable]
    private bool _isTouched = false;
    public bool isTouched { get { return _isTouched; } }

    public UnityEvent OnTapEvent;

    void FixedUpdate() {
      bool currentlyTouched = false;
      if (isPrimaryHovered) {
        currentlyTouched = touchDistance < MAX_TOUCHING_DISTANCE;
      }

      if (currentlyTouched && !_isTouched) {
        _isTouched = true;

        OnTapEvent.Invoke();
      }

      if (!currentlyTouched && _isTouched) {
        _isTouched = false;
      }
    }

  }

}

using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class InteractionButtonTrigger : MonoBehaviour, ITrigger {

    public InteractionButton button;

    private void Reset() {
      findButton();
    }

    private void OnValidate() {
      findButton();
    }

    private void findButton() {
      if (button == null) {
        button = GetComponent<InteractionButton>();
      }
    }

    public bool didFire {
      get {
        return _wasPressed;
      }
    }

    public bool isFiring {
      get {
        return button.isPressed;
      }
    }

    private bool _wasPressed = false;
    private bool _lastPressed = false;

    private void Update() {
      var isPressed = button.isPressed;

      if (_wasPressed && _lastPressed) {
        _wasPressed = false;
      }

      if (isPressed && !_lastPressed) {
        _wasPressed = true;
      }

      _lastPressed = isPressed;
    }

  }
  
}

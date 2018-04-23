using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapGraphicToggleBlendShapeController : LeapGraphicButtonBlendShapeController {

  public InteractionToggle toggle;

  protected override void Reset() {
    base.Reset();

    toggle = GetComponent<InteractionToggle>();
  }

  protected virtual void OnValidate() {
    if (button == null && toggle != null) {
      button = toggle;
    }

    if (button != null && !(button is InteractionToggle)) {
      button = null;
      Debug.LogError("LeapGraphicToggleBlendShapeController requires its InteractionButton to be a InteractionToggle.", this);
    }

    if (button != null && toggle == null && button is InteractionToggle) {
      toggle = (button as InteractionToggle);
    }
  }

  protected override void Update() {
    if (graphic != null && toggle != null && button != null) {
      try {
        if (!toggle.isToggled) {
          graphic.SetBlendShapeAmount(button.pressedAmount.Map(0F, 1F, 0F, 0.8F) + _scalePulsator.value);
        }
        else {
          graphic.SetBlendShapeAmount(0.8F + _scalePulsator.value);
        }
      }
      catch (System.Exception) {
        Debug.LogError("Error setting blend shape. Does the attached graphic have a blend shape feature?", this);
      }
    }
  }

}

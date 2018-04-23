using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapGraphicTogglePaletteController : LeapGraphicButtonPaletteController {

  [Header("Toggle")]
  public InteractionToggle toggle;

  public int toggledColorIdx;
  public int toggledPrimaryHoveredColorIdx;

  public Color toggledColor { get { return palette[toggledColorIdx]; } }
  public Color toggledPrimaryHoveredColor { get { return palette[toggledPrimaryHoveredColorIdx]; } }

  protected override void Reset() {
    base.Reset();

    toggle = GetComponent<InteractionToggle>();
  }

  protected override void OnValidate() {
    base.OnValidate();

    if (toggle != null && button == null) {
      button = toggle;
    }

    if (button != null && !(button is InteractionToggle)) {
      button = null;
      Debug.LogError("LeapGraphicTogglePaletteController requires an InteractionToggle attached to it to function.");
    }

    if (button != null && button is InteractionToggle && toggle == null) {
      toggle = (button as InteractionToggle);
    }
  }

  protected override Color updateTargetColor() {
    var targetColor = restingColor;

    if (toggle.isPressed) {
      targetColor = pressedColor;
    }
    else if (toggle.isToggled) {
      targetColor = toggledColor;
    }

    if (toggle.isPrimaryHovered) {
      if (toggle.isToggled) {
        targetColor = toggledPrimaryHoveredColor;
      }
      else {
        targetColor = primaryHoveredColor;
      }
    }

    if (!toggle.isToggled && !toggle.controlEnabled) {
      targetColor = controlDisabledColor;
    }

    return targetColor;
  }

}

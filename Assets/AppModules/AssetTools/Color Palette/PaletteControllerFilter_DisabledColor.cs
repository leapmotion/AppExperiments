using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PaletteControllerFilter_DisabledColor : PaletteControllerFilter {

    public InteractionButton button;

    public int disabledColorIdx;

    protected override void Reset() {
      base.Reset();

      if (button == null) {
        button = GetComponent<InteractionButton>();
      }
    }

    public override Color FilterGraphicPaletteTargetColor(Color inputTargetColor) {
      if (button.controlEnabled) {
        return inputTargetColor;
      }
      else {
        return controller.palette[disabledColorIdx];
      }
    }
  }

}

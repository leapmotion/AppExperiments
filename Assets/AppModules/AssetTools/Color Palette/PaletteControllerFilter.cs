using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public abstract class PaletteControllerFilter : MonoBehaviour {

    public GraphicPaletteController controller;

    protected virtual void Reset() {
      if (controller == null) {
        controller = GetComponent<GraphicPaletteController>();
      }

      if (controller != null && controller.filter == null) {
        controller.filter = this;
      }
    }

    protected virtual void OnValidate() {
      if (controller == null) {
        var attachedController = GetComponent<GraphicPaletteController>();
        if (attachedController != null && attachedController.filter == this) {
          controller = attachedController;
        }
      }
    }

    /// <summary>
    /// When a graphic palette controller has its filter slot set to this behaviour, it
    /// will call this method right after picking its target color for a given frame.
    /// Return the final target color for the graphic palette controller.
    /// </summary>
    public abstract Color FilterGraphicPaletteTargetColor(Color inputTargetColor);

  }

}

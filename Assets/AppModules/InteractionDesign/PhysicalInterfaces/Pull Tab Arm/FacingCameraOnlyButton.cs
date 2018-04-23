using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class FacingCameraOnlyButton : MonoBehaviour {

    public InteractionButton button;

    private void Reset() {
      findButton();
    }

    private void OnValidate() {
      findButton();
    }

    private void findButton() {
      if (button == null) button = GetComponent<InteractionButton>();
    }

    private void Update() {
      var isFacingCamera = (button.transform.forward * -1)
                             .IsFacing(button.transform.position,
                             Camera.main.transform.position, 120f);
      button.controlEnabled = isFacingCamera;
    }

  }

}

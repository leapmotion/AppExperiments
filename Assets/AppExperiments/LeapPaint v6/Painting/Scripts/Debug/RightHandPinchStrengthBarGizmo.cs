using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class RightHandPinchStrengthBarGizmo : MonoBehaviour {

    public Color color;

    float _pinchStrength = 0f;

    void Update() {
      var hand = Hands.Right;

      var useColor = color;

      if (hand != null) {
        _pinchStrength = Gestures.PinchGesture.Static_GetCustomPinchStrength(hand);

        var handFOVAngle = Vector3.Angle(Camera.main.transform.forward,
            hand.PalmPosition.ToVector3() - Camera.main.transform.position);
        var handWithinFOV = handFOVAngle < Camera.main.fieldOfView / 2.2f;

        if (!handWithinFOV) {
          useColor = Color.black;
        }
      }

      BarGizmo.Render(_pinchStrength, this.transform, useColor, 0.25f);
    }

  }

}

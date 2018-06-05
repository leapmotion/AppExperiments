using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class RightHandPinchStrengthBarGizmo : MonoBehaviour,
    IRuntimeGizmoComponent {

    public Color color;

    private float _pinchStrength = 0f;
    private Color _useColor;

    void Update() {
      var hand = Hands.Right;

      _useColor = color;

      if (hand != null) {
        _pinchStrength = Gestures.PinchGesture.Static_GetCustomPinchStrength(hand);

        var handFOVAngle = Vector3.Angle(Camera.main.transform.forward,
            hand.PalmPosition.ToVector3() - Camera.main.transform.position);
        var handWithinFOV = handFOVAngle < Camera.main.fieldOfView / 2.2f;

        if (!handWithinFOV) {
          _useColor = Color.black;
        }
      }
    }
    
    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.DrawBar(_pinchStrength, this.transform, _useColor, 0.25f);
    }

  }

}

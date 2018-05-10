using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Query;

public class PortalHandle : MonoBehaviour {

  public LeapProvider provider;
  public BoxCollider box;
  public float beginPinch = 0.3f;
  public float endPinch = 0.2f;

  [Header("Hover")]
  public float beginHoverDist = 0.2f;
  public Color baseColor;
  public Color hoverColor;
  public Color graspColor;

#if UNITY_EDITOR
  new
#endif
  public Renderer renderer;

  private int _graspedId = -1;
  private bool _grasped = false;

  public bool isGrasped {
    get {
      return _grasped;
    }
    set {
      _grasped = value;
    }
  }

  public Vector3 position {
    get {
      if (_grasped) {
        return provider.CurrentFrame.Hands.
               Query().
               FirstOrNone(h => h.Id == _graspedId).
               Match(h => {
                 return h.GetPinchPosition();
               },
               () => {
                 return transform.position;
               });
      } else {
        return transform.position;
      }
    }
  }

  private void Start() {
    renderer = GetComponentInParent<Renderer>();
  }

  private void Update() {
    if (!_grasped) {
      float hoverDist = float.MaxValue;
      Bounds localBounds = new Bounds(box.center, box.size);

      foreach (var hand in provider.CurrentFrame.Hands) {
        Vector3 localTip = transform.InverseTransformPoint(hand.GetPinchPosition());

        hoverDist = Mathf.Min(hoverDist, Mathf.Sqrt(localBounds.SqrDistance(localTip)));

        if (hand.PinchStrength > beginPinch && localBounds.Contains(localTip)) {
          _graspedId = hand.Id;
          _grasped = true;
          break;
        }
      }

      renderer.material.color = Color.Lerp(baseColor, hoverColor, Mathf.InverseLerp(beginHoverDist, 0, hoverDist));
    } else {
      _grasped = false;
      foreach (var hand in provider.CurrentFrame.Hands) {
        if (hand.Id == _graspedId && hand.PinchStrength > endPinch) {
          _grasped = true;
          break;
        }
      }
      renderer.material.color = graspColor;
    }
  }
}

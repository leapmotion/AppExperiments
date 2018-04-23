using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  [RequireComponent(typeof(RectTransform))]
  public class RectTransformBehaviour : MonoBehaviour {
    
    private RectTransform _rectTransform;
    public RectTransform rectTransform {
      get {
        if (_rectTransform == null) {
          _rectTransform = GetComponent<RectTransform>();
        }
        return _rectTransform;
      }
    }

  }

}

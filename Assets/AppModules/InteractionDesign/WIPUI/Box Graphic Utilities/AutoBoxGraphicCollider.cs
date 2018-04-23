using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.WIPUI {

  [ExecuteInEditMode]
  public class AutoBoxGraphicCollider : MonoBehaviour {

    [MinValue(0f)]
    public float additionalDepth = 0.005f;

    [MinValue(0f)]
    public float additionalNegativeDepth = 0f;

    [MinValue(0f)]
    public float additionalXYPadding = 0f;

#if UNITY_EDITOR
    void Update() {
      if (Application.isPlaying) return;

      var boxGraphic = GetComponent<LeapBoxGraphic>();
      if (boxGraphic != null) {
        var someCollider = GetComponent<Collider>();
        if (someCollider != null && !(someCollider is BoxCollider)) {
          Debug.LogError("AutoBoxGraphicCollider only works with _box_ colliders.");
          return;
        }

        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) {
          boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (boxCollider != null) {
          boxCollider.size = new Vector3(boxGraphic.size.x + additionalXYPadding * 2f,
                                         boxGraphic.size.y + additionalXYPadding * 2f,
                                         boxGraphic.size.z + additionalDepth);
          boxCollider.center = -boxGraphic.GetBoxCenter()
                                 + new Vector3(boxGraphic.size.x / 2F,
                                               boxGraphic.size.y / 2F,
                                               -boxGraphic.size.z / 2F
                                                 + additionalDepth / 2F
                                                 - additionalNegativeDepth / 2F);
        }
      }
    }
#endif

  }

  public static class LeapBoxGraphicExtensions {

    public static Vector3 GetBoxCenter(this LeapBoxGraphic boxGraphic) {
      var rectTransform = boxGraphic.GetComponent<RectTransform>();
      if (rectTransform != null) {
        return new Vector2(boxGraphic.size.x * rectTransform.pivot.x,
                           boxGraphic.size.y * rectTransform.pivot.y);
      }
      else {
        return Vector3.zero;
      }
    }

  }

}

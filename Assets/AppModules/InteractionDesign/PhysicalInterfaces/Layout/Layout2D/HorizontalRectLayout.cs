using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  public class HorizontalRectLayout : RectLayoutBehaviour {

    protected override void UpdateLayoutTargets() {
      // Ignored currently.
    }

    protected override void UpdateLayoutVisuals() {
      if (layoutTransforms.Length == 0) return;

      var   rect = rectTransform.rect;
      float rectWidth = rect.width / layoutTransforms.Length;

      float normalizedWidth = 1f / layoutTransforms.Length;
      for (int i = 0; i < layoutTransforms.Length; i++) {
        var layoutTransform = layoutTransforms[i];

        RectTransform layoutRectTransform = layoutTransform.GetComponent<RectTransform>();
        if (layoutRectTransform != null) {
          // Rect transform.

          layoutRectTransform.pivot = Vector2.one * 0.5f;

          layoutRectTransform.anchorMin = new Vector2(normalizedWidth * i, 0f);
          layoutRectTransform.anchorMax = new Vector2(normalizedWidth * (i + 1), 1f);
        }
        else {
          // Transform only.
          layoutTransform.localPosition = new Vector3(
            rect.size.x - (rectTransform.pivot.x * rect.size.x) - rectWidth * (i + 0.5f),
            0f,
            0f);
        }
      }
    }

  }

}

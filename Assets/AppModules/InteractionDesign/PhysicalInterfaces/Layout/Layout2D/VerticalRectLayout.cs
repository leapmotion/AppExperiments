using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  public class VerticalRectLayout : RectLayoutBehaviour {

    protected override void UpdateLayoutTargets() {
      // Ignored currently.
    }

    protected override void UpdateLayoutVisuals() {
      if (layoutTransforms.Length == 0) return;

      var   rect = rectTransform.rect;
      float rectHeight = rect.height / layoutTransforms.Length;

      float normalizedHeight = 1f / layoutTransforms.Length;
      for (int i = 0; i < layoutTransforms.Length; i++) {
        var layoutTransform = layoutTransforms[i];

        RectTransform layoutRectTransform = layoutTransform.GetComponent<RectTransform>();
        if (layoutRectTransform != null) {
          // Rect transform.

          layoutRectTransform.pivot = Vector2.one * 0.5f;

          layoutRectTransform.anchorMin = new Vector2(0f, 1f - normalizedHeight * (i + 1));
          layoutRectTransform.anchorMax = new Vector2(1f, 1f - normalizedHeight * i);
        }
        else {
          // Transform only.
          layoutTransform.localPosition = new Vector3(0f,
            rect.size.y - rectHeight * (i + 0.5f),
            0f);
        }
      }
    }

  }

}

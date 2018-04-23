using Leap.Unity.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  [ExecuteInEditMode]
  public class PaintbrushModelSizeMatcher : MonoBehaviour {

    public Paintbrush paintbrush;

    public bool executeInEditMode = true;

    [Header("Brush Radius to (this) Local Scale Map Parameters")]

    public Vector4 brushRadiusToXScaleMap = new Vector4(0.04f, 0.005f, 1f, (0.005f/0.04f));
    public Vector4 brushRadiusToYScaleMap = new Vector4(0.04f, 0.005f, 1f, 1f);
    public Vector4 brushRadiusToZScaleMap = new Vector4(0.04f, 0.005f, 1f, 1f);

    void Reset() {
      if (paintbrush == null) {
        paintbrush = GetComponentInParent<Paintbrush>();
      }
    }

    void Update() {
      if (!Application.isPlaying && !executeInEditMode) return;

      if (paintbrush != null) {
        var radius = paintbrush.radius;

        var sX = radius.Map(brushRadiusToXScaleMap);
        var sY = radius.Map(brushRadiusToYScaleMap);
        var sZ = radius.Map(brushRadiusToZScaleMap);

        this.transform.localScale = new Vector3(sX, sY, sZ);
      }
    }

  }

  public static class PaintbrushModelSizeMatcherExtensions {

    public static float Map(this float f, Vector4 mapParams) {
      return f.Map(mapParams.x, mapParams.y, mapParams.z, mapParams.w);
    }

  }

}
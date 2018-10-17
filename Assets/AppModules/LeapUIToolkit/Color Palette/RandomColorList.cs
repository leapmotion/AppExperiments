using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [System.Serializable]
  public struct RandomColorList {

    public const int MAX_COLOR_COUNT = 1024;

    [SerializeField]
    private List<Color> _backingColors;
    private List<Color> _colors {
      get {
        if (_backingColors == null) {
          _backingColors = new List<Color>(MAX_COLOR_COUNT);
          for (int i = 0; i < 8; i++) { _backingColors.Add(new Color()); }
        }
        return _backingColors;
      }
    }

    public Color this[int idx] {
      get {
        if (idx >= MAX_COLOR_COUNT) {
          idx = MAX_COLOR_COUNT - 1;
          Debug.LogWarning("Maximum color count is " + MAX_COLOR_COUNT + "; " + 
            "using index " + idx + " instead.");
        }
        while (_colors.Count < idx + 1) {
          _colors.Add(new Color());
        }
        if (!exists(_colors[idx])) {
          _colors[idx] = newRandomColor();
        }
        return _colors[idx];
      }
      set {
        if (idx >= MAX_COLOR_COUNT) {
          idx = MAX_COLOR_COUNT - 1;
          Debug.LogWarning("Maximum color count is " + MAX_COLOR_COUNT + "; " + 
            "using index " + idx + " instead.");
        }
        while (_backingColors.Count < idx + 1) {
          _backingColors.Add(new Color());
        }
        _backingColors[idx] = value;
      }
    }

    private bool exists(Color c) {
      return c.r != 0f || c.g != 0f || c.b != 0f || c.a != 0f;
    }
    
    private int _randIndex;
    private const float ALPHA_GOLDEN = 1.61803398875f;
    private Color newRandomColor() {
      var n = _randIndex;
      var g = 1.22074408460575947536f;
      var a1 = 1.0f / g;
      var a2 = 1.0f / (g * g);
      var a3 = 1.0f / (g * g * g);
      var hue = (0.5f + a1 * n) % 1;
      var sat = (0.5f + a2 * n) % 1;
      var val = (0.5f + a3 * n) % 1;
      // Bias saturation and value towards more vibrant colors.
      sat = sat.Map(0f, 1f, 0.2f, 1f);
      val = val.Map(0f, 1f, 0.2f, 1f);
      sat = Mathf.Sqrt(sat); val = Mathf.Sqrt(val);
      _randIndex += 1;
      return Color.HSVToRGB(hue, sat, val);
    }

  }

}
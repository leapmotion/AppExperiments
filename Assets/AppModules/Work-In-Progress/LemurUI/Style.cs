

namespace Leap.Unity.LemurUI {

  using UnityColor = UnityEngine.Color;

  public static class Style {

    public static ColorOnlyStyle Color(UnityColor color) {
      return new ColorOnlyStyle() { color = color };
    }
    public struct ColorOnlyStyle {
      public UnityColor color;

      public static implicit operator TextStyle(ColorOnlyStyle c) {
        return new TextStyle() { color = c.color };
      }
    }

  }

}
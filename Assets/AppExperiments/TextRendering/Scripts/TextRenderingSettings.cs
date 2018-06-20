using UnityEngine;
using Leap.Unity.DevGui;

public enum FontChoices {
  SansSerif,
  Serif,
  Monospaced
}

public enum AnchorPosition {
  Hand,
  Ahead
}

public enum TextRenderingMethod {
  DistanceField,
  TextureGlyph,
  Mesh
}

public class TextRenderingSettings : MonoBehaviour {

  [DevCategory("Preset Values")]
  [DevValue]
  public TextRenderingMethod method;

  [DevValue]
  public FontChoices font;

  [DevValue, Range(0, 40)]
  public float fontSize;

  [DevValue, Range(0, 1)]
  public float glyphResolution;

  [DevValue, Range(0, 1)]
  public float textColor = 0;

  [DevValue, Range(0, 1)]
  public float backgroundColor = 0;

  [DevValue, Range(0, 1)]
  public float skyColor = 0.5f;

  [DevValue, Range(0, 2)]
  public float panelDistance = 0.5f;
}

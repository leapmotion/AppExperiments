using UnityEngine;
using TMPro;
using Leap.Unity.DevGui;
using UnityEngine.UI;

public class TextRendererController : MonoBehaviour {

  public TextRenderingSettings settings;

  [DevCategory("Render Mode")]
  public float fontSizeMultiplier = 1;
  public Image backgroundImage;

  public GameObject distanceFieldAnchor;
  public TextMeshProUGUI distanceFieldText;

  public GameObject glyphAnchor;
  public CanvasScaler glyphScalar;
  public Text glyphText;

  public GameObject meshTextAnchor;
  public GameObject meshText;

  [Header("Fonts Assets")]
  public TMP_FontAsset sansSerifTMP;
  public TMP_FontAsset serifTMP;
  public TMP_FontAsset monospacedTMP;
  public Font sansSerifGlyph;
  public Font serifGlyph;
  public Font monospacedGlyph;

  private float multipliedFontSize {
    get {
      return settings.fontSize * fontSizeMultiplier;
    }
  }


  private void Update() {
    setEnabled((int)settings.method, distanceFieldAnchor, glyphAnchor, meshTextAnchor);

    switch (settings.method) {
      case TextRenderingMethod.DistanceField:
        updateDistanceText();
        break;
      case TextRenderingMethod.TextureGlyph:
        updateGlyphText();
        break;
      case TextRenderingMethod.Mesh:
        break;
    }

    backgroundImage.color = getGrayscale(settings.backgroundColor);
  }

  private void updateDistanceText() {
    switch (settings.font) {
      case FontChoices.SansSerif:
        distanceFieldText.font = sansSerifTMP;
        break;
      case FontChoices.Serif:
        distanceFieldText.font = serifTMP;
        break;
      case FontChoices.Monospaced:
        distanceFieldText.font = monospacedTMP;
        break;
    }

    distanceFieldText.fontSize = multipliedFontSize;
    distanceFieldText.color = getGrayscale(settings.textColor);
  }

  private void updateGlyphText() {
    switch (settings.font) {
      case FontChoices.SansSerif:
        glyphText.font = sansSerifGlyph;
        break;
      case FontChoices.Serif:
        glyphText.font = serifGlyph;
        break;
      case FontChoices.Monospaced:
        glyphText.font = monospacedGlyph;
        break;
    }

    glyphText.fontSize = Mathf.RoundToInt(multipliedFontSize);
    glyphText.color = getGrayscale(settings.textColor);
    glyphScalar.dynamicPixelsPerUnit = settings.glyphResolution;
  }

  private Color getGrayscale(float f) {
    return new Color(f, f, f, 1);
  }

  private void setEnabled(int value, params GameObject[] objs) {
    for (int i = 0; i < objs.Length; i++) {
      bool shouldBeEnabled = i == value;
      if (shouldBeEnabled != objs[i].activeSelf) {
        objs[i].SetActive(shouldBeEnabled);
      }
    }
  }
}

using Leap.Unity.GraphicalRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ZZOLD_LeapGraphicPaletteController : MonoBehaviour {

  public LeapGraphic graphic;

  public ColorPalette palette;
  public int colorIndex;

  public static ColorPalette s_lastPalette;

  private bool _paletteWasNull = true;

  void Reset() {
    graphic = GetComponent<LeapGraphic>();

    if (palette == null && s_lastPalette != null) {
      palette = s_lastPalette;
    }
  }

  void OnValidate() {
    if (palette != null) {
      if (_paletteWasNull) {
        _paletteWasNull = false;
        s_lastPalette = palette;
      }

      if (palette.colors.Length != 0) {
        colorIndex = Mathf.Max(0, Mathf.Min(palette.colors.Length - 1, colorIndex));

        refreshColor();
      }
    }
  }

  void Update() {
    if (!Application.isPlaying) {
      refreshColor();
    }
  }

  private void refreshColor() {
    if (palette == null) return;
    if (graphic == null) return;

    Color color = palette.colors[colorIndex];

    var text = graphic as LeapTextGraphic;
    if (text != null) {
      text.color = color;
    }
    else {
      try { graphic.SetRuntimeTint(color); }
      catch (System.Exception) { }
    }
  }

}

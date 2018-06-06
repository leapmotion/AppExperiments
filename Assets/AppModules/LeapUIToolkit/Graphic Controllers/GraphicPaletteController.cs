using Leap.Unity.GraphicalRenderer;
using UnityEngine;

namespace Leap.Unity.ColorPalettes {

  [ExecuteInEditMode]
  public class GraphicPaletteController : MonoBehaviour,
                                          IPaletteColorReceiver {

    public LeapGraphic graphic;

    [Header("Graphic Alternative (MeshRenderer) -- Play Mode Only")]

    public new Renderer renderer;
    public string shaderColorName = "_Color";
    private int _shaderColorID;

    [Header("Palette")]

    public ColorPalette palette;
    public float colorChangeSpeed = 20F;

    public static ColorPalette s_lastPalette;

    private bool _paletteWasNull = true;
    private Color _targetColor;

    [Header("Palette Filter")]

    public PaletteControllerFilter filter = null;

    [Header("Palette Colors")]

    public int restingColorIdx;
    public Color restingColor { get { return palette[restingColorIdx]; } }

    protected virtual void Reset() {
      graphic = GetComponent<LeapGraphic>();
      if (graphic == null) {
        renderer = GetComponent<Renderer>();
      }

      if (palette == null && s_lastPalette != null) {
        palette = s_lastPalette;
      }
    }

    protected virtual void OnValidate() {
      if (palette != null) {
        if (_paletteWasNull) {
          _paletteWasNull = false;
          s_lastPalette = palette;
        }
        validateColorIdx(ref restingColorIdx);
        setColor(restingColor);
      }

      refreshRendererShaderID();
    }

    protected virtual void Start() {
      refreshRendererShaderID();
    }

    private void refreshRendererShaderID() {
      _shaderColorID = Shader.PropertyToID(shaderColorName);
    }

    protected void validateColorIdx(ref int colorIdx) {
      colorIdx = Mathf.Max(0, Mathf.Min(palette.colors.Length - 1, colorIdx));
    }

    protected virtual void Update() {
      if (palette == null) return;
      if (graphic == null && renderer == null) return;

      if (Application.isPlaying) {
        _targetColor = updateTargetColor();
      } else {
        _targetColor = palette[restingColorIdx];
      }

      if (filter != null) {
        _targetColor = filter.FilterGraphicPaletteTargetColor(_targetColor);
      }

      if (Application.isPlaying) {
        Color curColor = getColor();
        if (curColor != _targetColor) {
          setColor(Color.Lerp(curColor, _targetColor, colorChangeSpeed * Time.deltaTime));
        }
      } else {
        setColor(_targetColor);
      }
    }

    protected virtual Color updateTargetColor() {
      return palette[restingColorIdx];
    }

    protected Color getColor() {
      if (graphic != null) {
        var text = graphic as LeapTextGraphic;
        if (text != null) {
          return text.color;
        } else {
          return graphic.GetRuntimeTint();
        }
      } else {
        if (Application.isPlaying) {
          return renderer.material.GetColor(_shaderColorID);
        } else {
          return renderer.sharedMaterial.GetColor(_shaderColorID);
        }
      }
    }

    private void setColor(int colorIdx) {
      setColor(palette[colorIdx]);
    }

    private void setColor(Color color) {
      var text = graphic as LeapTextGraphic;
      if (text != null) {
        text.color = color;
      } else {
        if (graphic != null) {
          graphic.SetRuntimeTint(color);
        } else {
          if (renderer != null) {
            if (Application.isPlaying) {
              renderer.material.SetColor(_shaderColorID, color);
            } else {
              renderer.sharedMaterial.SetColor(_shaderColorID, color);
            }
          }
        }
      }
    }

    public void Receive(int paletteColorIdx) {
      restingColorIdx = paletteColorIdx;
    }
  }

}

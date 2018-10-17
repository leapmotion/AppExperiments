using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity {

  public class ColorControl : MonoBehaviour, IColorReceiver {

    public Color targetColor = Color.white;

    public enum ControllerMode {
      Automatic    =   0,
      MeshRenderer = 100
    }
    [Tooltip("If the controller mode is set to automatic, the highest mode for " +
       "which there is valid data will get precedence.")]
    public ControllerMode mode = ControllerMode.Automatic;

    public
    #if UNITY_EDITOR
    new
    #endif
    Renderer renderer;
    [OnEditorChange("shaderColorName")]
    private string _shaderColorName = "_Color";
    public string shaderColorName {
      get { return _shaderColorName; }
      set {
        _shaderColorName = value;
        if (Application.isPlaying) { refreshRendererShaderID(); }
      }
    }
    private int _shaderColorID;

    [MinValue(0f)]
    [Tooltip("Lerp coefficient per-frame is this value * Time.deltaTime. If set " +
      "to zero, changes are instantaneous.")]
    public float colorChangeSpeed = 20F;

    // /// <summary> ColorControllers will try to remember the last palette that
    // /// was added to one, to auto-initialize with that palette. </summary>
    // private static ColorPalette s_lastPalette;

    private void Reset() {
      if (renderer == null) { renderer = GetComponent<Renderer>(); }
      if (renderer == null) { renderer = GetComponentInChildren<Renderer>(); }
    }

    private void OnValidate() {
      refreshRendererShaderID();
    }

    private void OnEnable() {
      if (renderer == null) { renderer = GetComponent<Renderer>(); }
      if (renderer == null) { renderer = GetComponentInChildren<Renderer>(); }
      refreshRendererShaderID();

      setColor(targetColor);
    }

    private void refreshRendererShaderID() {
      _shaderColorID = Shader.PropertyToID(shaderColorName);
    }

    private void Update() {
      if (Application.isPlaying) {
        Color curColor = getColor();
        if (curColor != targetColor) {
          setColor(Color.Lerp(curColor, targetColor,
            colorChangeSpeed * Time.deltaTime));
        }
      } else {
        setColor(targetColor);
      }
    }

    protected Color getColor() {
      if (renderer != null) {
        if (Application.isPlaying) {
          return renderer.material.GetColor(_shaderColorID);
        } else {
          return renderer.sharedMaterial.GetColor(_shaderColorID);
        }
      }
      else {
        return Color.magenta;
      }
    }

    private void setColor(Color color) {
      if (renderer != null) {
        if (Application.isPlaying) {
          renderer.material.SetColor(_shaderColorID, color);
        } else {
          renderer.sharedMaterial.SetColor(_shaderColorID, color);
        }
      }
    }

    public void Receive(Color color) {
      targetColor = color;
    }

  }
  
}
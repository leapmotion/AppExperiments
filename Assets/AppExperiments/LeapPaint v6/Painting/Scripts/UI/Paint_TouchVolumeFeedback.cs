using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Paint_TouchVolumeFeedback : MonoBehaviour {

    public Paint_InteractionTouchVolume touchVolume;

    public float lerpCoeffPerSec = 20f;

    #region Color Feedback

    [Header("Color Feedback")]

    public Color maxEmissionColor = Color.white;

    public float idleEmissionAmount = 0f;
    public float primaryHoverEmissionAmount = 0.1f;
    public float pressedEmissionAmount = 1f;

    public Renderer colorRendererToSet;
    private Material _colorMaterialInstance = null;

    [EditTimeOnly]
    public string emissionPropertyName = "_EmissionColor";
    private int _colorShaderPropId = -1;

    [SerializeField]
    [Disable]
    private Color _emissionColor;

    public Color targetIdleColor {
      get { return Color.Lerp(Color.black, maxEmissionColor, idleEmissionAmount); }
    }
    public Color targetPrimaryHoverColor {
      get { return Color.Lerp(Color.black, maxEmissionColor, primaryHoverEmissionAmount); }
    }
    public Color targetPressedColor {
      get { return Color.Lerp(Color.black, maxEmissionColor, pressedEmissionAmount); }
    }

    #endregion

    #region Outline Feedback

    [Header("Outline Feedback")]

    public bool doOutlineFeedback = true;

    public float outlinePrimaryHoverMult = 2.5f;
    public float outlinePressedMult = 0.3f;

    public Renderer outlineRendererToSet;
    private Material _outlineMaterialInstance = null;

    [EditTimeOnly]
    public string outlinePropertyName = "_Width";
    private int _outlineShaderPropId = -1;

    [SerializeField]
    [Disable]
    private float _baseOutline = -1f;

    [SerializeField]
    [Disable]
    private float _currentOutline = -1f;

    #endregion

    void Reset() {
      if (colorRendererToSet == null) {
        colorRendererToSet = GetComponentInChildren<Renderer>();
      }
      if (outlineRendererToSet == null && doOutlineFeedback) {
        outlineRendererToSet = GetComponentInChildren<Renderer>();
      }

      if (touchVolume == null) {
        touchVolume = GetComponent<Paint_InteractionTouchVolume>();
      }
    }

    void OnEnable() {
      _emissionColor = targetIdleColor;

      if (_colorMaterialInstance == null) {
        _colorMaterialInstance = colorRendererToSet.material;
      }

      if (_outlineMaterialInstance == null && outlineRendererToSet != null) {
        _outlineMaterialInstance = outlineRendererToSet.material;
      }

      // In the re-enable case (Start() already called), recalculate base outline value
      // from the material.
      if (_outlineMaterialInstance != null && _outlineShaderPropId != -1
          && doOutlineFeedback) {
        _baseOutline = _outlineMaterialInstance.GetFloat(_outlineShaderPropId);
        _currentOutline = _baseOutline;
      }
    }

    void Start() {
      _colorShaderPropId = Shader.PropertyToID(emissionPropertyName);
      _outlineShaderPropId = Shader.PropertyToID(outlinePropertyName);

      if (_outlineMaterialInstance != null && doOutlineFeedback) {
        _baseOutline = _outlineMaterialInstance.GetFloat(_outlineShaderPropId);
        _currentOutline = _baseOutline;
      }
    }

    void Update() {
      // Update color.
      if (_colorMaterialInstance != null) {
        Color targetEmissionColor;
        if (touchVolume.isTouched) {
          targetEmissionColor = targetPressedColor;
        }
        else if (touchVolume.isPrimaryHovered) {
          targetEmissionColor = targetPrimaryHoverColor;
        }
        else {
          targetEmissionColor = targetIdleColor;
        }

        _emissionColor = Color.Lerp(_emissionColor, targetEmissionColor,
                                    lerpCoeffPerSec * Time.deltaTime);

        _colorMaterialInstance.SetColor(_colorShaderPropId, _emissionColor);
      }

      // Update outline.
      if (_outlineMaterialInstance != null && doOutlineFeedback) {
        float targetOutline;
        if (touchVolume.isTouched) {
          targetOutline = _baseOutline * outlinePressedMult;
        }
        else if (touchVolume.isPrimaryHovered) {
          targetOutline = _baseOutline * outlinePrimaryHoverMult;
        }
        else {
          targetOutline = _baseOutline;
        }

        _currentOutline = Mathf.Lerp(_currentOutline, targetOutline,
                                     lerpCoeffPerSec * Time.deltaTime);

        _outlineMaterialInstance.SetFloat(_outlineShaderPropId, _currentOutline);
      }
    }

  }

}

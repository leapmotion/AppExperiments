using Leap.Unity;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapGraphicButtonPaletteController : GraphicPaletteController {

  [Header("Button")]

  public InteractionButton button;

  public int primaryHoverColorIdx;
  public int pressedColorIdx;
  public int controlDisabledColorIdx;

  private Pulsator _pressPulsator;

  public Color primaryHoveredColor { get { return palette[primaryHoverColorIdx]; } }
  public Color pressedColor { get { return palette[pressedColorIdx]; } }
  public Color controlDisabledColor { get { return palette[controlDisabledColorIdx]; } }

  protected override void Reset() {
    base.Reset();

    button = GetComponent<InteractionButton>();
  }

  protected override void OnValidate() {
    base.OnValidate();

    if (palette != null) {
      if (palette.colors.Length != 0) {
        validateColorIdx(ref primaryHoverColorIdx);
        validateColorIdx(ref pressedColorIdx);
      }
    }
  }

  protected virtual void OnEnable() {
    if (!Application.isPlaying) return;

    button.OnPress   += onPress;
    button.OnUnpress += onUnpress;
  }

  protected virtual void OnDisable() {
    if (!Application.isPlaying) return;

    button.OnPress   -= onPress;
    button.OnUnpress -= onUnpress;
  }

  private void onPress() { _pressPulsator.Pulse(); }
  private void onUnpress() { _pressPulsator.Relax(); }

  protected override void Start() {
    base.Start();

    if (Application.isPlaying) _pressPulsator = Pulsator.Spawn().SetValues(0F, 1F, 0.8F).SetSpeed(20F);
  }
  
  protected virtual void OnDestroy() {
    if (_pressPulsator != null) {
      Pulsator.Recycle(_pressPulsator);
    }
  }

  protected override Color updateTargetColor() {
    var targetColor = restingColor;
    
    if (!button.controlEnabled) {
      targetColor = controlDisabledColor;
    }
    else if (!_pressPulsator.isResting) {
      if (_pressPulsator.value < 1.0F) {
        targetColor = Color.Lerp(restingColor, pressedColor, _pressPulsator.value);
      }
      else {
        targetColor = Color.Lerp(pressedColor, restingColor, _pressPulsator.value - 1F);
      }
    }
    else if (button.isPrimaryHovered) {
      targetColor = primaryHoveredColor;
    }

    return targetColor;
  }

}

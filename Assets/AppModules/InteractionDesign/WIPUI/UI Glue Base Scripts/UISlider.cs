using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UISlider : UIButton {

  [Header("Slider")]

  public InteractionSlider slider;

  private Action<float> onSliderValue;

  protected override void initialize() {
    base.initialize();

    if (button != null && button is InteractionSlider) {
      slider = button as InteractionSlider;
    }

    if (slider != null) {
      button = slider;
    }

    onSliderValue = OnSliderValue;
  }

  protected override void OnEnable() {
    base.OnEnable();

    if (slider != null) {
      //slider.VerticalSlideEvent -= OnSliderValue;
      //slider.VerticalSlideEvent += OnSliderValue;

      slider.HorizontalSlideEvent -= onSliderValue;
      slider.HorizontalSlideEvent += onSliderValue;
    }
  }

  protected override void OnDisable() {
    base.OnDisable();

    if (slider != null) {
      slider.HorizontalSlideEvent -= onSliderValue;
    }
  }

  protected virtual void Start() {
    slider.defaultHorizontalValue = GetStartingSliderValue();
    slider.HorizontalSliderValue  = slider.defaultHorizontalValue;
  }

  public virtual void OnSliderValue(float value) { }

  public abstract float GetStartingSliderValue();

}

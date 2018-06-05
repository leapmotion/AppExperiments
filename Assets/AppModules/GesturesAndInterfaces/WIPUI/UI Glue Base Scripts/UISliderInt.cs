using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UISliderInt : UISlider {

  protected override void initialize() {
    base.initialize();

    if (slider != null) {
      slider.minHorizontalValue = GetMinValue();
      slider.maxHorizontalValue = GetMaxValue();
      slider.horizontalSteps = (GetMaxValue() - GetMinValue());
    }
  }

  public abstract int GetMinValue();

  public abstract int GetMaxValue();

  public virtual void OnSliderValue(int value) { }

  public sealed override void OnSliderValue(float value) {
    OnSliderValue(Mathf.RoundToInt(value));
  }

}

using Leap.Unity.UI;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class Slider_UconChannel : UISlider {

    [Header("Ucon Channel Output")]
    public FloatChannel sliderOutputChannel = new FloatChannel("brush/radius");

    public override float GetStartingSliderValue() {
      return slider.defaultHorizontalValue;
    }

    public override void OnSliderValue(float value) {
      base.OnSliderValue(value);

      sliderOutputChannel.Set(value);
    }

  }

}

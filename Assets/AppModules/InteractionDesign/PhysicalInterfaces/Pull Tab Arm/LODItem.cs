using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODItem : MonoBehaviour {
  
  [Header("Tween Switch (override propertySwitch)")]

  public TweenSwitch tweenSwitch;

  [Header("Or, non-Tween switch")]

  [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
  private MonoBehaviour _propertySwitch;
  public IPropertySwitch propertySwitch {
    get {
      if (tweenSwitch != null) return tweenSwitch;
      return _propertySwitch as IPropertySwitch;
    }
  }

}

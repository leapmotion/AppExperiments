using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.Animation.Examples {

  public class ScalePulsatorBehaviour : MonoBehaviour {

    [SerializeField]
    [OnEditorChange("speed")]
    private float _speed = 1f;
    public float speed {
      get { return _speed; }
      set {
        _speed = value;
        _pulsator.speed = value;
      }
    }

    public float warmHeight = 1.1f;
    public float pulseHeight = 1.3f;
    public float restHeight = 1f;

    public Vector3 pulseComponentStrengths = Vector3.one;

    private Pulsator _pulsator;

    private void OnEnable() {
      if (_pulsator == null) _pulsator = new Pulsator();

      _pulsator.SetSpeed(speed);

      _pulsator.pulseValue = pulseHeight;
      _pulsator.warmValue = warmHeight;
      _pulsator.restValue = restHeight;
    }

    private void Update() {
      _pulsator.UpdatePulsator(Time.deltaTime);

      var xComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.x);
      var yComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.y);
      var zComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.z);
      
      this.transform.localScale = new Vector3(xComp, yComp, zComp);
    }
    
    public static implicit operator Pulsator(ScalePulsatorBehaviour behaviour) {
      return behaviour._pulsator;
    }

  }

}

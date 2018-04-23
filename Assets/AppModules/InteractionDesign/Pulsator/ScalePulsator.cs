using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalePulsator : MonoBehaviour {

  public float speed = 0.5f;

  public float activeHeight = 1f;
  public float pulseHeight = 1.3f;
  public float restHeight = 1f;

  public Vector3 pulseComponentStrengths = Vector3.one;

  private Pulsator _pulsator;

  private void OnEnable() {
    if (_pulsator == null) _pulsator = new Pulsator();

    _pulsator.SetSpeed(speed);

    _pulsator.pulse = pulseHeight;
    _pulsator.active = activeHeight;
    _pulsator.rest = restHeight;
  }

  private void Update() {
    _pulsator.UpdatePulsator(Time.deltaTime);

    var xComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.x);
    var yComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.y);
    var zComp = Mathf.Lerp(1f, _pulsator.value, pulseComponentStrengths.z);
    
    this.transform.localScale = new Vector3(xComp, yComp, zComp);
  }

  public void WarmUp() {
    _pulsator.WarmUp();
  }

  public void Pulse() {
    _pulsator.Pulse();
  }

  public void Relax() {
    _pulsator.Relax();
  }

}

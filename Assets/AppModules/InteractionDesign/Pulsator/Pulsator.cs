using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulsator : IPoolable {

  public float rest   = 0F;
  public float pulse  = 1.2F;
  public float active = 1F;

  private float _speed = 1F;
  public float speed {
    get { return _speed; }
    set {
      float s = value;
      if (s <= 0F) s = 0.001F;
      _speed = s * 20F;
      _value.SetBlend(1 / (speed + 1.01F), 1F);
    }
  }

  private enum State {
    Resting,
    Warming,
    Pulsing
  }
  private State _state;

  public bool isResting {
    get {
      return _state == State.Resting;
    }
  }

  private SmoothedFloat _value;
  public float value {
    get {
      return _value.value;
    }
    set {
      _value.reset = true;
      _value.Update(value);
    }
  }

  private bool _isEnabled = true;
  /// <summary>
  /// Returns whether the pulsator is receiving updates from the Pulsator Runner.
  /// Disabling a pulsator effectively "pauses" it.
  /// </summary>
  public bool isEnabled {
    get { return _isEnabled; }
    set {
      if (value) {
        Enable();
      }
      else {
        Disable();
      }
    }
  }

  private float _targetValue = 0F;

  /// <summary>
  /// Returns a pooled instance of a new Pulsator. Be sure to call Pulsator.Recycle()
  /// when you're done with the Pulsator.
  /// </summary>
  public static Pulsator Spawn() {
    return Pool<Pulsator>.Spawn();
  }

  /// <summary>
  /// Recycles a Pulsator you're done using, to potentially be retrieved again via
  /// Pulsator.Spawn().
  /// </summary>
  /// <param name="p"></param>
  public static void Recycle(Pulsator p) {
    Pool<Pulsator>.Recycle(p);
  }

  public Pulsator() {
    Enable();
    if (_value == null) _value = new SmoothedFloat();
  }

  public void OnSpawn() { }

  public void OnRecycle() {
    Reset();
    Disable();
  }

  public void Enable() {
    _isEnabled = true;

    PulsatorRunner.NotifyEnabled(this);
  }

  public void Disable() {
    _isEnabled = false;

    PulsatorRunner.NotifyDisabled(this);
  }

  public void Reset(float toValue = 0F) {
    _value.reset = true;
    _value.Update(toValue);
  }

  public void UpdatePulsator(float deltaTime) {
    switch (_state) {
      case State.Warming:
        _targetValue = active;
        break;
      case State.Pulsing:
        _targetValue = pulse;
        break;
      case State.Resting:
        _targetValue = rest;
        break;
    }

    _value.Update(_targetValue, deltaTime);

    if (_state == State.Pulsing && Mathf.Abs(_value.value - pulse) < 0.001F) {
      _state = State.Warming;
    }
  }

  public Pulsator SetValues(float rest, float pulse, float active) {
    this.rest = rest;
    this.pulse = pulse;
    this.active = active;

    return this;
  }

  public Pulsator SetSpeed(float speed) {
    this.speed = speed;

    return this;
  }

  public void WarmUp() {
    _state = State.Warming;
  }

  public void Pulse() {
    _state = State.Pulsing;
  }

  public void Relax() {
    _state = State.Resting;
  }
}

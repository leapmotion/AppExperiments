using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation {

  public class Pulsator : IPoolable {

    /// <summary>
    /// The value the Pulsator moves to when initialized or when Relax() was
    /// called more recently than WarmUp().
    /// </summary>
    public float restValue   = 0F;

    /// <summary>
    /// The value the Pulsator moves to when Pulse() is called. A pulse
    /// naturally transitions back to being at rest or being warm once it
    /// reaches the pulse value, depending on whether WarmUp() or Relax() was
    /// called more recently.
    /// </summary>
    public float pulseValue  = 1.2F;

    /// <summary>
    /// The value the Pulsator moves to when WarmUp() was called more recently
    /// than Relax().
    /// </summary>
    public float warmValue = 1F;

    private float _speed = 1F;
    /// <summary>
    /// Unitless speed for the pulsator. Values scale almost-linearly.
    /// </summary>
    public float speed {
      get { return _speed; }
      set {
        float s = value;
        if (s <= 0F) s = 0.001F;
        _speed = s * 20F;
        _value.SetBlend(1 / (speed + 1.01F), 1F);
      }
    }
    
    /// <summary>
    /// Gets whether the pulsator is not warm and not currently pulsing, meaning
    /// it is current moving towards its rest value.
    /// Note that this value and isWarm are not direct opposites.
    /// </summary>
    public bool isResting { get { return !_warm && !_pulsing; } }

    private bool _warm = false;
    /// <summary>
    /// Whether the pulsator is targeting its warm value or _will_ target its
    /// warm value once its pulse is completed, if it is currently pulsing. (See
    /// isPulsing.)
    /// </summary>
    public bool isWarm { get { return _warm; } }

    private bool _pulsing = false;
    /// <summary>
    /// Whether the pulsator is currently pulsing, meaning it is moving towards
    /// its pulse value. Once it reaches its pulse value, the pulsator will
    /// either be "warm" or "resting" depending on whether WarmUp() or Relax()
    /// was called more recently on the pulsator.
    /// </summary>
    public bool isPulsing { get { return _pulsing; } }

    private SmoothedFloat _value;
    /// <summary>
    /// The current value of the Pulsator. Use this to drive properties to be
    /// pulsated. You can also force the pulsator to be reset to a new value by
    /// setting this property directly.
    /// </summary>
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
    /// Returns whether the pulsator is receiving updates from the Pulsator
    /// Runner. Disabling a pulsator pauses it by preventing it from receiving
    /// updates from the runner.
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

    private float _targetValue = 0F;
    public void UpdatePulsator(float deltaTime) {

      if (_pulsing) {
        _targetValue = pulseValue;
      }
      else if (_warm) {
        _targetValue = warmValue;
      }
      else {
        _targetValue = restValue;
      }

      _value.Update(_targetValue, deltaTime);

      if (_pulsing && Mathf.Abs(_value.value - pulseValue) < 0.001F) {
        _pulsing = false;
      }
    }

    public Pulsator SetValues(float rest, float pulse, float warm) {
      this.restValue = rest;
      this.pulseValue = pulse;
      this.warmValue = warm;

      return this;
    }

    public Pulsator SetSpeed(float speed) {
      this.speed = speed;

      return this;
    }

    public void WarmUp() {
      _warm = true;
    }

    public void Pulse() {
      _pulsing = true;
    }

    public void Relax() {
      _warm = false;
    }

    /// <summary>
    /// Cancels an active pulse early, returning to either the rest or warm
    /// state.
    /// </summary>
    public void CancelPulse() {
      _pulsing = false;
    }    
  }

}

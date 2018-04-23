using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Timers {

  public class TimerManager : SingletonBehaviour<TimerManager>, IInternalTimerManager {
    
    private HashSet<Timer> _timers;

    void Update() {
      foreach (var timer in _timers.Query().Where(timer => timer.isRunning)) {
        timer.time = timer.time
                   + (timer.type == TimerType.CountUp ?  Time.deltaTime
                                                      : -Time.deltaTime);
      }
    }

    void IInternalTimerManager.AddTimer(Timer timer) {
      _timers.Add(timer);
    }

    void IInternalTimerManager.RemoveTimer(Timer timer) {
      _timers.Remove(timer);
    }

  }

  interface IInternalTimerManager {
    void AddTimer(Timer timer);
    void RemoveTimer(Timer timer);
  }

  public enum TimerType {
    CountUp,
    CountDown
  }

  public class Timer : System.IDisposable {

    private TimerType _type = TimerType.CountUp;
    public TimerType type { get { return _type; } set { _type = value; } }

    private float _time = 0F;
    public float time { get { return _time; } set { _time = value; } }

    private float _resetTime = 0F;
    public float resetTime { get { return _resetTime; } set { _resetTime = value; } }

    private bool _isRunning = false;
    public bool isRunning { get { return _isRunning; } set { _isRunning = value; } }

    /// <summary>
    /// Constructs a new Timer.
    /// 
    /// Call Start() on the Timer to have it count time every Update().
    /// </summary>
    public Timer() {
      (TimerManager.instance as IInternalTimerManager).AddTimer(this);
    }

    /// <summary>
    /// Specifies a non-zero time to use when the timer is stopped or restarted.
    /// </summary>
    public Timer ResetTime(float resetTime) {
      _resetTime = resetTime;
      return this;
    }

    /// <summary>
    /// Sets the timer to count up or down depending on the argument TimerType.
    /// </summary>
    public Timer Type(TimerType type) {
      _type = type;
      return this;
    }

    /// <summary>
    /// Disposes of this timer by removing it from the TimerManager set of active timers.
    /// </summary>
    public void Dispose() {
      // Must reset ALL properties back to their defaults!
      _type = TimerType.CountUp;
      _time = 0F;
      _resetTime = 0F;
      _isRunning = false;

      (TimerManager.instance as IInternalTimerManager).RemoveTimer(this);
    }

    /// <summary>
    /// Starts the timer running, so its time gets incremented by deltaTime
    /// or fixedDeltaTime (depending on the Timer mode) when the TimerManager receives
    /// its Update or its FixedUpdate.
    /// </summary>
    public void Start() {
      _isRunning = true;
    }

    /// <summary>
    /// Sets the timer's time to zero and starts the timer if it isn't already running.
    /// </summary>
    public void Restart() {
      Stop();
      Start();
    }

    /// <summary>
    /// Stops the timer, so its time is no longer incremented. Does not reset the time
    /// on the timer to zero, unless the optional reset argument is passed
    /// <code>true</code>.
    /// </summary>
    public void Stop(bool reset = false) {
      _isRunning = false;
      if (reset) _time = resetTime;
    }

  }

}

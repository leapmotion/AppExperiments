using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  /// <summary>
  /// A quick template for creating a custom IHandle based on a MonoBehaviour. The only
  /// implementations required are ways to get and set the object's current Pose.
  /// 
  /// TransformHandle is a trivial implementation of HandleBase using a Transform as
  /// the Pose source.
  /// </summary>
  public abstract class zzOld_HandleBase : MovementObservingBehaviour, zzOld_IHandle {

    public override Pose GetPose() {
      return pose;
    }

    public abstract Pose pose {
      get;
      protected set;
    }

    #region Inspector

    [SerializeField]
    [Range(0f, 1f)]
    private float _rigidness = 0f;
    public float rigidness { get { return _rigidness; } }
    
    #endregion

    #region Unity Events

    protected override void OnEnable() {
      base.OnEnable();

      _lastPose = _targetPose = this.pose;
    }

    protected virtual void Update() {
      updateHeldState();
      updateMovedState();
      updateReleaseState();
      updateThrownState();
    }

    protected override void LateUpdate() {
      lateUpdateMoveToTarget();

      base.LateUpdate();
    }

    protected virtual void OnDestroy() {
      if (isHeld) {
        Release();
      }
    }

    #endregion

    #region Pivot Point

    public virtual Vector3 localPivot { get { return Vector3.zero; } }

    #endregion

    #region Target Pose

    private Pose _targetPose = Pose.identity;
    public Pose targetPose {
      get { return _targetPose; }
      set { _targetPose = value; }
    }

    protected virtual void lateUpdateMoveToTarget() {
      var smoothedPose = PhysicalInterfaceUtils.SmoothMove(prevPose,
                                                           this.pose,
                                                           targetPose,
                                                           rigidness);
      this.pose = smoothedPose;
    }

    #endregion

    #region Held State

    private bool _isHeld = false;
    public bool isHeld {
      get { return _isHeld; }
    }

    private bool _wasHeld = false;
    private bool _sawWasHeld = false;
    public bool wasHeld {
      get { return _wasHeld && _sawWasHeld; }
    }

    public void Hold() {
      _isHeld = true;
      
      _wasHeld = true;
    }

    private void updateHeldState() {
      if (_wasHeld && !_sawWasHeld) {
        _sawWasHeld = true;
      }
      else if (_wasHeld && _sawWasHeld) {
        _wasHeld = false;
        _sawWasHeld = false;
      }
    }

    #endregion

    #region Moved State

    private bool _wasMoved = false;
    public bool wasMoved {
      get {
        return _wasMoved;
      }
    }

    private Pose _lastPose = Pose.identity;
    private void updateMovedState() {
      if (_wasMoved) {
        _wasMoved = false;
      }
      if (!this.pose.ApproxEquals(_lastPose)) {
        _wasMoved = true;

        _lastPose = this.pose;
      }
    }

    #endregion

    #region Released State

    private bool _wasReleased = false;
    private bool _sawWasReleased = false;
    public bool wasReleased {
      get { return _wasReleased && _sawWasReleased; }
    }

    public virtual void Release() {
      _isHeld = false;
      _wasReleased = true;

      if (movement.velocity.sqrMagnitude > PhysicalInterfaceUtils.MIN_THROW_SPEED_SQR) {
        _wasThrown = true;
      }
    }

    private void updateReleaseState() {
      if (_wasReleased && !_sawWasReleased) {
        _sawWasReleased = true;
      }
      else if (_wasReleased && _sawWasReleased) {
        _wasReleased = false;
        _sawWasReleased = false;
      }
    }

    #endregion

    #region Thrown State

    private bool _wasThrown = false;
    private bool _sawWasThrown = true;
    public bool wasThrown {
      get {
        return _wasThrown && _sawWasThrown;
      }
    }

    private void updateThrownState() {
      if (_wasThrown && !_sawWasThrown) {
        _sawWasThrown = true;
      }
      else if (_wasThrown && _sawWasThrown) {
        _wasThrown = false;
        _sawWasThrown = false;
      }
    }

    #endregion

  }

}

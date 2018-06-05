using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelStateController : MonoBehaviour {

  public InteractionBehaviour widget;
  public PanelTransitionController transitionController;

  [Header("Auto")]
  public State state = State.Open;

  private float _grabIdleDuration = 0F;
  private Vector3 _lastGrabPosition;
  private float _grabIdleThreshold = 0.01F;
  private float _outlineHintDelay = 0.5F;

  void Start() {
    widget.OnGraspBegin += onGraspBegin;
  }

  void Update() {
    if (widget.isGrasped && state == State.Open) {
      transitionToClosed();
    }

    if (widget.isGrasped) {
      Vector3 grabPosition = widget.rigidbody.position;

      _grabIdleDuration += Time.deltaTime;

      float displacement = (grabPosition - _lastGrabPosition).magnitude;
      if (displacement > _grabIdleThreshold) {
        _grabIdleDuration = 0F;
      }

      if (_grabIdleDuration > _outlineHintDelay && state == State.Closed) {
        transitionToOutline();
      }
      else if (_grabIdleDuration < _outlineHintDelay && state == State.Outline) {
        transitionToClosed();
      }

      _lastGrabPosition = grabPosition;
    }

    if (!widget.isGrasped) {
      transitionToOpen();
    }
  }

  private void onGraspBegin() {
    _grabIdleDuration = 0F;
    _lastGrabPosition = widget.rigidbody.position;
  }

  private void transitionToClosed() {
    if (transitionController.UpdateTransitionToClosed()) {
      state = State.Closed;
    }
  }

  private void transitionToOutline() {
    if (transitionController.UpdateTransitionToOutline()) {
      state = State.Outline;
    }
  }

  private void transitionToOpen() {
    if (transitionController.UpdateTransitionToOpen()) {
      state = State.Open;
    }
  }

  public enum State {
    Open,
    Outline,
    Closed
  }

}

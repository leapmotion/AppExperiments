using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelTransitionController : MonoBehaviour {

  public RectTransform panel;
  public RectTransform outline;

  public float speed = 4f;

  [Header("Animation Curves")]
  public AnimationCurve panelWeightToXScale;
  public AnimationCurve panelWeightToYScale;
  public AnimationCurve panelWeightToZScale;

  private Vector3 _baseLocalScale = Vector3.one;

  private float _curPanelWeight    = 1f;
  private float _curOutlineWeight  = 0f;

  private float _targetPanelWeight   = 1f;
  private float _targetOutlineWeight = 0f;

  public bool isTransitionFinished {
    get {
      return _curPanelWeight == _targetPanelWeight
          && _curOutlineWeight == _targetOutlineWeight;
    }
  }

  protected virtual void Reset() {
    var panelStateController = GetComponent<PanelStateController>();
    if (panelStateController != null) {
      if (panelStateController.transitionController == null) {
        panelStateController.transitionController = this;
      }
    }
  }

  protected virtual void Start() {
    _baseLocalScale = panel.transform.localScale;
  }

  protected virtual void Update() {
    // Linearly move current to target weights, obeying constraints along the way.

    if (_curOutlineWeight < 0.80f) {
      if (_curPanelWeight > _targetPanelWeight) {
        _curPanelWeight -= speed * Time.deltaTime;
        if (_curPanelWeight < _targetPanelWeight) _curPanelWeight = _targetPanelWeight;
      }
      else if (_curPanelWeight < _targetPanelWeight) {
        _curPanelWeight += speed * Time.deltaTime;
        if (_curPanelWeight > _targetPanelWeight) _curPanelWeight = _targetPanelWeight;
      }
    }

    if (_curPanelWeight < 0.80f) {
      if (_curOutlineWeight > _targetOutlineWeight) {
        _curOutlineWeight -= speed * Time.deltaTime;
        if (_curOutlineWeight < _targetOutlineWeight) _curOutlineWeight = _targetOutlineWeight;
      }
      else if (_curOutlineWeight < _targetOutlineWeight) {
        _curOutlineWeight += speed * Time.deltaTime;
        if (_curOutlineWeight > _targetOutlineWeight) _curOutlineWeight = _targetOutlineWeight;
      }
    }

    updateAnimationState();
  }

  private void updateAnimationState() {
    panel.transform.localScale = new Vector3(_baseLocalScale.x * panelWeightToXScale.Evaluate(_curPanelWeight),
                                             _baseLocalScale.y * panelWeightToYScale.Evaluate(_curPanelWeight),
                                             _baseLocalScale.z * panelWeightToZScale.Evaluate(_curPanelWeight));

    outline.transform.localScale = new Vector3(_baseLocalScale.x * panelWeightToXScale.Evaluate(_curOutlineWeight),
                                               _baseLocalScale.y * panelWeightToYScale.Evaluate(_curOutlineWeight),
                                               _baseLocalScale.z * panelWeightToZScale.Evaluate(_curOutlineWeight));
  }

  private void updateActiveState() {
    bool targetPanelState = true;
    bool targetOutlineState = false;

    if (_curOutlineWeight > 0f) {
      targetOutlineState = true;
    }

    if (_curPanelWeight > 0f) {
      targetPanelState = true;
    }

    if (_curOutlineWeight < 0.01f
        && _curPanelWeight < 0.01f) {
      targetPanelState = false;
      targetOutlineState = false;
    }

    panel.gameObject.SetActive(targetPanelState);
    outline.gameObject.SetActive(targetOutlineState);
  }

  public bool UpdateTransitionToOpen() {
    _targetPanelWeight   = 1f;
    _targetOutlineWeight = 0f;

    updateActiveState();

    return isTransitionFinished;
  }

  public bool UpdateTransitionToClosed() {
    _targetPanelWeight   = 0f;
    _targetOutlineWeight = 0f;

    updateActiveState();

    return isTransitionFinished;
  }

  public bool UpdateTransitionToOutline() {
    _targetPanelWeight   = 0f;
    _targetOutlineWeight = 1f;

    updateActiveState();

    return isTransitionFinished;
  }

}

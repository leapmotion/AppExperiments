using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabBarController : MonoBehaviour {

  public RadioToggleGroup tabGroup;

  [SerializeField]
  private Vector3 _localOffsetFromTab = Vector3.zero;
  private Vector3 _targetLocalPos = Vector3.zero;

  void Awake() {
    if (Application.isPlaying) {
      tabGroup.OnIndexToggled += onIndexToggled;
    }
  }

  void Start() {
    _targetLocalPos = tabGroup.activeToggle.RelaxedLocalPosition + _localOffsetFromTab;
  }

  private void onIndexToggled(int idx) {
    _targetLocalPos = tabGroup.activeToggle.RelaxedLocalPosition + _localOffsetFromTab;
  }

  void Update() {
    _targetLocalPos = tabGroup.activeToggle.RelaxedLocalPosition + _localOffsetFromTab;
    this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, _targetLocalPos, 20F * Time.deltaTime);
  }

  public void SetLocalOffsetToCurrentPosition() {
    _localOffsetFromTab = this.transform.localPosition - tabGroup.activeToggle.RelaxedLocalPosition;
  }

  public void SetLocalPositionToLocalOffset() {
    this.transform.localPosition = tabGroup.activeToggle.RelaxedLocalPosition + _localOffsetFromTab;
  }

}

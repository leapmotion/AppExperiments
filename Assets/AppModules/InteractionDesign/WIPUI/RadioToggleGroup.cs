using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioToggleGroup : MonoBehaviour {

  [EditTimeOnly]
  public List<InteractionToggle> toggles;
  
  private int _activeToggleIdx = 0;
  public int activeToggleIdx { get { return _activeToggleIdx; } }
  public InteractionToggle activeToggle { get { return toggles[activeToggleIdx]; } }

  /// <summary>
  /// Gets whether any of the toggles in the RadioToggleGroup are enabled, or sets
  /// the controlEnabled state for all of the toggles in the group simultaneously.
  /// </summary>
  public bool controlsEnabled {
    get {
      return toggles.Query().Any(t => t.controlEnabled);
    }
    set {
      foreach (var toggle in toggles) {
        toggle.controlEnabled = value;
      }
    }
  }

  public void UntoggleAll() {
    foreach (var toggle in toggles) {
      toggle.Untoggle();
    }
  }

  public Action<int> OnIndexToggled = (idx) => { };

  void Awake() {
    for (int i = 0; i < toggles.Count; i++) {
      var toggle = toggles[i];

      int toggleIndex = i;
      toggle.OnToggle += () => {
        toggle.controlEnabled = false;
        onIndexToggled(toggleIndex);
      };

      for (int j = 0; j < toggles.Count; j++) {
        if (j == i) continue;

        var otherToggle = toggles[j];
        toggle.OnToggle += () => {
          otherToggle.controlEnabled = true;
          otherToggle.isToggled = false;
        };
      }
    }
  }

  private bool _sawWasToggled = false;
  private bool _wasToggled = false;

  public bool wasToggled {
    get { return _wasToggled && _sawWasToggled; }
  }

  private void Update() {
    // This guarantees that "wasToggled" will return true for a full Update cycle of
    // the RadioToggleGroup, regardless of when or how many onIndexToggled callbacks
    // there are. (It may also introduce a frame of latency, but that's fine.)
    if (_wasToggled && !_sawWasToggled) {
      _sawWasToggled = true;
    }
    else if (_sawWasToggled) {
      _wasToggled = false;
      _sawWasToggled = false;
    }
  }

  private void onIndexToggled(int idx) {
    _activeToggleIdx = idx;
    _wasToggled = true;

    OnIndexToggled(idx);
  }

}

using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.UI {

  public abstract class UIButton : MonoBehaviour {

    [Header("UI Button")]

    public InteractionButton button;

    private Action onPressAction;
    private Action onUnpressAction;

    protected virtual void Reset() {
      initialize();
    }

    protected virtual void OnValidate() {
      initialize();
    }

    protected virtual void Awake() {
      initialize();
    }

    /// <summary>
    /// Called on Reset(), OnValidate(), and Awake(). Use this for common runtime
    /// and edit-time state initialization, such as event subscription and
    /// last-chance component searches.
    /// </summary>
    protected virtual void initialize() {
      if (button == null) {
        button = GetComponent<InteractionButton>();
      }

      onPressAction   = OnPress;
      onUnpressAction = OnUnpress;
    }

    protected virtual void OnEnable() {
      if (button != null) {
        button.OnPress   -= onPressAction;
        button.OnUnpress -= onUnpressAction;
        button.OnPress   += onPressAction;
        button.OnUnpress += onUnpressAction;
      }
    }

    protected virtual void OnDisable() {
      if (button != null) {
        button.OnPress -= onPressAction;
        button.OnUnpress -= onUnpressAction;
      }
    }

    public virtual void OnPress() { }

    public virtual void OnUnpress() { }

  }

}

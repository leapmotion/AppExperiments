using Leap.Unity.Animation;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class Tool_ModeActivator : MonoBehaviour {

    [Tooltip("If true, when this behaviour receives Start(), it will set the mode string "
           + "at its channel to the state specified by the activeMode field.")]
    public bool setModeOnStart = false;

    public string activeMode = "paint";

    [Header("Ucon Mode Channel In")]
    public StringChannel modeChannel = new StringChannel("tool");

    private void Start() {
      Updater.instance.OnUpdate += onUpdaterUpdate;

      if (setModeOnStart) {
        modeChannel.Set(activeMode);
      }
    }

    /// <summary>
    /// The use of an Updater callback here allows this MonoBehaviour to control the
    /// "active" or "inactive" state of its GameObject dynamically -- it will receive
    /// onUpdaterUpdate even if its GameObject is disabled.
    /// 
    /// This script controls its own GameObject's active state, which means it's
    /// non-obvious how to disable _the script itself_ from actively doing this.
    /// Actually, it's easy, because Unity has script activation states too. Just
    /// disable the component to prevent it from doing any switching activity.
    /// </summary>
    private void onUpdaterUpdate() {
      if (!this.enabled) return;

      var shouldBeActive = activeMode.Equals(modeChannel.Get());

      if ((shouldBeActive && !gameObject.activeSelf)
          || (!shouldBeActive && gameObject.activeSelf)) {
        gameObject.SetActive(shouldBeActive);
      }
    }

  }

}

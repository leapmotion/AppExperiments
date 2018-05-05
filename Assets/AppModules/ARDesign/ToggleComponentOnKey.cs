using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class ToggleComponentOnKey : MonoBehaviour {

    public MonoBehaviour componentToToggle;

    public bool startEnabled = true;

    public KeyCode toggleKey = KeyCode.None;

    private void Start() {
      if (componentToToggle != null) {
        componentToToggle.enabled = startEnabled;
      }
    }

    private void Update() {
      if (Input.GetKeyDown(toggleKey) && componentToToggle != null) {
        componentToToggle.enabled = !componentToToggle.enabled;
      }
    }

  }

}

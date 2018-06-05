using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  public class DevPositionCommandGestureTrigger : MonoBehaviour {


    [ImplementsInterface(typeof(IPoseGesture))]
    public MonoBehaviour _gesture;
    public IPoseGesture gesture {
      get { return _gesture as IPoseGesture; }
      set { _gesture = value as MonoBehaviour; }
    }

    public string devCommandName = "Spawn Something";

    void Reset() {
      if (gesture == null) {
        gesture = GetComponent<TwoHandedHeldGesture>();
      }
    }

    void Update() {
      if (gesture.wasFinished) {
        DevCommand.Invoke(devCommandName, gesture.pose.position);
      }
    }

  }

}

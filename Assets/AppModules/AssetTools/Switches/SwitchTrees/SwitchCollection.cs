using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation {

  [System.Serializable]
  public struct SwitchCollection {

    [SerializeField]
    private MonoBehaviour[] _switches;
    public ImplementingBehaviours<IPropertySwitch> switches {
      get {
        return ImplementingBehaviours<IPropertySwitch>.FromMonoBehaviours(_switches);
      }
    }

  }

}
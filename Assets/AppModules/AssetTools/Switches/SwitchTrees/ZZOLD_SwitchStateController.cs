using Leap.Unity.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Leap.Unity.Animation {

  public class ZZOLD_SwitchStateController : MonoBehaviour {

    #region Inspector

    [System.Serializable]
    public class StateDictionary : SerializableDictionary<string, SwitchState> {
      public override float KeyDisplayRatio() {
        return 0.2f;
      }
    }

    [System.Serializable]
    public struct SwitchState {

      [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
      private MonoBehaviour _switch;
      public IPropertySwitch propertySwitch {
        get { return _switch as IPropertySwitch; }
      }

      public UnityEvent onSwitchedOn;
    }

    public StateDictionary states;

    [SerializeField, OnEditorChange("curState")]
    private string _curState = "";
    public string curState {
      get { return _curState; }
      set {
        if (!curState.Equals(value)) {
          SwitchState newStateSwitch;
          if (states.TryGetValue(value, out newStateSwitch)) {

            var oldStateSwitch = states[curState];
            if (oldStateSwitch.propertySwitch != null) {
              if (Application.isPlaying) {
                oldStateSwitch.propertySwitch.Off();
              }
              else {
                oldStateSwitch.propertySwitch.OffNow();
              }
            }
            
            _curState = value;

            if (newStateSwitch.propertySwitch != null) {
              if (Application.isPlaying) {
                newStateSwitch.propertySwitch.On();
              }
              else {
                newStateSwitch.propertySwitch.OnNow();
              }
            }
            if (newStateSwitch.onSwitchedOn != null) {
              newStateSwitch.onSwitchedOn.Invoke();
            }
          }
        }
      }
    }

    #endregion

    #region Unity Events

    void Start() {
      SwitchState curStateSwitch;
      if (states.TryGetValue(curState, out curStateSwitch)) {
        if (curStateSwitch.propertySwitch != null) {
          curStateSwitch.propertySwitch.OnNow();
        }
      }
    }

    #endregion

    #region Public API

    public void SetState(string state) {
      curState = state;
    }

    #endregion

  }

}

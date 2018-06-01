using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class DirectionSwitchDriver : MonoBehaviour {

    [SerializeField]
    [ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _propertySwitch;
    public IPropertySwitch propertySwitch {
      get { return _propertySwitch as IPropertySwitch; }
    }

    public Transform facingTarget;
    public float activationAngle = 80f;
    //public float deactivationAngle = 90f;

    private void Update() {
      if (facingTarget != null && propertySwitch != null) {
        var shouldActivate = transform.forward.IsFacing(transform.position,
                                                        facingTarget.position,
                                                        activationAngle);
        //var shouldDeactivate = transform.forward.IsFacing(transform.position,
        //                                                facingTarget.position,
        //                                                deactivationAngle);

        var onOrTurningOn = propertySwitch.GetIsOnOrTurningOn();
        var offOrTurningOff = propertySwitch.GetIsOffOrTurningOff();
        if (shouldActivate && !onOrTurningOn) {
          propertySwitch.On();
        } else if (!shouldActivate && !offOrTurningOff) {
          propertySwitch.Off();
        }
      }
    }

  }

}

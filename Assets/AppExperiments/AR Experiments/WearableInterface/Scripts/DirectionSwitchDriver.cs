using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.AR.Testing {

  public class DirectionSwitchDriver : MonoBehaviour {

    [SerializeField]
    [ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _propertySwitch = default;
    public IPropertySwitch propertySwitch {
      get { return _propertySwitch as IPropertySwitch; }
    }

    public Transform facingTarget;
    public float activationAngle = 80f;
    public float deactivationAngle = 81f;

    private void Update() {
      if (facingTarget != null && propertySwitch != null) {
        var shouldActivate = transform.forward.IsFacing(transform.position,
                                                        facingTarget.position,
                                                        activationAngle);
        var shouldDeactivate = !transform.forward.IsFacing(transform.position,
                                                           facingTarget.position,
                                                           deactivationAngle);

        var onOrTurningOn = propertySwitch.GetIsOnOrTurningOn();
        var offOrTurningOff = propertySwitch.GetIsOffOrTurningOff();
        if (shouldActivate && !onOrTurningOn) {
          propertySwitch.On();
        } else if (shouldDeactivate && !offOrTurningOff) {
          propertySwitch.Off();
        }
      }
    }

  }

}

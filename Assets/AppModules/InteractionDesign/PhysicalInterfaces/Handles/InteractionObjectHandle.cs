using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class InteractionObjectHandle : MovementObservingBehaviour,
                                         IHandle {

    [SerializeField]
    [EditTimeOnly]
    private IntObj _intObj;
    public IntObj intObj {
      get { return _intObj; }
      protected set { _intObj = value; }
    }

    void Reset() {
      if (intObj == null) intObj = GetComponent<IntObj>();
    }

    protected override void OnEnable() {
      base.OnEnable();

      if (intObj == null) intObj = GetComponent<IntObj>();

      if (intObj != null) {
        intObj.OnGraspBegin += onGraspBegin;
        intObj.OnGraspEnd   += onGraspEnd;
      }
    }

    public override Pose GetPose() {
      return this.transform.ToPose();
    }

    public bool isHeld { get { return intObj != null && intObj.isGrasped; } }

    public bool isKinematic {
      get { return intObj != null && intObj.rigidbody.isKinematic; }
      set { if (intObj != null) intObj.rigidbody.isKinematic = value; }
    }

    public event Action OnHoldBegin = () => { };
    public event Action<IHandle> OnHandleHoldBegin = (thisHandle) => { };

    public event Action OnHoldEnd = () => { };
    public event Action<IHandle> OnHandleHoldEnd = (thisHandle) => { };

    private void onGraspBegin() {
      OnHoldBegin();
      OnHandleHoldBegin(this);
    }

    private void onGraspEnd() {
      OnHoldEnd();
      OnHandleHoldEnd(this);
    }

  }

}

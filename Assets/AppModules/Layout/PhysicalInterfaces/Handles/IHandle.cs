using System;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {

    bool isHeld { get; }

    bool isMoving { get; }

    bool isKinematic { get; set; }


    event Action OnHoldBegin;
    event Action<IHandle> OnHandleHoldBegin;

    event Action OnHoldEnd;
    event Action<IHandle> OnHandleHoldEnd;
    
  }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IKinematicStateProvider {

    KinematicState GetKinematicState();

  }

}

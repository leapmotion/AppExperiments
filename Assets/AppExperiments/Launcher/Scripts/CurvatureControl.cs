using Leap.Unity.Space;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Launcher {

  public class CurvatureControl : MonoBehaviour {

    public LeapSphericalSpace leapSphericalSpace;

    public void SetRadius(float radius) {
      radius = Mathf.Clamp(radius, 0.2f, 1f);

      leapSphericalSpace.radius = radius;
    }

  }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  public interface ILocalPositionProvider {

    Vector3 GetLocalPosition(Transform transform);

  }

}

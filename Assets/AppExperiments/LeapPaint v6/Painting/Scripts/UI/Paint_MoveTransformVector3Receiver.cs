using Leap.Unity.Attributes;
using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Paint_MoveTransformVector3Receiver : MonoBehaviour,
                                                    IStreamReceiver<Vector3> {

    [Tooltip("The transform that should be moved by delta positions received by the stream.")]
    public Transform toMove;

    [Tooltip("Coefficient for movement speed, in meters of input vector length per output "
           + "speed in meters per second.")]
    public float sensitivity = 1f;

    public void Close() { }

    public void Open() { }

    public void Receive(Vector3 deltaPosition) {
      toMove.position += deltaPosition * sensitivity * Time.deltaTime;
    }

  }

}

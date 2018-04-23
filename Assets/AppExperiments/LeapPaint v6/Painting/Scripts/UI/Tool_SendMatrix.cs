using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Tool_SendMatrix : MonoBehaviour {

    [Header("Ucon Matrix Channel Output")]
    public MatrixChannel worldFrameChannel = new MatrixChannel("tool/frame");

    private void Update() {
      worldFrameChannel.Set(this.transform.worldToLocalMatrix);
    }


  }

}

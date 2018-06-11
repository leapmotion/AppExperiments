using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.AR.Testing {

  public class PoseBangReceiver_SetToTrigger : MonoBehaviour,
                                               IStreamReceiver<Pose> {

    public Collider toSetToTrigger;

    private void Reset() {
      if (toSetToTrigger == null) toSetToTrigger = GetComponent<Collider>();
    }
    private void OnValidate() {
      if (toSetToTrigger == null) toSetToTrigger = GetComponent<Collider>();
    }
    
    public void Close() {

    }

    public void Open() {

    }

    public void Receive(Pose data) {
      toSetToTrigger.isTrigger = true;
    }

  }

}

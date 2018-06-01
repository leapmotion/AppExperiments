using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class ChildSequenceProvider : MonoBehaviour, IGameObjectSequenceProvider {

    public GameObject this[int idx] {
      get { return this.transform.GetChild(idx).gameObject; }
    }

    public int Count {
      get {
        return this.transform.childCount;
      }
    }

  }

}
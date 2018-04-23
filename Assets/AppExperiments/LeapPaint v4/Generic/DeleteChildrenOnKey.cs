using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteChildrenOnKey : MonoBehaviour {

  public string key = "c";

  void Update() {
    if (Input.GetKeyDown(key)) {
      foreach (var child in this.transform.GetChildren()) {
        Destroy(child.gameObject);
      }
    }
  }

}

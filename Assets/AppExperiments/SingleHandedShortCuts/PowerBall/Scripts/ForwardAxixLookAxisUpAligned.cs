using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardAxixLookAxisUpAligned : MonoBehaviour {
  public Transform Target;//Camera
  public bool UseCameraAsTarget = true;
  void Awake() {
    if (UseCameraAsTarget) Target = Camera.main.transform;
  }
  void LateUpdate() {
    
    transform.eulerAngles = new Vector3(0f, 0f, 0f);
    align();
  }
  private void align() {
    Vector3 targetPostition = new Vector3(Target.position.x,
                                           this.transform.position.y,
                                           Target.position.z);
    this.transform.LookAt(targetPostition);
  }
}

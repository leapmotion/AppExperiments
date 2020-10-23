using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimConstraint : MonoBehaviour {
  public Transform Target;
  public bool UseCameraAsTarget = true;
	// Use this for initialization
	void Awake () {
    if (UseCameraAsTarget) Target = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {
    transform.rotation = Quaternion.LookRotation(transform.position - Target.position);
  }
}

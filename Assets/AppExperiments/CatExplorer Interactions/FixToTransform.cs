using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixToTransform : MonoBehaviour {

	public Transform objectToFixTo;
	private Vector3 initialPos;
	// Use this for initialization
	void Start () {
		initialPos = transform.localPosition;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(objectToFixTo.gameObject.activeInHierarchy){
			transform.localPosition = objectToFixTo.localPosition;
			// transform.localRotation = objectToFixTo.localRotation;
		} else {
			transform.localPosition = initialPos;
		}
	}
}

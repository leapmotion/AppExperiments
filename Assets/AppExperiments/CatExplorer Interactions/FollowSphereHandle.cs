using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphereHandle : MonoBehaviour {
	
	public SphereHandle frontSphereHandle;
	public SphereHandle backSphereHandle;	
	
	[HideInInspector]
	public Transform targetTransform;
	
	public float damping = 0.2f;
	
	public GameObject Lines;
	
	private Transform Head;
	private bool leftSideActive = true;
	
	[HideInInspector]
	public bool moving = false;

	[HideInInspector]
	public static bool pinched = false;

	private void Start() {
		Head = GameObject.Find("Main Camera").transform;
		backSphereHandle.gameObject.SetActive(false);
		targetTransform = frontSphereHandle.transform;
		HideLines();
	}

	void Update () {
		ManageSides();

		Vector3 target = transform.localPosition;
		
		if(frontSphereHandle.isPinched) target = frontSphereHandle.transform.localPosition;
		if(backSphereHandle.isPinched) 	target = backSphereHandle.transform.localPosition;

		moving = target != transform.localPosition;

		if(frontSphereHandle.isPinched || backSphereHandle.isPinched){
			target.y = transform.localPosition.y;
			target.z = transform.localPosition.z;
			transform.localPosition += (target - transform.localPosition) * damping * Time.deltaTime * 100;
			ShowLines();
		} else HideLines();
	}

	void ShowLines(){
		if(!Lines.activeInHierarchy) Lines.SetActive(true);
	}

	void HideLines(){
		if(Lines.activeInHierarchy) Lines.SetActive(false);
	}

	void ManageSides(){
		
		float dot = Vector3.Dot(Head.position - transform.position, transform.forward);

		if(dot >= 0 && leftSideActive && !frontSphereHandle.isPinched){
			frontSphereHandle.gameObject.SetActive(false);
			backSphereHandle.gameObject.SetActive(true);
			targetTransform = frontSphereHandle.transform;
			leftSideActive = false;
		} else if(dot < 0 && !leftSideActive && !backSphereHandle.isPinched){
			frontSphereHandle.gameObject.SetActive(true);
			backSphereHandle.gameObject.SetActive(false);
			targetTransform = backSphereHandle.transform;
			leftSideActive = true;
		}

	}
}

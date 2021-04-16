using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionPlaneManager : MonoBehaviour {

	public Transform planeLeft, planeRight;
	public float thickness = 0.1f;

	// public List<GameObject> RightSide, LeftSide;

	private FollowSphereHandle sectionLeft, sectionRight;

	// Use this for initialization
	void Start () {
		sectionLeft = planeLeft.GetComponent<FollowSphereHandle>();
		sectionRight = planeRight.GetComponent<FollowSphereHandle>();
		
	}
	
	private void LateUpdate() {
		if(planeLeft.localPosition.x > planeRight.localPosition.x - thickness){
			if(sectionLeft.moving) planeRight.localPosition = planeLeft.localPosition + Vector3.right * thickness;
			else if(sectionRight.moving) planeLeft.localPosition = planeRight.localPosition - Vector3.right * thickness;
		}
	}
}

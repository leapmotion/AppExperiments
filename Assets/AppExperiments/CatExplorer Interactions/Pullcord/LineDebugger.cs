using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineDebugger : MonoBehaviour {

	public Transform pointA, pointB;
	public Vector3 shift = new Vector3(0,0,0);

	public float bothEndsOffset = 0;
	public float beginningOffset = 0;
	public float endOffset = 0;
	
	public bool rope = false;
	public int chains = 10;

	private float stiffness = 0.9f;
  	private float damping = 0.7f;

	private Vector3 prevPosA, prevPosB;

	[HideInInspector]
	public LineRenderer line;
	
	void Start () {
	line = GetComponent<LineRenderer>();

	prevPosA = pointA.position;
	prevPosB = pointB.position;
	
	if(rope){
		line.positionCount = chains + 2;
		
		line.SetPosition(0, pointA.position);
		line.SetPosition(line.positionCount-1, pointB.position);

		float incrementMag = Vector3.Distance(pointA.position, pointB.position) / (chains+1);

		for(int i = 0; i<chains; ++i){

			Vector3 chainPos = pointA.position + (pointB.position - pointA.position).normalized * (i+1) * incrementMag;
			
			line.SetPosition(i+1, chainPos);
		}
	}
	}

	void LateUpdate () {
		if(pointA!=null && pointB!=null && (prevPosA != pointA.position || prevPosB != pointB.position)){
			Vector3 atob = (pointB.position - pointA.position);
			
			Vector3 a = -atob.normalized * bothEndsOffset;
			Vector3 b = -atob.normalized * beginningOffset;
			Vector3 c = -atob.normalized * endOffset;

			
			line.SetPosition(0, pointA.position + shift + a + b);
			line.SetPosition(line.positionCount-1, pointB.position + shift - a - c);
		
			if(rope){

				for(int i = 0; i<chains; ++i){
					Vector3 prevPos = line.GetPosition(line.positionCount-1 - i);
					Vector3 thisPos = line.GetPosition(line.positionCount-1 - i - 1);
					Vector3 nextPos = line.GetPosition(line.positionCount-1 - i - 2);
					
					Vector3 acc = (prevPos - thisPos) * stiffness + (nextPos - thisPos) * stiffness;
					Vector3 chainPos = thisPos + acc;
					
					line.SetPosition(line.positionCount-1 - i -1, chainPos);
				}
			}
		
		}

		prevPosA = pointA.position;
		prevPosB = pointB.position;
	}
}

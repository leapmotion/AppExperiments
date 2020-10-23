using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTrailRenderer : MonoBehaviour {

	// Use this for initialization
	void Start () {

        GetComponent<TrailRenderer>().Clear();


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

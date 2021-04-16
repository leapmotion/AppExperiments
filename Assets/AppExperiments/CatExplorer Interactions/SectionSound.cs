using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionSound : MonoBehaviour {
	
	private AudioSource audioSource;	
	public AudioClip moveSound;

	private float deltaX = 0;
	private float prevX;

	private bool grasped = false;
	private bool isOn = false;

	void Start () {
		audioSource = GetComponent<AudioSource>();
		Invoke("SwitchOn", 4);
	}
	
	// Update is called once per frame
	void Update () {
		float d = transform.localPosition.x - prevX;
		deltaX += d;

		if(Mathf.Abs(deltaX) > 0.02f){
			deltaX = 0;
			if(isOn) audioSource.PlayOneShot(moveSound);

		}

		prevX = transform.localPosition.x;

	}

	void SwitchOn(){
		isOn = true;
	}
}

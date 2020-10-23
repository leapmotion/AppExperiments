using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionTester : MonoBehaviour {
  public TextMesh text;
  public int Option;

	// Use this for initialization
	void Start () {
		
	}
	
  public void CurrentOption(int option) {
    Debug.Log("Firing");
    text.text = option.ToString();
  }
}

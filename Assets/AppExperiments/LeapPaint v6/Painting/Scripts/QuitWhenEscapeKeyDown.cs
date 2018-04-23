using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitWhenEscapeKeyDown : MonoBehaviour {
	
	void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
  }

}

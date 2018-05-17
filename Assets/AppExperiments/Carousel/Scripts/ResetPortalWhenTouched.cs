using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class ResetPortalWhenTouched : MonoBehaviour {

  // Use this for initialization
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (GetComponent<InteractionBehaviour>().contactingControllers.Count > 0) {
      foreach (var card in FindObjectsOfType<PortalCard>()) {
        card.reset = true;
      }
    }
  }
}

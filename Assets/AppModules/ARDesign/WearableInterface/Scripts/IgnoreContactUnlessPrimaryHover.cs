using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Interaction {

  [AddComponentMenu("Leap Motion/Interaction Engine/Utilities/Ignore Contact Unless Primary Hover")]
  public class IgnoreContactUnlessPrimaryHover : MonoBehaviour {

    public InteractionBehaviour intObj;

    private void Reset() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }
    private void OnValidate() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }
    private void Start() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }

    private void Update() {
      if (intObj.isPrimaryHovered) {
        intObj.ignoreContact = false;
      }
      else {
        intObj.ignoreContact = true;
      }
    }

  }

}

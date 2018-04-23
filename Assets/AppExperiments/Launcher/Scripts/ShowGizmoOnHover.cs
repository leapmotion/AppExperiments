using Leap.Unity.Interaction;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Launcher {

  public class ShowGizmoOnHover : MonoBehaviour {

    public InteractionBehaviour intObj;
    public RuntimeColliderGizmos colliderGizmos;

    public enum HoverMode { Hover, PrimaryHover }
    public HoverMode hoverMode = HoverMode.PrimaryHover;

    [Header("Proximity")]
    public float minimumHoverDistance = 0.10f;

    void Reset() {
      if (colliderGizmos == null) colliderGizmos = GetComponent<RuntimeColliderGizmos>();
      if (intObj == null) intObj = GetComponentInParent<InteractionBehaviour>();
    }

    void Update() {

      bool isHovered = false;
      if (hoverMode == HoverMode.Hover) {
        isHovered = intObj.isHovered;
      }
      else if (hoverMode == HoverMode.PrimaryHover) {
        isHovered = intObj.isPrimaryHovered;
      }

      if (isHovered && intObj.closestHoveringControllerDistance <= minimumHoverDistance) {
        colliderGizmos.enabled = true;
      }
      else {
        colliderGizmos.enabled = false;
      }

    }

  }

}
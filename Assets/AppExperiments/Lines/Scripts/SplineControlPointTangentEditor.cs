using Leap.Unity.Splines;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Lines {

  [RequireComponent(typeof(InteractionBehaviour))]
  public class SplineControlPointTangentEditor : MonoBehaviour {

    public const float TANGENT_ZERO_SNAP_MAGNITUDE = 0.01F;

    public SplineControlPointEditor controlPointEditor;
    public bool isForwardTangent;

    private InteractionBehaviour _intObj;

    void Awake() {
      _intObj = GetComponent<InteractionBehaviour>();
      if (_intObj != null) {
        _intObj.OnGraspStay += onGraspStay;
        _intObj.OnGraspEnd  += onGraspEnd;
      }
    }

    void Start() {
      checkZeroTangent();
    }

    private void onGraspStay() {
      Vector3 tangent = (this.transform.position - controlPointEditor.transform.position)
                         * (isForwardTangent ? 1F : -1F);
      controlPointEditor.spline.SetControlTangent(controlPointEditor.controlPointIdx,
                                                  tangent.magnitude < TANGENT_ZERO_SNAP_MAGNITUDE ? Vector3.zero : tangent);
    }

    private void onGraspEnd() {
      checkZeroTangent();
    }

    public void RefreshHandle() {
      Vector3 tangent = controlPointEditor.spline[controlPointEditor.controlPointIdx].tangent
                        * (isForwardTangent ? 1F : -1F);
      this.transform.position = controlPointEditor.transform.position + tangent;

      checkZeroTangent();
    }

    private void checkZeroTangent() {
      Vector3 tangent = controlPointEditor.spline[controlPointEditor.controlPointIdx].tangent
                        * (isForwardTangent ? 1F : -1F);
      if (tangent.magnitude < TANGENT_ZERO_SNAP_MAGNITUDE) {
        if (!_intObj.isGrasped) _intObj.ignoreGrasping = true;
        foreach (Collider c in _intObj.primaryHoverColliders) {
          c.enabled = false;
        }
      }
      else {
        _intObj.ignoreGrasping = false;
        foreach (Collider c in _intObj.primaryHoverColliders) {
          c.enabled = true;
        }
      }
    }

  }

}
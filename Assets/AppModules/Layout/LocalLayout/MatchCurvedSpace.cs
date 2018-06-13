using Leap.Unity.Attributes;
using Leap.Unity.Space;
using UnityEngine;

namespace Leap.Unity.Layout {

  [ExecuteInEditMode]
  public class MatchCurvedSpace : MonoBehaviour {

    public LeapSpace leapSpace;

    private void Reset() {
      if (leapSpace == null) {
        leapSpace = FindObjectOfType<LeapSpace>();
      }
    }

    [Header("Manual Specification")]
    public Vector3 localRectangularPosition = Vector3.zero;
    public bool matchRotation = true;
    public Vector3 localRectangularRotation = Vector3.zero;

    [Header("ILocalPositionProvider (overrides manual local position)")]
    [ImplementsInterface(typeof(ILocalPositionProvider))]
    public MonoBehaviour localPositionProvider = null;

    private void Update() {
      refreshPosition();
    }

    private void refreshPosition() {
      if (leapSpace != null) {
        if (leapSpace.transformer != null) {
          var localRectPos = leapSpace.transform.InverseTransformPoint(
                               this.transform.parent.TransformPoint(
                                 localPositionProvider == null ? localRectangularPosition
                                                               : (localPositionProvider as ILocalPositionProvider)
                                                                 .GetLocalPosition(this.transform)));
          this.transform.position =
            leapSpace.transform.TransformPoint(
              leapSpace.transformer.TransformPoint(
                localRectPos));

          if (matchRotation) {
            this.transform.rotation =
              leapSpace.transform.TransformRotation(
                leapSpace.transformer.TransformRotation(
                  localRectPos,
                  leapSpace.transform.InverseTransformRotation(
                    this.transform.parent.TransformRotation(
                      Quaternion.Euler(localRectangularRotation)))));
          }
        }
      }
    }

  }
  
}

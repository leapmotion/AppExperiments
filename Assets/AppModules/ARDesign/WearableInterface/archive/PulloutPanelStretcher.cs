using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  [ExecuteInEditMode]
  public class PulloutPanelStretcher : MonoBehaviour {

    public Transform rootA;
    public Transform rootB;

    [Disable]
    [Tooltip("Put a Quad on this GameObject and make sure it isn't being scaled by any "
           + "parent during normal use.")]
    public string stretchElement = "This transform's local X scale";

    [Disable]
    [Tooltip("The panel quad will connect rootA to rootB along Root A's X axis.")]
    public string stretchDirection = "RootA's X Axis";

    [Tooltip("This local scale X value is modified, expecting a scale of 1 to be result "
           + "in a quad THIS long in world meters.")]
    [MinValue(0.01f)]
    public float quadBaseScale = 1f;

    public bool enforceNonZeroScale = false;
    public const float VERY_VERY_SMALL = 0.00001f;

    public bool enforceNonNegativeScale = true;

    private void Update() {
      if (rootA != null && rootB != null) {
        updateStretch();
      }
    }

    private void updateStretch() {
      var a = rootA.position;
      var b = rootB.position;
      var ab = b - a;

      var aXDir = rootA.rotation * Vector3.right;
      var abAlongX = Vector3.Dot(ab, aXDir);

      var targetScale = abAlongX / (quadBaseScale * this.transform.parent.lossyScale.x);

      if (enforceNonZeroScale) {
        targetScale = Mathf.Max(VERY_VERY_SMALL, targetScale);
      }
      if (enforceNonNegativeScale) {
        targetScale = Mathf.Max(0f, targetScale);
      }

      this.transform.localScale = this.transform.localScale.WithX(targetScale);
    }

  }

}

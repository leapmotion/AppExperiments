using Leap.Unity.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Layout {

  public class UILookPositionProvider : MonoBehaviour,
                                        IWorldPositionProvider {

    public Transform lookPositionTransform;

    private void Reset() {
      if (lookPositionTransform == null) lookPositionTransform = this.transform;
    }

    [Header("Optional")]

    [Tooltip("If this property is non-null, its 'on' localPosition (transformed into "
           + "world space) will be used to calculate the look anchor world position, "
           + "instead of relying on the current transform's position.")]
    public TranslationSwitch lookAnchorTranslationSwitch;

    #region IWorldPositionProvider

    public Vector3 GetTargetWorldPosition() {
      if (lookAnchorTranslationSwitch != null) {
        // This is the magic that allows an "expandable" UI return its "open" world
        // position instead of its current position, which may be subject to animation.
        return lookAnchorTranslationSwitch.localTranslateTarget
                                          .parent
                                          .TransformPoint(lookAnchorTranslationSwitch.onLocalPosition);
      }
      else {
        return lookPositionTransform.position;
      }
    }

    #endregion
  }


}
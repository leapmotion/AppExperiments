using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicHoverPaletteController : GraphicPaletteController {

  #region Inspector

  [Header("Hover")]

  public InteractionBehaviour intObj;

  public bool useNormalHover = false;

  [DisableIf("useNormalHover", isEqualTo: false)]
  public int hoveredColorIdx;
  [DisableIf("useNormalHover", isEqualTo: false)]
  public float hoverFalloffDistance = 0.10f;

  public int primaryHoverColorIdx;

  #endregion

  #region Properties

  public Color hoveredColor { get { return palette[hoveredColorIdx]; } }
  public Color primaryHoveredColor { get { return palette[primaryHoverColorIdx]; } }

  #endregion

  #region Unity Events

  protected override void Reset() {
    base.Reset();

    intObj = GetComponent<InteractionBehaviour>();
  }

  protected override void OnValidate() {
    base.OnValidate();

    if (palette != null) {
      if (palette.colors.Length != 0) {
        validateColorIdx(ref hoveredColorIdx);
        validateColorIdx(ref primaryHoverColorIdx);
      }
    }
  }

  #endregion

  #region Palette Controller Implementation

  protected override Color updateTargetColor() {
    var targetColor = restingColor;

    if (intObj.isPrimaryHovered) {
      targetColor = primaryHoveredColor;
    }
    else if (intObj.isHovered && useNormalHover) {
      targetColor = Color.Lerp(restingColor, hoveredColor,
                      intObj.closestHoveringControllerDistance
                            .Map(hoverFalloffDistance, 0f, 0f, 1f));
    }

    return targetColor;
  }

  #endregion

}

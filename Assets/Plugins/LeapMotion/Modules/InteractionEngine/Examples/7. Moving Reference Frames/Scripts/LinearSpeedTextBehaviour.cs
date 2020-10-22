/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;

namespace Leap.Unity.Examples {

  [AddComponentMenu("")]
  public class LinearSpeedTextBehaviour : MonoBehaviour {

    public TextMesh textMesh;

    public Spaceship ship;

    public string linearSpeedPrefixText;

    public string linearSpeedPostfixText;

    void Update() {
      textMesh.text = linearSpeedPrefixText + ship.shipAlignedVelocity.magnitude.ToString("G3") + linearSpeedPostfixText;
    }
  }
}

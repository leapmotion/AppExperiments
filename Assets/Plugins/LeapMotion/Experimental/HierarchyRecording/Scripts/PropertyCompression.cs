/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;

namespace Leap.Unity.Recording {

  public class PropertyCompression : MonoBehaviour {

    public NamedCompression[] compressionOverrides;

    [Serializable]
    public class NamedCompression {
      public string propertyName;

      [MinValue(0)]
      public float maxError;
    }
  }

}

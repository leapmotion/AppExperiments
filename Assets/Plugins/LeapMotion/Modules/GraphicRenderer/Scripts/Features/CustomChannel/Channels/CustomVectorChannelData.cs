/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;
using System;

namespace Leap.Unity.GraphicalRenderer {

  public partial class LeapGraphic {

    /// <summary>
    /// Helper method to set the custom channel value for the given channel
    /// name.  This method will throw an exception if there is no channel
    /// with the given name, if the graphic is not currently attached to a
    /// group, or if the channel does not match up with the data type.
    /// </summary>
    public void SetCustomChannel(string channelName, Vector4 value) {
      GetCustomChannel<CustomVectorChannelData>(channelName).value = value;
    }
  }

  [LeapGraphicTag("Vector Channel")]
  [Serializable]
  public class CustomVectorChannelData : CustomChannelDataBase<Vector4> { }
}

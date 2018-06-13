using Leap.Unity.UI;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class Button_UconChannel : UIButton {

    [Header("Ucon Channel Output")]
    public BangChannel pressOutputChannel = new BangChannel("tool/action");

    public override void OnUnpress() {
      pressOutputChannel.Set(new Bang());
    }

  }

}

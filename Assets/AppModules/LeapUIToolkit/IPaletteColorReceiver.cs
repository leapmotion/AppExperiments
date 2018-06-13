using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.UI {

  public interface IPaletteColorReceiver {

    void Receive(int paletteColorIdx);

  }

}

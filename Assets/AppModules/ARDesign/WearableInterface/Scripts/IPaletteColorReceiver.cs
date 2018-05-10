using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public interface IPaletteColorReceiver {

    void Receive(int paletteColorIdx);

  }

}

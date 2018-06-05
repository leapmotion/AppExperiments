using UnityEngine;

namespace Leap.Unity.ColorPalettes {

  public interface IColorReceiver {

    void Receive(Color color);

  }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

namespace Leap.Unity.zzOldDrawing {

  public class PaintLeftPinchRightController : MonoBehaviour {

    public Brush brush;
    public Chirality pinchControlHand;
    public Chirality indexBrushHand;

    void Update() {
      Hand brushHand = Hands.Get(indexBrushHand);
      if (brushHand != null) {
        Hand controlHand = Hands.Get(pinchControlHand);
        if (controlHand != null) {
          brush.transform.position = brushHand.GetIndex().TipPosition.ToVector3();

          if (controlHand.IsPinching() && !brush.IsBrushing()) {
            brush.Begin();
          }
          if (!controlHand.IsPinching() && brush.IsBrushing()) {
            brush.End();
          }
        }
      }
    }

  }


}
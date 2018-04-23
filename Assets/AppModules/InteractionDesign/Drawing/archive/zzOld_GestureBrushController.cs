using Leap.Unity.Attributes;
using Leap.Unity.Drawing;
using Leap.Unity.Gestures;
using Leap.Unity.Intention;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class zzOld_GestureBrushController : MonoBehaviour {

    [ImplementsInterface(typeof(zzOld_IBrush))]
    [SerializeField]
    private MonoBehaviour _brush;
    public zzOld_IBrush brush {
      get { return _brush as zzOld_IBrush; }
      set { _brush = value as MonoBehaviour; }
    }

    [SerializeField, OnEditorChange("poseGesture")]
    [ImplementsInterface(typeof(IPoseGesture))]
    private MonoBehaviour _poseGesture;
    public IPoseGesture poseGesture {
      get { return _poseGesture as IPoseGesture; }
      set { _poseGesture = value as MonoBehaviour; }
    }

    void Update() {
      brush.Move(poseGesture.pose);

      if (poseGesture.isActive && !brush.isBrushing) {
        brush.Begin();
      }

      if (!poseGesture.isActive && brush.isBrushing) {
        brush.End();
      }
    }

  }

}

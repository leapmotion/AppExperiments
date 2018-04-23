using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public interface zzOld_IBrush {

    bool isBrushing { get; }
    
    Pose  pose { get; }
    float size { get; set; }
    Color color { get; set; }

    void Move(Pose newPose);
    void Begin();
    void End();

  }

}

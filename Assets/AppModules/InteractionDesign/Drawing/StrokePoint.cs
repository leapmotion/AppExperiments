using UnityEngine;

namespace Leap.Unity.Drawing {

  [System.Serializable]
  public struct StrokePoint {
    public Pose  pose;
    public Color color;
    public float radius;
    
    /// <summary>
    /// The world-space transform representing the painting origin when this stroke was
    /// placed.
    /// </summary>
    public Matrix4x4 temp_refFrame;

    public Vector3 position { get { return pose.position; } }
    public Quaternion rotation { get { return pose.rotation; } }
  }

}

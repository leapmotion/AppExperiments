

using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class PhysicalInterfaceUtils {

    #region Constants

    /// <summary>
    /// The minimum speed past which a released object should be considered thrown,
    /// and beneath which a released object should be considered placed.
    /// </summary>
    public const float MIN_THROW_SPEED = 0.70f;
    public const float MIN_THROW_SPEED_SQR = MIN_THROW_SPEED * MIN_THROW_SPEED;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 10 m/s represents
    /// about a quarter of the speed of the world's fastest fastball.
    /// </summary>
    public const float MID_THROW_SPEED = 10.00f;

    /// <summary>
    /// For the purposes of mapping values based on throw speed, 40 m/s is about the
    /// speed of the fastest fast-ball. (~90 mph.)
    /// </summary>
    public const float MAX_THROW_SPEED = 40.00f;

    /// <summary>
    /// A standard speed for calculating e.g. how much time it should take for an
    /// element to move a given distance.
    /// </summary>
    public const float STANDARD_SPEED = 1.00f;

    /// <summary>
    /// A standard minimum speed under which an object is considered to be standing
    /// still.
    /// </summary>
    public const float MIN_MOVING_SPEED = 0.001f;

    /// <summary>
    /// As MIN_MOVING_SPEED, but squared, for checks against sqrMagnitude.
    /// </summary>
    public const float MIN_MOVING_SPEED_SQR = MIN_MOVING_SPEED * MIN_MOVING_SPEED;

    /// <summary>
    /// A distance representing being well within arms-reach without being too close to
    /// the head.
    /// </summary>
    public const float OPTIMAL_UI_DISTANCE = 0.60f;

    #endregion

    #region Functions

    public static Pose SmoothMove(Pose prev, Pose current, Pose target,
                                  float rigidness = 0f) {
      var prevSqrDist = (current.position - prev.position).sqrMagnitude;
      var lerpFilter = prevSqrDist.Map(0.0f, 0.4f, 0.2f, 1f);

      //lerpFilter = Vector3.Dot((current.position - prev.position),
      //                         (target.position - current.position))
      //                    .Map(0f, 1f, 0f, 1f);

      var prevAngle = Quaternion.Angle(current.rotation, prev.rotation);
      var slerpFilter = prevAngle.Map(0.0f, 16f, 0.01f, 1f);

      //slerpFilter = Vector3.Dot(current.rotation.From(prev.rotation).ToAngleAxisVector(),
      //                          target.rotation.From(current.rotation).ToAngleAxisVector())
      //                     .Map(0f, 1f, 0f, 1f);

      var sqrDist = (target.position - current.position).sqrMagnitude;
      float angle = Quaternion.Angle(current.rotation, target.rotation);

      var smoothLerpCoeff = sqrDist.Map(0.00001f, 0.0004f, 0.2f, 0.8f) * lerpFilter;
      var rigidLerpCoeff = 1f;
      var effLerpCoeff = Mathf.Lerp(smoothLerpCoeff, rigidLerpCoeff, rigidness.Clamp01());

      var smoothSlerpCoeff = angle.Map(0.3f, 4f, 0.01f, 0.8f) * slerpFilter;
      var rigidSlerpCoeff = 1f;
      var effSlerpCoeff = Mathf.Lerp(smoothSlerpCoeff, rigidSlerpCoeff, rigidness.Clamp01());

      var smoothedPose = new Pose(Vector3.Lerp(current.position,
                                               target.position,
                                               effLerpCoeff),
                                  Quaternion.Slerp(current.rotation,
                                                   target.rotation,
                                                   effSlerpCoeff));

      return smoothedPose;
    }

    #endregion

  }

  public static class FloatExtensions {
    public static float Clamp01(this float f) {
      return Mathf.Clamp01(f);
    }
  }

}

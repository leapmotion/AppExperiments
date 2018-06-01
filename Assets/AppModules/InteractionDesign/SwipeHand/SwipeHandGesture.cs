using System;
using Leap.Unity.Attributes;
using Leap.Unity.Infix;
using UnityEngine;
using UnityEngine.Events;
using Leap.Unity.RuntimeGizmos;

namespace Leap.Unity.Gestures {

  public class SwipeHandGesture : OneHandedGesture, IPoseGesture, IStream<Pose> {

    #region Intention (NYI)

    // TODO: Incorporate intention system for exclusivity
    //[Header("Intention System")]
    //[SerializeField]
    //private bool _requireIntent = true;

    #endregion

    #region Inspector

    #region Core Pinch Heuristic Checks

    private const string CATEGORY_PINCH_HEURISTIC = "Pinch Heuristic";

    [Header("Activation")]

    [DevGui.DevCategory(CATEGORY_PINCH_HEURISTIC)]
    [DevGui.DevValue]
    public bool drawDebugPinchDistance = false;

    [DevGui.DevCategory(CATEGORY_PINCH_HEURISTIC)]
    [DevGui.DevValue]
    [Range(0f, 0.04f)]
    public float pinchActivateDistance = 0.0075f;

    [Header("Deactivation")]

    [DevGui.DevCategory(CATEGORY_PINCH_HEURISTIC)]
    [DevGui.DevValue]
    [Range(0f, 0.04f)]
    public float pinchDeactivateDistance = 0.035f;


    [DevGui.DevCategory(CATEGORY_PINCH_HEURISTIC)]
    [DevGui.DevValue]
    [Range(0f, 0.04f)]
    public float failedPinchResetDistance = 0.010f;

    #endregion

    #region Gesture Eligibility -- Visual Feedback only!

    #region Eligibility Angles

    private const string CATEGORY_INDEX_ANGLE = "Index Angle Eligibility (Visual Only)";

    [Header("Eligibility (Visual Only)")]

    [DevGui.DevCategory(CATEGORY_INDEX_ANGLE)]
    [DevGui.DevValue]
    [Range(45f, 130f)]
    [Tooltip("Angle from the palm normal to the index finger direction to make the "
           + "gesture eligible.")]
    public float maxIndexAngleForEligibilityActivation = 98f;

    [DevGui.DevCategory(CATEGORY_INDEX_ANGLE)]
    [DevGui.DevValue]
    [Range(45f, 130f)]
    [Tooltip("Angle from the palm normal to the index finger direction to make the "
           + "gesture ineligible.")]
    public float maxIndexAngleForEligibilityDeactivation = 110f;

    private const string CATEGORY_THUMB_ANGLE = "Thumb Angle Eligibility (Visual Only)";

    [DevGui.DevCategory(CATEGORY_THUMB_ANGLE)]
    [DevGui.DevValue]
    [Range(45f, 130f)]
    [Tooltip("Angle from the palm normal to the thumb direction to make the gesture "
           + "eligible.")]
    public float maxThumbAngleForEligibilityActivation = 85f;

    [DevGui.DevCategory(CATEGORY_THUMB_ANGLE)]
    [DevGui.DevValue]
    [Range(45f, 130f)]
    [Tooltip("Angle from the palm normal to the thumb direction to make the gesture "
           + "ineligible.")]
    public float maxThumbAngleForEligibilityDeactivation = 100f;

    #endregion

    #region Feedback

    [Header("Custom Feedback")]

    public FloatEvent OnPinchStrengthEvent;
    [System.Serializable] public class FloatEvent : UnityEvent<float> { }

    #endregion

    #region Debug

    [Header("Debug")]

    public bool _drawDebug = false;
    public bool _drawDebugPath = false;

    #endregion

    #endregion

    #endregion

    #region Custom Pinch Distance

    public static Vector3 PinchSegment2SegmentDisplacement(Hand h) {
      Vector3 c0, c1; float t0, t1;
      return PinchSegment2SegmentDisplacement(h, out c0, out c1, out t0, out t1);
    }
    public static Vector3 PinchSegment2SegmentDisplacement(Hand h,
                                                           out Vector3 c0,
                                                           out Vector3 c1, 
                                                           out float t0,
                                                           out float t1) {
      var indexDistal = h.GetIndex().bones[0].NextJoint.ToVector3();
      var indexTip = h.GetIndex().TipPosition.ToVector3();
      var thumbDistal = h.GetThumb().bones[2].NextJoint.ToVector3();
      var thumbTip = h.GetThumb().TipPosition.ToVector3();

      return Segment2SegmentDisplacement(indexDistal, indexTip, thumbDistal, thumbTip,
                                         out c0, out c1, out t0, out t1);
    }

    public static float Static_GetCustomPinchStrength(Hand h) {
      Vector3 c0, c1; float t0, t1;
      float pinchDistance = PinchSegment2SegmentDisplacement(h, out c0, out c1, out t0, out t1).magnitude;

      pinchDistance -= 0.01f;
      pinchDistance = pinchDistance.Clamped01();

      if (Input.GetKeyDown(KeyCode.C)) {
        Debug.Log(pinchDistance);
      }

      return pinchDistance.MapUnclamped(0.0168f, 0.08f, 1f, 0f);
    }

    public float GetCustomPinchDistance(Hand h, out float swipeLength) {
      Vector3 c0, c1; float t1;
      float pinchDistance = PinchSegment2SegmentDisplacement(h, out c0, out c1, out swipeLength, out t1).magnitude;
      swipeLength = 1f - swipeLength;
      pinchDistance -= 0.01f;
      pinchDistance = pinchDistance.Clamped01();

      if (Input.GetKeyDown(KeyCode.C)) {
        Debug.Log(pinchDistance);
      }

      if (drawDebugPinchDistance) {
        DebugPing.Line("RH pinch", c0, c1, LeapColor.blue);
        //DebugPing.Label("RH pinch",
        //                labelText: pinchDistance.ToString("F3"),
        //                labeledPosition: ((c1 + c0) / 2f),
        //                color: LeapColor.blue);
        Vector3 avg = ((c1 + c0) / 2f);
        DebugPing.Label("RH pinch",
                        labelText: pinchDistance.ToString("F3"),
                        labeledPosition: avg,
                        color: LeapColor.blue);

        Vector3 basePos = h.PalmPosition.ToVector3() + Camera.main.transform.up * 0.1f;
        RuntimeGizmoDrawer drawer;
        RuntimeGizmoManager.TryGetGizmoDrawer(out drawer);
        drawer.color = Color.white;
        drawer.DrawWireCapsule(basePos, basePos + (Camera.main.transform.right * 0.1f), 0.01f);
        drawer.color = Color.green;
        drawer.DrawWireCapsule(basePos, basePos + (Camera.main.transform.right * swipeLength * 0.1f), 0.01f);
      }

      return pinchDistance;
    }

    #region Segment-to-Segment Displacement (John S)

    public static Vector3 Segment2SegmentDisplacement(Vector3 a1, Vector3 a2,
                                                      Vector3 b1, Vector3 b2,
                                                      out Vector3 c0, out Vector3 c1, 
                                                      out float t0, out float t1) {
      return PinchGesture.Segment2SegmentDisplacement(a1, a2, b1, b2,
                                                      out t0, out t1,
                                                      out c0, out c1);
    }

    #endregion

    #endregion

    #region Private Memory

    #region Core Buffers

    /// <summary>
    /// One of the heuristics uses the velocity of the hand to adjust the activation
    /// pinch distance.
    /// </summary>
    private DeltaBuffer _handPositionBuffer = new DeltaBuffer(5);

    /// <summary>
    /// An automatically-mapped value from 0 to 1, where 1 indicates a pinch will have
    /// activated, or 0 if the pinch is ineligible.
    /// </summary>
    private float _latestPinchStrength;

    /// <summary>
    /// An automatically-mapped value from 0 to 1, where 1 indicates a swipe is 
    /// away from the hand, and a zero indicates the swipe is towards the palm.
    /// </summary>
    private float _latestSwipeLength;

    #endregion

    #region Finger Curl Buffers (for Safety Pinch)

    private DeltaFloatBuffer _indexCurlBuffer = new DeltaFloatBuffer(5);
    private DeltaFloatBuffer _middleCurlBuffer = new DeltaFloatBuffer(5);

    private void updateIndexCurl(Hand h) {
      var index = h.GetIndex();
      var indexCurl = getCurl(h, index);

      _indexCurlBuffer.Add(indexCurl, Time.time);
    }

    private void updateMiddleCurl(Hand h) {
      var middle = h.GetMiddle();
      var middleCurl = getCurl(h, middle);

      _middleCurlBuffer.Add(middleCurl, Time.time);
    }

    private float getCurl(Hand h, Finger f) {
      return getBaseCurl(h, f);
    }

    /// <summary>
    /// Base curl is rotation at the knuckle joint. This is more reliable than grip curl,
    /// which is the rotation of the joint between the proximal and medial finger bones.
    /// </summary>
    private float getBaseCurl(Hand h, Finger f) {
      var palmAxis = h.PalmarAxis();
      var leftPositiveThumbAxis = h.RadialAxis() * (h.IsLeft ? 1f : -1f);
      int baseBoneIdx = 1;
      if (f.Type == Finger.FingerType.TYPE_THUMB) baseBoneIdx = 2;
      var baseCurl = f.bones[baseBoneIdx].Direction.ToVector3()
                      .SignedAngle(palmAxis, leftPositiveThumbAxis)
                      .Map(0f, 90f, 1f, 0f);

      return baseCurl;
    }

    /// <summary>
    /// Grip curl is really unreliably tracked, so I don't recommend using it.
    /// </summary>
    private float getGripCurl(Hand h, Finger f) {
      var leftPositiveThumbAxis = h.RadialAxis() * (h.IsLeft ? 1f : -1f);
      int baseBoneIdx = 1;
      if (f.Type == Finger.FingerType.TYPE_THUMB) baseBoneIdx = 2;
      var baseDir = f.bones[baseBoneIdx].Direction.ToVector3();

      var gripAngle = baseDir.SignedAngle(
                                f.bones[3].Direction.ToVector3(),
                                leftPositiveThumbAxis);

      if (gripAngle < -30f) {
        gripAngle += 360f;
      }

      var gripCurl = gripAngle.Map(0f, 150f, 0f, 1f);
      return gripCurl;
    }

    #endregion

    #region Timers

    /// <summary>
    /// Value in number of frames.
    /// </summary>
    private const int MIN_REACTIVATE_TIME = 5;
    private int minReactivateTimer = 0;

    /// <summary>
    /// Value in number of frames.
    /// </summary>
    private const int MIN_REACTIVATE_TIME_SINCE_DEGENERATE_CONDITIONS = 6;
    private int minReactivateSinceDegenerateConditionsTimer = 0;

    /// <summary>
    /// Value in number of frames.
    /// </summary>
    private const int MIN_DEACTIVATE_TIME = 5;
    private int minDeactivateTimer = 0;

    #endregion

    #region Failed-pinch Memory

    private bool requiresRepinch = false;

    #endregion

    #region Eligibility Memory

    private bool _isGestureEligible = false;
    public override bool isEligible {
      get {
        return base.isEligible && (isActive || _isGestureEligible);
      }
    }

    #endregion

    #region Pose Gesture Memory

    private Pose _lastPinchPose = Pose.identity;

    #endregion

    #endregion

    #region OneHandedGesture

    #region ShouldGestureActivate

    protected override bool ShouldGestureActivate(Hand hand) {
      bool shouldActivate = false;

      var wasEligibleLastCheck = _isGestureEligible;
      _isGestureEligible = false;

      // Update curl samples for each index and middle.
      updateIndexCurl(hand);
      updateMiddleCurl(hand);

      // Need to update the "pinch strength" during processing.
      _latestPinchStrength = 0f;
      _latestSwipeLength = 0f;

      // Can only activate a pinch if we haven't already activated a pinch very recently.
      if (minReactivateTimer > MIN_REACTIVATE_TIME) {

        // Can only activate a pinch if we're a certain number of frames past the last
        // frame where the hand was in a tracking-degenerate orientation (e.g. looking
        // down the wrist.)
        if (minReactivateSinceDegenerateConditionsTimer
              > MIN_REACTIVATE_TIME_SINCE_DEGENERATE_CONDITIONS) {

          // Update pinch and hand position samples.
          float latestSwipeLength;
          float latestPinchDistance = GetCustomPinchDistance(hand, out latestSwipeLength);
          OnPinchStrengthEvent.Invoke(latestPinchDistance);
          _handPositionBuffer.Add(hand.PalmPosition.ToVector3(), Time.time);


          // Full buffer == optimally stable hand velocity, also implicitly enforces
          // a hand lifetime. Hand velocity NOT CURRENTLY ACTUALLY USED.
          if (_handPositionBuffer.IsFull) {

            // Determine whether the hand meets the FOV heuristic -- result may be
            // ignored depending on public settings.
            var handFOVAngle = Vector3.Angle(Camera.main.transform.forward,
            hand.PalmPosition.ToVector3() - Camera.main.transform.position);
            var handWithinFOV = handFOVAngle < Camera.main.fieldOfView / 2.2f;

            // Heuristic: Higher hand velocity == more stringent pinch requirement.
            // Goal: Reduce accidental pinches when e.g. dropping the hands by requiring
            // a more "certain" pinch while the hand is moving.
            // CURRENTLY UNUSED. Comment left here as potential inspiration for
            // additional heuristics. TODO DELETEME.
#pragma warning disable 0219
            var handVelocity = _handPositionBuffer.Delta();
#pragma warning restore 0219

            var palmDir = hand.PalmarAxis();

            #region Index Angle (Eligibility Only)

            // Note: obviously pinching already requires the index finger to
            // close relative to the palm -- this check simply drives the
            // isEligible state for this pinch gesture so that the gesture isn't
            // "eligible" when the hand is fully open.

            var indexDir = hand.GetIndex().bones[1].Direction.ToVector3();
            var indexPalmAngle = Vector3.Angle(indexDir, palmDir);

            #endregion

            #region Thumb Angle (Eligibility Only)

            // Note: obviously pinching already requires the thumb finger to
            // close to touch the index finger -- this check simply drives the
            // isEligible state for this pinch gesture so that the gesture isn't
            // "eligible" when the hand is fully open.

            var thumbDir = hand.GetThumb().bones[2].Direction.ToVector3();
            var thumbPalmAngle = Vector3.Angle(thumbDir, palmDir);

            #endregion

            #region Check: Eligibility

            // Eligibility checks -- necessary, but not sufficient conditions to start
            // a pinch, suitable for e.g. visual feedback on whether the gesture is
            // "able to occur" or "about to occur."
            if (
                // Index angle (eligibility state only)
                ((!wasEligibleLastCheck
                     && indexPalmAngle < maxIndexAngleForEligibilityActivation)
                    || (wasEligibleLastCheck
                        && indexPalmAngle < maxIndexAngleForEligibilityDeactivation))

                // Thumb angle (eligibility state only)
                && ((!wasEligibleLastCheck
                     && thumbPalmAngle < maxThumbAngleForEligibilityActivation)
                    || (wasEligibleLastCheck
                        && thumbPalmAngle < maxThumbAngleForEligibilityDeactivation))

                // FOV.
                && (handWithinFOV)

                // Must cross pinch threshold from a non-pinching / non-fist pose.
                && (!requiresRepinch)

                ) {

              // Conceptually, this should be true when all but the most essential
              // parameters for the gesture are satisfied, so the user can be notified
              // that the gesture is imminent.
              _isGestureEligible = true;
            }

            #endregion

            #region Update Pinch Strength

            // Update global "pinch strength".
            // If the gesture is eligible, we'll have a non-zero pinch strength.
            if (_isGestureEligible) {
              _latestPinchStrength = latestPinchDistance.Map(0f, pinchActivateDistance,
                                                             1f, 0f);
              _latestSwipeLength = latestSwipeLength;
            } else {
              _latestPinchStrength = 0f;
            }

            #endregion

            #region Check: Pinch Distance

            if (_isGestureEligible

                // Absolute pinch strength.
                && (latestPinchDistance < pinchActivateDistance)

                    ) {
              shouldActivate = true;

              if (_drawDebug) {
                DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.red, 0.20f);
              }
            }

            #endregion

            #region Hysteresis for Failed Pinches

            // "requiresRepinch" prevents a closed-finger configuration from beginning
            // a pinch when the index and thumb never actually actively close from a
            // valid position -- think, closed-fist to safety-pinch, as opposed to
            // open-hand to safety-pinch -- without introducing any velocity-based
            // requirement.
            if (latestPinchDistance < pinchActivateDistance && !shouldActivate) {
              requiresRepinch = true;
            }
            if (requiresRepinch && latestPinchDistance > failedPinchResetDistance) {
              requiresRepinch = false;
            }

            #endregion
          }
        } else {
          minReactivateSinceDegenerateConditionsTimer += 1;
          float latestSwipeLength;
          float latestPinchDistance = GetCustomPinchDistance(hand, out latestSwipeLength);
        }

      } else {
        minReactivateTimer += 1;
        float latestSwipeLength;
        float latestPinchDistance = GetCustomPinchDistance(hand, out latestSwipeLength);
      }

      if (shouldActivate) {
        minDeactivateTimer = 0;
      }

      OnPinchStrengthEvent.Invoke(_latestPinchStrength);

      return shouldActivate;
    }

    #endregion

    #region ShouldGestureDeactivate

    protected override bool ShouldGestureDeactivate(Hand hand,
                                                    out DeactivationReason?
                                                      deactivationReason) {
      deactivationReason = DeactivationReason.FinishedGesture;

      bool shouldDeactivate = false;

      _latestPinchStrength = 1f;
      OnPinchStrengthEvent.Invoke(_latestPinchStrength);

      if (minDeactivateTimer > MIN_DEACTIVATE_TIME) {
        float latestSwipeLength;
        float pinchDistance = GetCustomPinchDistance(hand, out latestSwipeLength);

        if (pinchDistance > pinchDeactivateDistance) {
          shouldDeactivate = true;

          if (_drawDebug) {
            DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.black, 0.20f);
          }
        }
      } else {
        float latestSwipeLength;
        float latestPinchDistance = GetCustomPinchDistance(hand, out latestSwipeLength);
        minDeactivateTimer++;
      }

      if (shouldDeactivate) {
        minReactivateTimer = 0;
      }

      return shouldDeactivate;
    }

    #endregion

    #region Secondary Events (IPoseGesture, IStream<Pose>, Degenerate conditions, ...)

    // TODO: OneHandedGesture should implement IPoseGesture AND IStream<Pose> by default!

    protected override void WhenGestureActivated(Hand hand) {
      base.WhenGestureActivated(hand);

      OnOpen();
    }

    protected override void WhileGestureActive(Hand hand) {
      if (_drawDebugPath) {
        DebugPing.Ping(hand.GetPredictedPinchPosition(), LeapColor.amber, 0.05f);
      }

      // TODO: Make this a part of OneHandedGesture so this doesn't have to be explicit!
      OnSend(this.pose);
    }

    protected override void WhenGestureDeactivated(Hand maybeNullHand,
                                                   DeactivationReason reason) {
      //_pinchDistanceBuffer.Clear();

      OnClose();
    }

    protected override void WhileHandTracked(Hand hand) {

      // Update pose with the position of the pinch, which is theoretical if there's no
      // pinch but absolute when a pinch is actually occuring.
      var pinchPosition = hand.GetPredictedPinchPosition();

      var avgIndexThumbTip = ((hand.GetIndex().TipPosition
                                 + hand.GetThumb().TipPosition) / 2f).ToVector3();
      pinchPosition = Vector3.Lerp(pinchPosition, avgIndexThumbTip, _latestPinchStrength);

      _lastPinchPose = new Pose() {
        position = pinchPosition,
        rotation = hand.Rotation.ToQuaternion()
      };

      // Reset the "degenerate conditions" timer if we detect that we're looking down
      // the wrist of the hand; here fingers are usually occluded, so we want to ignore
      // pinch information in this case.
      var lookingDownWrist = Vector3.Angle(hand.DistalAxis(),
         hand.PalmPosition.ToVector3() - Camera.main.transform.position) < 25f;
      if (lookingDownWrist) {
        if (_drawDebug) {
          DebugPing.Ping(hand.WristPosition.ToVector3(), Color.black, 0.10f);
        }
        minReactivateSinceDegenerateConditionsTimer = 0;
      }
    }

    #endregion

    #endregion

    #region IPoseGesture

    public Pose pose {
      get {
        return _lastPinchPose;
      }
    }

    #endregion

    #region IStream<Pose>

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    #endregion

  }

}
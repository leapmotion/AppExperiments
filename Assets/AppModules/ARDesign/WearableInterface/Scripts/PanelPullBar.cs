using Leap.Unity.Geometry;
using Leap.Unity.Gestures;
using Leap.Unity.Infix;
using Leap.Unity.Interaction;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.AR.Testing {

  using Collision = Leap.Unity.Geometry.Collision;

  public class PanelPullBar : MonoBehaviour {

    public InteractionBehaviour intObj; // used for closest hovering hand.
    public PinchGesture pinchGesture;
    public CapsuleCollider barCapsule;

    public float beginHoldDist = 0.10f;

    public Transform slideTransform = null;
    private Matrix4x4 slideMatrix { get { return slideTransform.localToWorldMatrix; } }
    private Matrix4x4 slideMatrixInv { get { return slideTransform.worldToLocalMatrix; } }

    private float _slideHeldSpeed = 80f;
    public float slideBackSpeed = 20f;

    /// <summary>
    /// The axis in slide-local space along which sliding is allowed.
    /// </summary>
    private Vector3 slideAxis_slide = Vector3.right;

    /// <summary>
    /// The maximum distance from "zero" in slide-local space to which sliding is allowed.
    /// </summary>
    public float maxDistance_slide = 0.2f;

    public float stayOpenDistance_slide = 0.17f;

    /// <summary>
    /// Farthest point along the slide, in slide-local space.
    /// </summary>
    public Vector3 maxSlidePos_slide {
      get {
        return slideAxis_slide * maxDistance_slide;
      }
    }

    private Vector3 holdOffset_slide = Vector3.zero;

    /// <summary>
    /// LocalSegment3 in slide-local space defining the line segment along which this
    /// component's Transform is allowed to slide.
    /// </summary>
    public LocalSegment3 slideSegment_slide {
      get {
        return new LocalSegment3(Vector3.zero, maxSlidePos_slide);
      }
    }

    public HingeElement[] hingeButtons;

    private bool _isHeld = false;
    private bool _isLockedOpen = false;

    private void Reset() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }
    private void OnValidate() {
      stayOpenDistance_slide = Mathf.Min(stayOpenDistance_slide, maxDistance_slide);
    }
    private void Start() {
      stayOpenDistance_slide = Mathf.Min(stayOpenDistance_slide, maxDistance_slide);

      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }

    private void Update() {
      //var isInteractingWithAButton = hingeButtons.Query().Any(b => b.isInteracting);
      
      // Determine whether to begin or end the held state this frame.
      var shouldBeginHolding = false;
      var interactingHand = intObj.closestHoveringHand;
      if (interactingHand != null) {
        if (pinchGesture.wasActivated) {
          var pinchPos = pinchGesture.pose.position;
          Vector3 barSegmentA, barSegmentB;
          barCapsule.GetCapsulePoints(out barSegmentA, out barSegmentB);
          var barPinchSqrDist = Geometry.Collision.SqrDistPointSegment(barSegmentA,
            barSegmentB, pinchPos);
          if (barPinchSqrDist < beginHoldDist * beginHoldDist) {
            shouldBeginHolding = true;
          }
        }
      }
      var shouldStopHolding = !pinchGesture.isActive;

      // Update grasped or released state of the slide.
      if (!_isHeld && shouldBeginHolding) {
        // Just grasped.
        _isHeld = true;

        var slidePos_slide = slideMatrixInv.MultiplyPoint3x4(this.transform.position);
        var holdPos_slide = slideMatrixInv.MultiplyPoint3x4(pinchGesture.pose.position);
        holdOffset_slide = slidePos_slide - holdPos_slide;
      }
      if (_isHeld && shouldStopHolding) {
        // Just released.
        _isHeld = false;

        // Lock the slide open if released while fully open.
        var pos = this.transform.position;
        var pos_slide = slideMatrixInv.MultiplyPoint3x4(pos);
        float slideT;
        var clampedPos_slide
          = Collision.ClosestPtPointSegment(slideSegment_slide, pos_slide, out slideT);
        var slideDistance_slide = clampedPos_slide.Dot(slideAxis_slide);
        _isLockedOpen = slideDistance_slide >= stayOpenDistance_slide;
      }

      // Slide the pull bar to match the held position or its target rest position.
      if (_isHeld) {
        var pinchPos = pinchGesture.pose.position;
        var heldPosition_slide = slideMatrixInv.MultiplyPoint3x4(pinchPos);
        heldPosition_slide += holdOffset_slide;
        var clampedPosition_slide
          = Collision.ClosestPtPointSegment(slideSegment_slide, heldPosition_slide);

        var targetPos = slideMatrix.MultiplyPoint3x4(clampedPosition_slide);
        this.transform.position = Vector3.Lerp(this.transform.position, targetPos,
          (_slideHeldSpeed * Time.deltaTime));
      }
      else {
        var targetPosition = slideTransform.position;

        if (_isLockedOpen) {
          targetPosition = slideMatrix.MultiplyPoint3x4(maxSlidePos_slide);
        }

        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition,
          slideBackSpeed * Time.deltaTime);
      }
    }

    #region Archive -- segment 2 segment for basic pinch data

    ///// <summary>
    ///// Returns the smallest _squared_ pinch distance (pinching the thumb) of the hand,
    ///// checking the index, middle, and ring fingers.
    ///// 
    ///// Pinching is checked against the last thumb segment and the last two finger
    ///// segments of each finger. The point on the finger for the closest pinch distance
    ///// is output to a, and the point on the thumb for the pinch is output to b.
    ///// </summary>
    //private static float getPinchDistance(Hand hand, out Vector3 a, out Vector3 b) {
    //  Finger index = hand.GetIndex(), middle = hand.GetMiddle(), ring = hand.GetRing();

    //  Vector3 index_indexP, index_thumbP, middle_middleP, middle_thumbP,
    //    ring_ringP, ring_thumbP;

    //  var indexSqrDist
    //    = getFingerPinchDistance(hand, index, out index_indexP, out index_thumbP);
    //  var middleSqrDist
    //    = getFingerPinchDistance(hand, middle, out middle_middleP, out middle_thumbP);
    //  var ringSqrDist
    //    = getFingerPinchDistance(hand, ring, out ring_ringP, out ring_thumbP);

    //  float[] dists = { indexSqrDist, middleSqrDist, ringSqrDist };
    //  Vector3[] indexPs = { index_indexP, index_thumbP };
    //  Vector3[] middlePs = { middle_middleP, middle_thumbP };
    //  Vector3[] ringPs = { ring_ringP, ring_thumbP };
    //  Vector3[][] Ps = { indexPs, middlePs, ringPs };
      
    //  float smallestDist = float.PositiveInfinity; int smallestIdx = -1;
    //  for (int i = 0; i < dists.Length; i++) {
    //    var testDist = dists[i];
    //    if (smallestIdx == -1 || smallestDist == float.PositiveInfinity
    //        || testDist < smallestDist) {
    //      smallestDist = testDist;
    //      smallestIdx = i;
    //    }
    //  }
    //  a = Ps[smallestIdx][0];
    //  b = Ps[smallestIdx][1];
    //  return smallestDist;
    //}

    ///// <summary>
    ///// Checks pinch distance against the last two bones of the argument finger and
    ///// the last bone of the thumb, returns the squared distance between the two closest
    ///// finger-and-thumb segments, and outputs the closest positions on each segment.
    ///// </summary>
    //private static float getFingerPinchDistance(Hand hand, Finger finger,
    //  out Vector3 fingerP, out Vector3 thumbP) {
    //  var thumb = hand.GetThumb();

    //  var fingerSegBone1 = finger.bones[3];
    //  var fingerSegment1 = new LocalSegment3(fingerSegBone1.PrevJoint.ToVector3(),
    //    fingerSegBone1.NextJoint.ToVector3());

    //  var fingerSegBone2 = finger.bones[2];
    //  var fingerSegment2 = new LocalSegment3(fingerSegBone2.PrevJoint.ToVector3(),
    //    fingerSegBone2.NextJoint.ToVector3());

    //  var thumbSegBone = thumb.bones[3];
    //  var thumbSegment = new LocalSegment3(thumbSegBone.PrevJoint.ToVector3(),
    //    thumbSegBone.NextJoint.ToVector3());

    //  Vector3 fingerP1, thumbP1;
    //  float fingerT1, thumbT1;
    //  var sqrDist1 = Leap.Unity.Geometry.Collision.Intersect(fingerSegment1, thumbSegment,
    //    out fingerT1, out thumbT1, out fingerP1, out thumbP1);

    //  Vector3 fingerP2, thumbP2;
    //  float fingerT2, thumbT2;
    //  var sqrDist2 = Leap.Unity.Geometry.Collision.Intersect(fingerSegment2, thumbSegment,
    //    out fingerT2, out thumbT2, out fingerP2, out thumbP2);

    //  if (sqrDist1 <= sqrDist2) {
    //    fingerP = fingerP1; thumbP = thumbP1;
    //    return sqrDist1;
    //  }
    //  else {
    //    fingerP = fingerP2; thumbP = thumbP2;
    //    return sqrDist2;
    //  }
    //}

    #endregion

  }

}

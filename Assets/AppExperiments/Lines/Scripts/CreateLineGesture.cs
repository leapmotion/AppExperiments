using Leap.Unity.Attributes;
using Leap.Unity.Gestures;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  public class CreateLineGesture : TwoHandedGesture {

    [OnEditorChange("lineCreator")]
    [ImplementsInterface(typeof(ILineCreator))]
    [SerializeField]
    [Tooltip("A MonoBehaviour that implements ILineCreator, used to create new lines.")]
    private MonoBehaviour _lineCreator;
    public ILineCreator lineCreator {
      get {
        return _lineCreator as ILineCreator;
      }
      set {
        if (lineCreator != null && lineCreator.isCreatingLine) {
          lineCreator.CancelLine();
        }
        _lineCreator = value as MonoBehaviour;
      }
    }

    // TODO: Deleteme! Replace with a better gesture exclusivity system
    [Header("TODO: Delete this reference (exclusivity)")]
    public PullSplineGesture pullSplineGesture;

    private const float ACTIVATION_PINCH_STRENGTH = 0.85F;
    private const float DEACTIVATION_PINCH_STRENGTH = 0.75F;

    private Vector3 _leftLineSpawnPos;
    private Vector3 _rightLineSpawnPos;
    private float _spawnToPinchPosLerpCoeff = 0F;
    private Vector3 _leftHandPinchPos;
    private Vector3 _rightHandPinchPos;

    protected override bool ShouldGestureActivate(Hand leftHand, Hand rightHand) {
      if (pullSplineGesture != null && pullSplineGesture.isActive) return false;

      return !GetIsOutOfBounds(leftHand) && !GetIsOutOfBounds(rightHand)
             && leftHand.PinchStrength > ACTIVATION_PINCH_STRENGTH
             && rightHand.PinchStrength > ACTIVATION_PINCH_STRENGTH;
    }

    protected override bool ShouldGestureDeactivate(Hand leftHand, Hand rightHand,
                                                    out DeactivationReason? reason) {
      if (GetIsOutOfBounds(leftHand) || GetIsOutOfBounds(rightHand)) {
        reason = DeactivationReason.CancelledGesture;
        return true;
      }

      if (leftHand.PinchStrength < DEACTIVATION_PINCH_STRENGTH
          || rightHand.PinchStrength < DEACTIVATION_PINCH_STRENGTH) {
        reason = DeactivationReason.FinishedGesture;
        return true;
      }

      reason = null;
      return false;
    }

    /// <summary>
    /// Returns whether the hand is too far outside Camera.main's FOV.
    /// </summary>
    private bool GetIsOutOfBounds(Hand hand) {
      return Vector3.Dot(Camera.main.transform.forward,
                         (hand.PalmPosition.ToVector3()
                          - Camera.main.transform.position).normalized) < 0.5F;
    }

    protected override void WhileBothHandsTracked(Hand leftHand, Hand rightHand) {
      base.WhileBothHandsTracked(leftHand, rightHand);

      // Default line spawn point is between the hand pinch positions.
      _leftHandPinchPos = leftHand.GetPredictedPinchPosition();
      _rightHandPinchPos = rightHand.GetPredictedPinchPosition();
      Vector3 targetLineSpawnPos = (_leftHandPinchPos + _rightHandPinchPos) / 2F;

      // If only one hand is pinching, the spawn point should be at that
      // hand's pinch point.
      if (leftHand.PinchStrength > DEACTIVATION_PINCH_STRENGTH
          && rightHand.PinchStrength < ACTIVATION_PINCH_STRENGTH) {
        targetLineSpawnPos = _leftHandPinchPos;
      }
      if (leftHand.PinchStrength < DEACTIVATION_PINCH_STRENGTH
          && rightHand.PinchStrength > ACTIVATION_PINCH_STRENGTH) {
        targetLineSpawnPos = _rightHandPinchPos;
      }

      _leftLineSpawnPos = Vector3.Lerp(_leftLineSpawnPos, targetLineSpawnPos, 20F * Time.deltaTime);
      _rightLineSpawnPos = Vector3.Lerp(_rightLineSpawnPos, targetLineSpawnPos, 20F * Time.deltaTime);
    }

    protected override void WhenHandBecomesTracked(Hand hand) {
      base.WhenHandBecomesTracked(hand);

      // If a hand has just become tracked, we should reset the line point
      // to that hand's pinch point so that sudden line spawns don't appear
      // to come from far away.
      Vector3 pinchPos = hand.GetPredictedPinchPosition();
      if (hand.IsLeft) {
        _leftLineSpawnPos = pinchPos;
      }
      else {
        _rightLineSpawnPos = pinchPos;
      }
    }

    protected override void WhenGestureActivated(Hand leftHand, Hand rightHand) {
      base.WhenGestureActivated(leftHand, rightHand);

      lineCreator.BeginLine();
      lineCreator.UpdateLine(_leftLineSpawnPos, _rightLineSpawnPos);

      _spawnToPinchPosLerpCoeff = 0F;
    }

    protected override void WhenGestureDeactivated(Hand maybeNullLeftHand, Hand maybeNullRightHand, DeactivationReason reason) {
      base.WhenGestureDeactivated(maybeNullLeftHand, maybeNullRightHand, reason);

      if (reason == DeactivationReason.CancelledGesture) {
        lineCreator.CancelLine();
      }
      else {
        lineCreator.FinishLine();
      }

      _spawnToPinchPosLerpCoeff = 0F;
    }

    protected override void WhileGestureActive(Hand leftHand, Hand rightHand) {
      base.WhileGestureActive(leftHand, rightHand);

      _spawnToPinchPosLerpCoeff = Mathf.Lerp(_spawnToPinchPosLerpCoeff, 1F, 20F * Time.deltaTime);

      Vector3 leftLinePos = Vector3.Lerp(_leftLineSpawnPos, _leftHandPinchPos, _spawnToPinchPosLerpCoeff);
      Vector3 rightLinePos = Vector3.Lerp(_rightLineSpawnPos, _rightHandPinchPos, _spawnToPinchPosLerpCoeff);

      lineCreator.UpdateLine(leftLinePos, rightLinePos);
    }

  }

}
using System;

namespace Leap.Unity.Intention {

  #region Supporting Types

  public enum IntentionType {
    Free,
    SingleHand,
    BothHands
  }

  public struct IntentDefinition {

    public IntentionType type;

    public bool isSingleHand { get { return type == IntentionType.SingleHand; } }
    public Chirality handedness;

  }

  #endregion

  public class UserIntent {

    #region User Intent Objects

    #region Data

    private IntentDefinition _def;
    public IntentDefinition definition {
      get { return _def; }
      private set { _def = value; }
    }

    #endregion

    #region Construction

    private UserIntent(IntentDefinition intentDefinition) {
      _def = intentDefinition;
    }

    private static UserIntent intentForSingleHand(Chirality whichHand) {
      return new UserIntent(new IntentDefinition() {
        type = IntentionType.SingleHand,
        handedness = whichHand
      });
    }

    private static UserIntent intentForBothHands() {
      return new UserIntent(new IntentDefinition() {
        type = IntentionType.BothHands
      });
    }

    //public static UserIntent ForFree() {
    //  return new UserIntent(new IntentDefinition() {
    //    type = IntentionType.Free
    //  });
    //}

    #endregion

    #region Methods

    /// <summary>
    /// Returns whether this intent is a SingleHand intent.
    /// </summary>
    public bool IsSingleHandIntent() {
      return definition.type == IntentionType.SingleHand;
    }

    /// <summary>
    /// Returns the handedness of this intent, valid only if the intent type is
    /// SingleHand.
    /// </summary>
    public Chirality GetHandedness() {
      return definition.handedness;
    }

    #endregion;

    #endregion

    #region Static Intention System

    private static UserIntent _leftHandIntention  = intentForSingleHand(Chirality.Left);
    private static UserIntent _rightHandIntention = intentForSingleHand(Chirality.Right);
    private static UserIntent _bothHandIntention  = intentForBothHands();

    public static bool TryReceive(IntentDefinition intent, out UserIntent intentObject) {
      intentObject = null;

      switch (intent.type) {
        case IntentionType.SingleHand:
          if (intent.handedness == Chirality.Left) {
            if (_leftHandIntention != null && _bothHandIntention != null) {
              intentObject = _leftHandIntention;
              _leftHandIntention = null;
              return true;
            }
            else return false;
          }
          else {
            if (_rightHandIntention != null && _bothHandIntention != null) {
              intentObject = _rightHandIntention;
              _rightHandIntention = null;
              return true;
            }
            else return false;
          }
        case IntentionType.BothHands:
          if (_leftHandIntention != null
              && _rightHandIntention != null
              && _bothHandIntention != null) {
            intentObject = _bothHandIntention;
            _bothHandIntention = null;
            return true;
          }
          else return false;
        default:
          return false;
      }
    }

    /// <summary>
    /// Drops the provided intent object, returning it to the intention system. You must
    /// pass a valid intention object in by reference; upon calling this method, your
    /// reference to the object will be nullified.
    /// </summary>
    public static void Drop(ref UserIntent intent) {
      if (intent == null) return;

      switch (intent.definition.type) {
        case IntentionType.BothHands:
          _bothHandIntention = intent;
          intent = null;
          break;
        case IntentionType.SingleHand:
          if (intent.GetHandedness() == Chirality.Left) {
            _leftHandIntention = intent;
            intent = null;
          }
          else {
            _rightHandIntention = intent;
            intent = null;
          }
          break;
      }
    }

    #endregion

    #region Helpers
    
    private static Chirality otherChirality(Chirality handedness) {
      return handedness == Chirality.Left ? Chirality.Right : Chirality.Left;
    }

    #endregion

  }


}

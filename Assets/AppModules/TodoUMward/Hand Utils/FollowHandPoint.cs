using System;
using Leap.Unity.Attributes;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.Attachments {
  
  [ExecuteInEditMode]
  public class FollowHandPoint : MonoBehaviour, IStream<Pose> {

    #region Inspector

    [SerializeField]
    private LeapProvider _provider;
    public LeapProvider provider {
      get {
        if (_provider == null) { _provider = Hands.Provider; }
        return _provider;
      }
    }

    public Chirality whichHand;
    
    public AttachmentPointFlags attachmentPoint = AttachmentPointFlags.Palm;

    public enum FollowMode { Update, FixedUpdate }
    private FollowMode _followMode = FollowMode.Update;
    public FollowMode followMode {
      get {
        return _followMode;
      }
      set {
        if (value != _followMode) {
          unsubscribeFrameCallback();

          _followMode = value;

          subscribeFrameCallback();
        }
      }
    }

    [SerializeField]
    [Disable]
    private bool _isHandTracked = false;
    public bool isHandTracked { get { return _isHandTracked; } }

    [Header("Pose Stream")]

    [Tooltip("Follow Hand Point implements IStream<Pose>; It will stream data as long as "
          + "the component is enabled, the hand is tracked, and this option is enabled.")]
    public bool doPoseStream = true;

    [DisableIf("doPoseStream", isEqualTo: false)]
    public bool usePoseStreamOffset = false;

    [DisableIfAny("usePoseStreamOffset", "doPoseStream", areEqualTo: false)]
    public Transform poseStreamOffsetSource = null;

    [Disable]
    public Pose poseStreamOffset = Pose.identity;

    private bool _isStreamOpen = false;

    #endregion

    #region Unity Events

    private void OnValidate() {
      if (!usePoseStreamOffset) {
        poseStreamOffset = Pose.identity;
      }
      else if (poseStreamOffsetSource != null) {
        poseStreamOffset = poseStreamOffsetSource.ToWorldPose()
                             .From(transform.ToWorldPose());
      }
    }

    void OnEnable() {
      unsubscribeFrameCallback();
      subscribeFrameCallback();
    }

    void OnDisable() {
      unsubscribeFrameCallback();
    }

    private void Update() {
      if (!Application.isPlaying) {
        moveToAttachmentPointNow();
      }
    }

    #endregion

    #region On Frame Event

    private void onUpdateFrame(Frame frame) {
      if (frame == null) Debug.Log("Frame null");

      var hand = frame.Hands.Query()
                            .FirstOrDefault(h => h.IsLeft == (whichHand == Chirality.Left));

      bool shouldStream = false;
      Pose streamPose = Pose.identity;
      
      if (hand != null) {
        _isHandTracked = true;

        if (enabled && gameObject.activeInHierarchy) {
          Vector3 pointPosition; Quaternion pointRotation;
          AttachmentPointBehaviour.GetLeapHandPointData(hand, attachmentPoint,
                                                        out pointPosition,
                                                        out pointRotation);

          // Replace wrist rotation data with that from the palm for now.
          if (attachmentPoint == AttachmentPointFlags.Wrist) {
            Vector3 unusedPos;
            AttachmentPointBehaviour.GetLeapHandPointData(hand, AttachmentPointFlags.Palm,
                                                          out unusedPos,
                                                          out pointRotation);
          }

          this.transform.position = pointPosition;
          this.transform.rotation = pointRotation;

          streamPose = new Pose(pointPosition, pointRotation);
          var streamOffset = Pose.identity;
          if (usePoseStreamOffset && poseStreamOffsetSource != null) {
            streamOffset = poseStreamOffsetSource.transform.ToWorldPose()
                             .From(streamPose);
          }
          streamPose = streamPose.Then(streamOffset);
          shouldStream = true;
        }
      }
      else {
        _isHandTracked = false;
      }

      // Pose Stream data.
      shouldStream &= doPoseStream;
      shouldStream &= Application.isPlaying;
      shouldStream &= this.enabled && gameObject.activeInHierarchy;
      if (!shouldStream && _isStreamOpen) {
        OnClose();
        _isStreamOpen = false;
      }
      if (shouldStream && !_isStreamOpen) {
        OnOpen();
        _isStreamOpen = true;
      }
      if (shouldStream) {
        OnSend(streamPose);
      }
    }

    #endregion

    #region Frame Subscription

    private void unsubscribeFrameCallback() {
      if (_provider != null) {
        switch (_followMode) {
          case FollowMode.Update:
            Hands.Provider.OnUpdateFrame -= onUpdateFrame;
            break;
          case FollowMode.FixedUpdate:
            Hands.Provider.OnFixedFrame -= onUpdateFrame;
            break;
        }
      }
    }

    private void subscribeFrameCallback() {
      if (_provider != null) {
        switch (_followMode) {
          case FollowMode.Update:
            Hands.Provider.OnUpdateFrame += onUpdateFrame;
            break;
          case FollowMode.FixedUpdate:
            Hands.Provider.OnFixedFrame += onUpdateFrame;
            break;
        }
      }
    }

    #endregion

    #region IStream<Pose>
    
    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    #endregion

    #region Editor Methods

    private void moveToAttachmentPointNow() {
      onUpdateFrame(provider.CurrentFrame);
    }

    #endregion

  }

}

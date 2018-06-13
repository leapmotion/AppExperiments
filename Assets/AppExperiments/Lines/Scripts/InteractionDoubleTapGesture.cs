using Leap.Unity.Gestures;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Lines {

  public class InteractionDoubleTapGesture : OneHandedGesture, IRuntimeGizmoComponent {

    public InteractionTapGesture tapSource;

    public const float MIN_TAP_INTERVAL = 0.03F;
    public const float MAX_TAP_INTERVAL = 0.3F;

    private Vector3 _lastDoubleTapPos = Vector3.zero;
    /// <summary>
    /// Gets the last position that registered as a double-tap.
    /// </summary>
    public Vector3 lastDoubleTapPosition { get { return _lastDoubleTapPos; } }

    private InteractionBehaviour _lastDoubleTappedObject = null;
    /// <summary>
    /// Gets the last InteractionBehaviour that was double-tapped.
    /// </summary>
    public InteractionBehaviour lastDoubleTappedObject { get { return _lastDoubleTappedObject; } }

    public PointInteractionEvent OnDoubleTap = new PointInteractionEvent();

    private bool _timerEnabled = false;
    private float _timer = 0F;

    private bool _fireDoubleTap = false;

    protected override void Start() {
      base.Start();

      tapSource.OnGestureActivated += onTap;
    }

    private void onTap() {
      if (_timerEnabled) {
        if (_timer > MIN_TAP_INTERVAL && _timer < MAX_TAP_INTERVAL) {
          _fireDoubleTap = true;
          _timerEnabled = false;
          _timer = 0F;
        }
      }

      _timerEnabled = true;
      _timer = 0F;
    }

    protected override void Update() {
      base.Update();

      if (_timerEnabled) {
        _timer += Time.deltaTime;
      }
    }

    protected override bool ShouldGestureActivate(Hand hand) {
      if (_fireDoubleTap) {
        _fireDoubleTap = false;
        return true;
      }

      return false;
    }

    protected override bool ShouldGestureDeactivate(Hand hand, out Gesture.DeactivationReason? deactivationReason) {
      // One-shot gesture, so deactivate immediately.
      deactivationReason = DeactivationReason.FinishedGesture;
      return true;
    }

    protected override void WhenGestureActivated(Hand hand) {
      base.WhenGestureActivated(hand);

      _lastDoubleTapPos = tapSource.lastTapPosition;
      _lastDoubleTappedObject = tapSource.lastTappedObject;

      OnDoubleTap.Invoke(tapSource.interactionHand, tapSource.interactionHand.primaryHoveredObject as InteractionBehaviour, tapSource.lastTapPosition);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = Color.yellow;
      drawer.DrawWireSphere(_lastDoubleTapPos, 0.01F);
    }

  }

}
using Leap.Unity.Gestures;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.Examples.Lines {
  
  /// <summary>
  /// This one-shot gesture activates when it detects the hand has tapped an
  /// InteractionBehaviour. A tap is a sharp motion of the fingertip towards
  /// and away from an InteractionBehaviour that also touches the InteractionBehaviour
  /// at the tip of its motion curve.
  /// </summary>
  public class InteractionTapGesture : OneHandedGesture, IRuntimeGizmoComponent {

    private const int POSITION_BUFFER_WIDTH = 7;
    private const int MIN_ANALYZE_WIDTH = 2;
    private const int MAX_ANALYZE_WIDTH = 3;
    private const float TAP_HEAT_THRESHOLD = 0.8F;
    private const float MAX_TAP_DISTANCE = 0.020F;

    public InteractionHand interactionHand;

    private RingBuffer<Vector3> _indexPosBuffer = new RingBuffer<Vector3>(POSITION_BUFFER_WIDTH);

    //private float _minTimeBetweenTaps = 0.2F;
    //private float _timeSinceLastTap = 0F;

    private InteractionBehaviour _tappedObj = null;
    /// <summary>
    /// Gets the last InteractionBehaviour that was tapped.
    /// </summary>
    public InteractionBehaviour lastTappedObject { get { return _tappedObj; } }

    private Vector3 _lastTapPosition = Vector3.zero;
    /// <summary>
    /// Gets the last fingertip position reported as a tap.
    /// </summary>
    public Vector3 lastTapPosition { get { return _lastTapPosition; } }

    public PointInteractionEvent OnTap = new PointInteractionEvent();

    protected virtual void FixedUpdate() {
      if (interactionHand.isTracked && interactionHand.primaryHoveredObject != null) {
        Vector3 tipPos = interactionHand.leapHand.GetIndex().TipPosition.ToVector3();
        _indexPosBuffer.Add(tipPos);
      }
      else {
        _indexPosBuffer.Clear();
      }
    }

    private float[] _heat = new float[POSITION_BUFFER_WIDTH];
    private bool DidTapAir(out Vector3 tapPosition) {
      tapPosition = Vector3.zero;
      if (!_indexPosBuffer.IsFull) return false;


      for (int k = 0; k < _heat.Length; k++) {
        _heat[k] = 0F;
      }

      for (int width = MIN_ANALYZE_WIDTH; width <= MAX_ANALYZE_WIDTH; width++) {
        for (int k = 0; k + width * 2 < _heat.Length; k++) {
          Vector3 p0 = _indexPosBuffer.Get(k + 0);
          Vector3 p1 = _indexPosBuffer.Get(k + width);
          Vector3 p2 = _indexPosBuffer.Get(k + width * 2);

          // sharpness convolution -> heat
          Vector3 v01 = (p1 - p0);
          Vector3 v12 = (p2 - p1);

          // Vectors must be far enough away (prevents heat on idle finger)
          float minMag = 0.007F;
          float sharpness;
          if (v01.magnitude < minMag && v12.magnitude < minMag) {
            sharpness = 0F;
          }
          else {
            sharpness = Vector3.Dot(v01.normalized, v12.normalized).Map(0.6F, 1F, 1F, 0F);
          }

          float a = 0.5F;
          for (int w = 0; w < width * 2; w++) {
            _heat[k + w] += sharpness * a
                          * (w <= width * 2 ? (w / (width)) : (1 - ((w - width) / width))); // upside-down V, domain [0, width], range [0, 1]
          }
        }
      }

      int tapIndex = POSITION_BUFFER_WIDTH / 2;
      bool tapDetected = _heat[tapIndex] > TAP_HEAT_THRESHOLD;
      if (tapDetected) {
        tapPosition = _indexPosBuffer.Get(tapIndex);
      }

      return tapDetected;
    }

    protected override bool ShouldGestureActivate(Hand hand) {
      Vector3 tapPosition;
      bool didTapAir = DidTapAir(out tapPosition);

      if (didTapAir && interactionHand.primaryHoveredObject != null) {
        if (interactionHand.primaryHoveredObject.GetHoverDistance(tapPosition) <= MAX_TAP_DISTANCE) {
          _tappedObj = interactionHand.primaryHoveredObject as InteractionBehaviour;
          _lastTapPosition = tapPosition;

          OnTap.Invoke(interactionHand, _tappedObj, _lastTapPosition);

          return true;
        }
      }

      return false;
    }

    protected override bool ShouldGestureDeactivate(Hand hand, out DeactivationReason? deactivationReason) {
      // Tap gestures are one-shot gestures, so finish the gesture immediately.
      deactivationReason = DeactivationReason.FinishedGesture;
      return true;
    }

    private Color[] _colors = new Color[POSITION_BUFFER_WIDTH];

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      for (int i = 0; i < _heat.Length; i++) {
        _colors[i] = Color.Lerp(Color.cyan, Color.red, _heat[i]);
      }

      for (int i = 0; i < _indexPosBuffer.Count; i++) {
        drawer.color = _colors[i];
        float radius = 0.005F;
        if (_heat[i] > 0.8F) {
          radius = 0.01F;
        }
        drawer.DrawSphere(_indexPosBuffer.Get(i), radius);
      }

      drawer.color = Color.green;
      drawer.DrawWireSphere(_lastTapPosition, 0.008F);
    }
  }

}
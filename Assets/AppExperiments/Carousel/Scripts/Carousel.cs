using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Space;
using Leap.Unity.Animation;

public class Carousel : MonoBehaviour {

  public LeapProvider provider;
  public LeapSpace space;
  public PortalCard[] cards;
  public Transform resetBall;

  [Header("Card Motion")]
  public bool interactionEnabled = true;
  public float releaseVelocitySmoothing = 0.01f;
  public float positionWarmup = 0.05f;
  public float damping = 0.99f;
  public float twoHandedZThreshold = 0.05f;

  [Header("Layout")]
  public float spacing = 0.05f;

  public float _position;
  public float _velocity;

  private bool _needsAccum = true;
  private float _accumPos;

  private SmoothedFloat _smoothedVelocity;

  private Tween _resetTween;

  private Dictionary<int, float> _idToPos = new Dictionary<int, float>();

  private void Start() {
    _smoothedVelocity = new SmoothedFloat();
    _smoothedVelocity.delay = releaseVelocitySmoothing;
    _smoothedVelocity.reset = true;

    _resetTween = Tween.Persistent().Target(resetBall).LocalScale(0, resetBall.localScale.x).OverTime(1.0f).Smooth();
    resetBall.localScale = Vector3.zero;
  }

  void Update() {
    space.RecalculateTransformers();
    var transformer = space.transformer;

    List<int> pressedIds = new List<int>();

    float minDistance = float.MaxValue;
    float averageDelta = 0;
    int deltaCount = 0;

    if (interactionEnabled) {
      foreach (var hand in provider.CurrentFrame.Hands) {
        Vector3 tip = hand.GetIndex().Bone(Leap.Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();

        Vector3 rectTip = transformer.InverseTransformPoint(tip);

        minDistance = Mathf.Min(minDistance, rectTip.z);

        if (rectTip.z > 0) {
          float prevPos;
          if (_idToPos.TryGetValue(hand.Id, out prevPos)) {
            averageDelta += rectTip.x - prevPos;
            deltaCount++;
          }

          pressedIds.Add(hand.Id);
          _idToPos[hand.Id] = rectTip.x;
        }
      }
    }

    //If there are two hands and they are both farther than the threshold, do nothing
    if (provider.CurrentFrame.Hands.Count >= 2 && minDistance > -twoHandedZThreshold) {
      deltaCount = 0;
    }

    if (cards.Query().Any(c => c.isExpandedOrGrasped)) {
      deltaCount = 0;
      _resetTween.Play(Direction.Forward);
    } else {
      _resetTween.Play(Direction.Backward);
    }

    var idsToRemove = _idToPos.Query().Select(t => t.Key).Where(id => !pressedIds.Contains(id)).ToList();
    foreach (var idToRemove in idsToRemove) {
      _idToPos.Remove(idToRemove);
    }

    if (deltaCount > 0) {
      averageDelta /= deltaCount;

      if (_needsAccum) {
        _accumPos += averageDelta;
        if (Mathf.Abs(_accumPos) > positionWarmup) {
          _needsAccum = false;
        }
      } else {
        _position += averageDelta;
      }

      _smoothedVelocity.Update(averageDelta, Time.deltaTime);
      _velocity = _smoothedVelocity.value;
    } else {
      _accumPos = 0;
      _needsAccum = true;

      _position += _velocity;
      _velocity *= damping;
    }

    float totalWidth = cards.Length * spacing;

    for (int i = 0; i < cards.Length; i++) {
      float cardOffset = i * spacing;

      float cardPosition = _position + cardOffset;

      while (cardPosition < 0) {
        cardPosition += totalWidth;
      }

      float loopedPosition = cardPosition % totalWidth - totalWidth * 0.5f;

      var curvedPos = transformer.TransformPoint(new Vector3(loopedPosition, 0, 0));
      var curvedRot = transformer.TransformRotation(new Vector3(loopedPosition, 0, 0), Quaternion.identity);

      cards[i].transform.localPosition = curvedPos;
      cards[i].transform.localRotation = curvedRot;
    }
  }




}

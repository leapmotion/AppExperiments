using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Leap;
using Leap.Unity;

using Leap.Unity.Attributes;

public class LeapTurntable : MonoBehaviour
{

    [SerializeField]
    private LeapProvider _provider;

    [Header("Turntable Shape")]
    [SerializeField]
    [Tooltip("The local height of the upper section of the turntable.")]
    private float _tableHeight;
    

    [MinValue(0)]
    [SerializeField]
    [Tooltip("The radius of the upper section of the turntable.")]
    private float _tableRadius;

    [MinValue(0)]
    [SerializeField]
    [Tooltip("The length of the edge that connects the upper and lower sections of the turntable.")]
    private float _edgeLength;

    [Range(0, 90)]
    [SerializeField]
    [Tooltip("The angle the edge forms with the upper section of the turntable.")]
    private float _edgeAngle = 45;

    [Header("Turntable Motion")]
    [MinValue(0)]
    [SerializeField]
    [Tooltip("How much to scale the rotational motion by.  A value of 1 causes no extra scale.")]
    private float _rotationScale = 1.5f;

    [MinValue(0.00001f)]
    [SerializeField]
    [Tooltip("How much to smooth the velocity while the user is touching the turntable.")]
    private float _rotationSmoothing = 0.1f;

    [Range(0, 1)]
    [SerializeField]
    [Tooltip("The damping factor to use to damp the rotational velocity of the turntable.")]
    private float _rotationDamping = 0.95f;

    [MinValue(0)]
    [SerializeField]
    [Tooltip("The speed under which the turntable will stop completely.")]
    private float _minimumSpeed = 0.01f;

    private float _lowerLevelHeight
    {
        get
        {
            return _tableHeight - _edgeLength * Mathf.Sin(_edgeAngle * Mathf.Deg2Rad);
        }
    }

    private float _lowerLevelRadius
    {
        get
        {
            return _tableRadius + _edgeLength * Mathf.Cos(_edgeAngle * Mathf.Deg2Rad);
        }
    }

    //Maps a finger from a specific finger to the world tip position when it first entered the turntable
    private Dictionary<FingerPointKey, Vector3> _currTipPoints = new Dictionary<FingerPointKey, Vector3>();
    private Dictionary<FingerPointKey, Vector3> _prevTipPoints = new Dictionary<FingerPointKey, Vector3>();

    private SmoothedFloat _smoothedVelocity;
    private float _rotationalVelocity;

    private void Awake()
    {
        if (_provider == null)
        {
            _provider = Hands.Provider;
        }

        _smoothedVelocity = new SmoothedFloat();
        _smoothedVelocity.delay = _rotationSmoothing;
    }

    private void Update()
    {
        Utils.Swap(ref _currTipPoints, ref _prevTipPoints);

        _currTipPoints.Clear();
        foreach (var hand in _provider.CurrentFrame.Hands)
        {
            foreach (var finger in hand.Fingers)
            {
                var key = new FingerPointKey()
                {
                    handId = hand.Id,
                    fingerType = finger.Type
                };

                Vector3 worldTip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
                Vector3 localTip = transform.InverseTransformPoint(worldTip);

                if (isPointInsideTurntable(localTip))
                {
                    _currTipPoints[key] = worldTip;
                }
            }
        }

        float deltaAngleSum = 0;
        float deltaAngleWeight = 0;
        foreach (var pair in _currTipPoints)
        {
            Vector3 currWorldTip = pair.Value;
            Vector3 prevWorldTip;
            if (!_prevTipPoints.TryGetValue(pair.Key, out prevWorldTip))
            {
                return;
            }

            Vector3 currLocalTip = transform.InverseTransformPoint(currWorldTip);
            Vector3 prevLocalTip = transform.InverseTransformPoint(prevWorldTip);

            Vector2 planarPrevLocalTip = new Vector2(prevLocalTip.x, prevLocalTip.z);
            Vector2 planarCurrLocalTip = new Vector2(currLocalTip.x, currLocalTip.z);

            deltaAngleSum += Vector2.SignedAngle(planarPrevLocalTip, planarCurrLocalTip) * _rotationScale * -1.0f;
            deltaAngleWeight += 1.0f;
        }

        if (deltaAngleWeight > 0.0f)
        {
            float deltaAngle = deltaAngleSum / deltaAngleWeight;

            Vector3 localRotation = transform.localEulerAngles;
            localRotation.y += deltaAngle;
            transform.localEulerAngles = localRotation;

            _smoothedVelocity.Update(deltaAngle / Time.deltaTime, Time.deltaTime);
            _rotationalVelocity = _smoothedVelocity.value;
        }
        else
        {
            _rotationalVelocity = _rotationalVelocity * _rotationDamping;
            if (Mathf.Abs(_rotationalVelocity) < _minimumSpeed)
            {
                _rotationalVelocity = 0;
            }

            Vector3 localRotation = transform.localEulerAngles;
            localRotation.y += _rotationalVelocity * Time.deltaTime;
            transform.localEulerAngles = localRotation;
        }
    }

    private bool isPointInsideTurntable(Vector3 localPoint)
    {
        if (localPoint.y > _tableHeight)
        {
            return false;
        }

        float heightFactor = Mathf.Clamp01(Mathf.InverseLerp(_tableHeight, _lowerLevelHeight, localPoint.y));
        float effectiveRadius = Mathf.Lerp(_tableRadius, _lowerLevelRadius, heightFactor);

        float pointRadius = new Vector2(localPoint.x, localPoint.z).magnitude;
        if (pointRadius > effectiveRadius || pointRadius < effectiveRadius - 0.05f)
        {
            return false;
        }

        return true;
    }

    private struct FingerPointKey : IEquatable<FingerPointKey>
    {
        public int handId;
        public Finger.FingerType fingerType;

        public override int GetHashCode()
        {
            return new Hash() {
        handId,
        (int)fingerType
      };
        }

        public override bool Equals(object obj)
        {
            if (obj is FingerPointKey)
            {
                return Equals((FingerPointKey)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(FingerPointKey other)
        {
            return handId == other.handId &&
                   fingerType == other.fingerType;
        }
    }
}

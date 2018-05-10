using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class MakeKinematicWhileSwitching : MonoBehaviour {

    [SerializeField]
    [ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _tweenSwitch;
    public TweenSwitch tweenSwitch {
      get { return _tweenSwitch as TweenSwitch; }
    }

    public Rigidbody body;

    public ConfigurableJoint joint;

    private Vector3 _connectedAnchor = Vector3.zero;

    private Pose _localPoseFromAttachedBody = Pose.identity;
    public Vector3 _localRectPositionAdjust = Vector3.zero;
    public Vector3 _connectedAnchorAdjust = Vector3.zero;

    public InteractionBehaviour graspPullBar;
    public Transform distanceA;
    public Transform distanceB;
    public float closeSpring = 1200f;
    [SerializeField, Disable]
    private float _currentDistance;
    //private bool _stayOpen = false;

    public float stayOpenDistance = 0.20f;

    private void Awake() {
      if (joint != null) {
        _connectedAnchor = joint.connectedAnchor;

        _localPoseFromAttachedBody
          = (joint.connectedBody.GetMatrix().inverse
             * (this.body.GetMatrix()
                * Matrix4x4.Translate(_localRectPositionAdjust))).GetPose()
                                                                  * _connectedAnchor;
      }
    }

    private void Start() {
      if (tweenSwitch != null && body != null) {
        tweenSwitch.OnTweenLeftEnd += onLeftEnd;
        tweenSwitch.OnTweenReachedEnd += onReachedEnd;
      }
    }

    private void Update() {
      if (joint != null && body != null) {
        if (!body.isKinematic) {
          joint.connectedAnchor = _connectedAnchor + _connectedAnchorAdjust;
        }
        else {
          this.body.SetPose(joint.connectedBody.GetPose() * _localPoseFromAttachedBody);
        }

        _currentDistance = Vector3.Distance(distanceA.position, distanceB.position);
        if (distanceA != null && distanceB != null) {
          if (_currentDistance >= stayOpenDistance
              && !graspPullBar.isGrasped) {
            var drive = joint.xDrive;
            drive.positionSpring = 0f;
            joint.xDrive = drive;

            body.AddRelativeForce(Vector3.right * 4f * Time.deltaTime);
          }
          if (_currentDistance < stayOpenDistance
              && !graspPullBar.isGrasped) {
            var drive = joint.xDrive;
            drive.positionSpring = closeSpring;
            joint.xDrive = drive;
          }
        }
      }
    }

    //private bool _prevKinematicState = false;

    private void onLeftEnd() {
      body.isKinematic = true;
    }

    private void onReachedEnd() {
      body.isKinematic = false;

      if (joint != null) {
        joint.connectedAnchor = Vector3.zero;
        joint.connectedAnchor = _connectedAnchor;
      }
    }

  }

  public static class Extensions {
    public static Matrix4x4 GetMatrix(this Rigidbody body) {
      return Matrix4x4.TRS(
        body.position, body.rotation, body.transform.lossyScale);
    }

    public static Pose GetPose(this Matrix4x4 matrix) {
      return new Pose(matrix.MultiplyPoint3x4(Vector3.zero), matrix.rotation);
    }
  }

}

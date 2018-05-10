using System.Collections.Generic;
using Leap.Unity.Attributes;
using Leap.Unity.Space;
using UnityEngine;

namespace Leap.Unity.Interaction {

  /// <summary>
  /// A transform and optionally keyboard-controllable InteractionController
  /// implementation, useful for testing functionality in the editor.
  /// </summary>
  public class InteractionTestController : InteractionController {

    public override bool isTracked {
      get {
        return this.enabled && this.gameObject.activeInHierarchy;
      }
    }

    public override bool isBeingMoved {
      get { return velocity.magnitude > 0.001f; }
    }

    [SerializeField]
    private bool _isLeft = false;
    public override bool isLeft {
      get {
        return _isLeft;
      }
    }

    public override Vector3 position {
      get {
        return this.transform.position;
      }
    }

    public override Quaternion rotation {
      get {
        return this.transform.rotation;
      }
    }

    public override Vector3 velocity {
      get {
        if (!_lastTwoPositions.IsFull) {
          return Vector3.zero;
        }
        else {
          return _lastTwoPositions.Delta();
        }
      }
    }

    public override ControllerType controllerType {
      get {
        return ControllerType.XRController;
      }
    }

    public override InteractionHand intHand {
      get {
        return null;
      }
    }

    public override Vector3 hoverPoint {
      get {
        return this.transform.position;
      }
    }

    protected override void OnEnable() {
      base.OnEnable();

      _lastTwoPositions.Clear();
    }

    private ContactBone _singleContactBone = null;
    public ContactBone singleContactBone {
      get {
        if (_singleContactBone == null && Application.isPlaying) {
          GameObject boneObj = new GameObject();
          boneObj.transform.parent = this.transform;
          var rigidbody = boneObj.AddComponent<Rigidbody>();
          var collider = boneObj.AddComponent<SphereCollider>();
          collider.radius = 0.0075f;
          _singleContactBone = boneObj.AddComponent<ContactBone>();
          _singleContactBone.rigidbody = rigidbody;
          _singleContactBone.rigidbody.useGravity = false;
          _singleContactBone.collider = collider;
          _singleContactBone.interactionController = this;
        }
        return _singleContactBone;
      }
    }

    private ContactBone[] _contactBones = null;
    public override ContactBone[] contactBones {
      get {
        if (_contactBones == null && Application.isPlaying) {
          _contactBones = new ContactBone[1];
          _contactBones[0] = singleContactBone;
        }
        return _contactBones;
      }
    }

    private List<Vector3> _graspManipulatorPoints = null;
    public override List<Vector3> graspManipulatorPoints {
      get {
        if (_graspManipulatorPoints == null) {
          _graspManipulatorPoints = new List<Vector3>();
          _graspManipulatorPoints.Add(this.transform.position);
        }
        if (_graspManipulatorPoints.Count != 1) {
          _graspManipulatorPoints.Clear();
          _graspManipulatorPoints.Add(this.transform.position);
        }
        _graspManipulatorPoints[0] = this.transform.position;
        return _graspManipulatorPoints;
      }
    }

    private List<Transform> _backingPrimaryHoverPoints = null;
    protected override List<Transform> _primaryHoverPoints {
      get {
        if (_backingPrimaryHoverPoints == null) {
          _backingPrimaryHoverPoints = new List<Transform>();
          _backingPrimaryHoverPoints.Add(this.transform);
        }
        return _backingPrimaryHoverPoints;
      }
    }

    protected override GameObject contactBoneParent {
      get {
        return this.gameObject;
      }
    }

    public override Vector3 GetGraspPoint() {
      return this.transform.position;
    }

    [Header("Keyboard Control")]
    public bool enableKeyboardControl = false;

    [DisableIf("enableKeyboardControl", isEqualTo: false)]
    public KeyCode graspKey = KeyCode.G;
    [DisableIf("enableKeyboardControl", isEqualTo: false)]
    public KeyCode releaseKey = KeyCode.R;

    // TODO: refactor into a physics utility.
    private static Collider[] s_hitResultsBuffer = new Collider[32];
    public static Collider OverlapSphereGetClosest(SphereCollider sphereCollider,
                                                   int layerMask) {
      Vector3 colliderPos = sphereCollider.transform
                                          .TransformPoint(sphereCollider.center);

      int numHit = -1;
      do {
        numHit = Physics.OverlapSphereNonAlloc(
          colliderPos,
          sphereCollider.transform.TransformVector(Vector3.right
                                                   * sphereCollider.radius).magnitude,
          s_hitResultsBuffer,
          layerMask);
        if (numHit == s_hitResultsBuffer.Length) {
          Collider[] largerBuffer = new Collider[s_hitResultsBuffer.Length * 2];
          Utils.Swap(ref largerBuffer, ref s_hitResultsBuffer);
          numHit = -1;
        }
      } while (numHit == -1);

      float closestSqrDist = float.PositiveInfinity;
      Collider closestCollider = null;
      for (int i = 0; i < numHit; i++) {
        var testCollider = s_hitResultsBuffer[i];
        var body = testCollider.attachedRigidbody;
        if (body == null) continue;
        else {
          var testSqrDist = (colliderPos - body.position).sqrMagnitude;
          if (closestCollider == null || testSqrDist < closestSqrDist) {
            closestCollider = testCollider;
            closestSqrDist = testSqrDist;
          }
        }
      }

      return closestCollider;
    }

    protected override bool checkShouldGrasp(out IInteractionBehaviour objectToGrasp) {
      var toGrasp = OverlapSphereGetClosest(singleContactBone.collider as SphereCollider,
                                            manager.GetInteractionLayerMask());
      if (toGrasp == null) {
        objectToGrasp = null;
        return false;
      }

      var intObj = toGrasp.attachedRigidbody.GetComponent<InteractionBehaviour>();
      if (intObj != null && !intObj.ignoreGrasping
          &&
          enableKeyboardControl && Input.GetKey(graspKey)) {
        
        objectToGrasp = intObj;
        return true;
      }
      else {
        objectToGrasp = null;
        return false;
      }
    }

    protected override bool checkShouldGraspAtemporal(IInteractionBehaviour intObj) {
      var sphere = singleContactBone.collider as SphereCollider;
      return intObj.GetHoverDistance(sphere.transform.TransformPoint(sphere.center))
        < sphere.transform.TransformVector(Vector3.right).magnitude * sphere.radius;
    }

    protected override bool checkShouldRelease(out IInteractionBehaviour objectToRelease) {
      if (enableKeyboardControl && Input.GetKeyDown(releaseKey)) {
        objectToRelease = graspedObject;
        return true;
      }
      else {
        objectToRelease = null;
        return false;
      }
    }

    protected override void fixedUpdateGraspingState() { }

    protected override void getColliderBoneTargetPositionRotation(int contactBoneIndex,
        out Vector3 targetPosition, out Quaternion targetRotation) {
      targetPosition = this.transform.position;
      targetRotation = this.transform.rotation;
    }

    protected override bool initContact() {
      return true;
    }

    protected override void onObjectUnregistered(IInteractionBehaviour intObj) {
      if (graspedObject == intObj) {
        Debug.LogError("Object was unregistered while grasped by the "
          + "InteractionTestController: " + intObj.name);
      }
    }

    private DeltaBuffer _lastTwoPositions = new DeltaBuffer(2);

    protected override void fixedUpdateController() {
      base.fixedUpdateController();

      // Remember the two last fixed update positions.
      _lastTwoPositions.Add(singleContactBone.rigidbody.position, Time.fixedTime);

      // Reset the unwarping local pose.
      //_unwarpingLocalPose = Pose.identity;
    }

    /// <summary>
    /// Usually this is the identity, but when dealing with a curved space, you want to
    /// apply this pose offset to any hover points or primary hover points.
    /// </summary>
    //private Pose _unwarpingLocalPose = Pose.identity;
    protected override void unwarpColliders(Transform primaryHoverPoint,
                                            ISpaceComponent warpedSpaceElement) {


      // TODO: support warped spaces with the interaction test controller

      //Vector3    unwarpedPos;
      //Quaternion unwarpedRot;
      //warpedSpaceElement.anchor.transformer.WorldSpaceUnwarp(primaryHoverPoint.position,
      //                                                       primaryHoverPoint.rotation,
      //                                                       out unwarpedPos,
      //                                                       out unwarpedRot);

      //_unwarpingLocalPose = this.transform.ToPose().inverse * new Pose(unwarpedPos,
      //                                                                 unwarpedRot);
    }
  }

}
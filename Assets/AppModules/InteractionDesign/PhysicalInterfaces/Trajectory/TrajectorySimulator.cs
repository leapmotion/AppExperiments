using Leap.Unity.Attributes;
using Leap.Unity.Layout;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation {

  public class TrajectorySimulator : MonoBehaviour,
                                     IRuntimeGizmoComponent,
                                     IKinematicStateProvider {

    #region Inspector

    [Header("Non-Simulation")]

    [Tooltip("The TrajectorySimulator watches this transform to match its velocity and "
           + "angular velocity when the simulation begins. If null, the simulator will "
           + "watch its own transform.")]
    public Transform watchTransform;
    private Transform effWatchTransform {
      get {
        if (watchTransform == null) {
          return this.transform;
        }
        return watchTransform;
      }
    }

    [Header("Simulation")]

    [SerializeField, OnEditorChange("simulating")]
    [RunTimeOnly]
    [Tooltip("The trajectory simulation component provides a stoppable and restartable "
           + "basic physics simulation of a body with an initial velocity experiencing "
           + "a constant acceleration (gravity by default) and, optionally, drag. "
           + "This component merely integrates a virtual point mass; it does not detect "
           + "collisions. When the simulation is started, by default, the point mass "
           + "will inherit the current velocity of the object, regardless of whether "
           + "this object has a Rigidbody.")]
    private bool _isSimulating = false;
    public bool isSimulating {
      get {
        return _isSimulating;
      }
      set {
        if (value) { StartSimulating(); }
        else { StopSimulating(); }
      }
    }

    public bool simulateRotation = true;

    [Tooltip("If set, you can specify a custom acceleration value for the simulation's "
           + "gravity.")]
    public bool applyGravity = false;

    [DisableIf("applyGravity", isEqualTo: false)]
    public Vector3 gravityAcceleration = new Vector3(0f, -9.81f, 0f);

    public bool accelerateTowardsTransform;

    [DisableIf("accelerateTowardsTransform", isEqualTo: false)]
    public Transform targetTransform;

    public bool accelerateTowardsPose;

    [DisableIf("accelerateTowardsPose", isEqualTo: false)]
    [SerializeField, ImplementsInterface(typeof(IPoseProvider))]
    private MonoBehaviour _targetPoseProvider;
    public IPoseProvider targetPoseProvider {
      get { return _targetPoseProvider as IPoseProvider; }
    }

    [MinValue(0f)]
    [DisableIfAll("accelerateTowardsTransform", "accelerateTowardsPose", areEqualTo: false)]
    public float accelerationForce = 1f;

    [MinValue(0f)]
    public float drag = 0f;

    [MinValue(0f)]
    public float angularDrag = 0.01f;

    [Header("Debug")]

    [SerializeField]
    private bool _drawRuntimeGizmos = false;

    #endregion

    #region Unity Events

    void Reset() {
      gravityAcceleration = Physics.gravity;
    }

    void Start() {

    }

    void Update() {
      if (_isSimulating) {
        updateSimulation();
      }
      else {
        updateNonSimulation();
      }
    }

    #endregion

    #region Public API

    public Vector3 position { get { return _position; } }
    public Vector3 velocity { get { return _velocity; } }

    public KinematicState GetKinematicState() {
      return new KinematicState(new Pose(_position, _rotation),
                                new Movement(_velocity, _angularVelocity));
    }

    public void StartSimulating() {
      _isSimulating = true;

      if (accelerateTowardsPose && targetPoseProvider != null) {
        _cachedTargetPose = targetPoseProvider.GetPose();
      }
    }

    public void StopSimulating() {
      _isSimulating = false;
    }

    public Vector3 GetSimulatedPosition() {
      return _position;
    }

    public Quaternion GetSimulatedRotation() {
      return _rotation;
    }

    public void SetSimulatedRotation(Quaternion rotation) {
      _rotation = rotation;
    }

    public Pose GetSimulatedPose() {
      return new Pose(_position, _rotation);
    }

    public void SetMovement(Movement movement) {
      _velocity = movement.velocity;
      _angularVelocity = movement.angularVelocity;
    }

    public void ResetNonSimulationTracking() {
      _hasPositionLastUpdate = false;
      _velocity = Vector3.zero;
      _angularVelocity = Vector3.zero;
    }

    #endregion

    #region Simulation

    private Rigidbody _rigidbody;
    private Vector3 _positionLastUpdate;
    private Quaternion _rotationLastUpdate;
    private bool _hasPositionLastUpdate = false;

    private Vector3 _position = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;
    private Quaternion _rotation = Quaternion.identity;
    private Vector3 _angularVelocity = Vector3.zero;

    private Pose _cachedTargetPose = Pose.identity;

    private void initSimulation() {
      _rigidbody = GetComponent<Rigidbody>();
      _hasPositionLastUpdate = false;
    }

    /// <summary>
    /// While simulating, we just apply forces and integrate as usual.
    /// </summary>
    private void updateSimulation() {
      Vector3 dragAccel = -_velocity.normalized * _velocity.sqrMagnitude * drag;
      Vector3 gravAccel = applyGravity ? gravityAcceleration : Vector3.zero;

      if (accelerateTowardsTransform && targetTransform != null) {
        var accelTowardsTarget = targetTransform.position.From(_position)
                                 * accelerationForce;
        gravAccel += accelTowardsTarget;
      }
      if (accelerateTowardsPose && targetPoseProvider != null) {
        var accelTowardsPoseTarget = _cachedTargetPose.position.From(_position)
                                     * accelerationForce;
        gravAccel += accelTowardsPoseTarget;
      }

      _velocity += (dragAccel + gravAccel) * Time.deltaTime;

      _position += _velocity * Time.deltaTime;

      // Rotation
      if (simulateRotation) {
        Vector3 angDragAccel = -_angularVelocity.normalized * _angularVelocity.sqrMagnitude * angularDrag;

        _angularVelocity += angDragAccel * Time.deltaTime;

        _rotation = Quaternion.AngleAxis(_angularVelocity.magnitude * Time.deltaTime, _angularVelocity.normalized) * _rotation;
      }
    }

    /// <summary>
    /// While the object is NOT simulating, we'd still like to know its current velocity
    /// and position so that the simulation begins in a sensible state.
    /// </summary>
    private void updateNonSimulation() {
      _position = effWatchTransform.position;
      _rotation = effWatchTransform.rotation;

      if (_rigidbody != null) {
        _velocity = _rigidbody.velocity;
        _angularVelocity = _rigidbody.angularVelocity;
      }
      else if (_hasPositionLastUpdate) {
        _velocity = (_position - _positionLastUpdate) / Time.deltaTime;
        _angularVelocity = (Quaternion.Inverse(_rotationLastUpdate) * _rotation).ToAngleAxisVector() / Time.deltaTime;
      }

      _positionLastUpdate = _position;
      _rotationLastUpdate = _rotation;
      _hasPositionLastUpdate = true;
    }

    #endregion

    #region Runtime Gizmos

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (_isSimulating && _drawRuntimeGizmos) {
        drawer.color = Color.red;

        drawer.PushMatrix();
        drawer.matrix = Matrix4x4.TRS(_position, _rotation, Vector3.one);

        drawer.DrawWireCube(Vector3.zero, Vector3.one * 0.05f);

        drawer.color = Color.red;
        drawer.DrawLine(Vector3.zero, Vector3.right * 0.10f);
        drawer.color = Color.green;
        drawer.DrawLine(Vector3.zero, Vector3.up * 0.10f);
        drawer.color = Color.blue;
        drawer.DrawLine(Vector3.zero, Vector3.forward * 0.10f);

        drawer.PopMatrix();
      }
    }

    #endregion

  }

}

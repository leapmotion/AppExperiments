using Leap.Unity.Attributes;
using Leap.Unity.Geometry;
using Leap.Unity.Infix;
using Leap.Unity.Interaction;
using Leap.Unity.RuntimeGizmos;
using System;
using UnityEngine;

namespace Leap.Unity {

  //using UnityRect = UnityEngine.Rect;
  using Rect = Geometry.Rect;
  using Plane = Leap.Unity.Geometry.Plane;

  public class HingeElement : MonoBehaviour, IRuntimeGizmoComponent {

    #region Inspector

    public InteractionBehaviour intObj;

    [Header("Hinge Joint")]

    [SerializeField]
    private Transform _hingeTransform;
    public Transform hingeTransform {
      get {
        if (_hingeTransform == null) return this.transform;
        return _hingeTransform;
      }
    }

    [SerializeField]
    private LocalRect _localButtonSurface;
    public Rect buttonSurface {
      get {
        return _localButtonSurface.With(this.transform);
      }
    }

    public Vector2 minMaxHingeAngle = new Vector2(-30f, 80f);

    [Header("Hinge Spring")]

    [MinValue(0f)]
    public float restSpringForce = 300f;
    [MinValue(0f)]
    public float restSpringFriction = 10f;
    [MinValue(0f)]
    public float restSpringDrag = 0.07f;

    [Header("Debug")]
    public bool drawDebug = false;

    #endregion

    #region State

    public Plane currentPlane {
      get {
        return new Plane(-hingeTransform.forward, hingeTransform.position);
      }
    }

    public Vector3 hingePosition {
      get {
        return hingeTransform.position;
      }
    }

    private Vector3 _hingeLocalAxis = Vector3.up;
    public Vector3 hingeAxis {
      get {
        return hingeTransform.rotation * _hingeLocalAxis;
      }
    }

    private Vector3 _hingeLocalZeroAngleDir = Vector3.right;
    public Vector3 hingeRestAngleDir {
      get {
        return hingeTransform.rotation * _hingeLocalZeroAngleDir;
      }
    }

    private Vector3 _elementLocalActivationAxis = Vector3.forward;
    /// <summary>
    /// The (current) direction of activation ("pressing") of the hinging element,
    /// somewhere on the plane about the hinge and orthogonal to the element direction.
    /// </summary>
    public Vector3 activationDir {
      get {
        return _hingeTransform.rotation
               * Quaternion.AngleAxis(_elementAngle, _hingeLocalAxis)
               * _elementLocalActivationAxis;
      }
    }

    private Vector3 _elementLocalExtensionAxis = Vector3.right;
    /// <summary>
    /// The direction of the hinging element, somewhere on the plane about the hinge.
    /// If the element were a flag, this is the direction the flag points. This direction
    /// is up-to-date with the current _elementAngle value.
    /// </summary>
    public Vector3 elementDir {
      get {
        return _hingeTransform.rotation
               * Quaternion.AngleAxis(_elementAngle, _hingeLocalAxis)
               * _elementLocalExtensionAxis;
      }
    }

    /// <summary>
    /// The angle of the hinging element from the rest angle of the hinge.
    /// 
    /// At the beginning of each hinge Update, this angle is recalculated based on the
    /// element transform and the hinge transform. The update step applies resting and
    /// depenetration forces directly to this angle, then the angle is used to place the
    /// element Transform relative to the hinge Transform.
    /// </summary>
    private float _elementAngle = 0f;
    /// <summary>
    /// The angle of the hinging element from the rest angle of the hinge. (Read only.)
    /// </summary>
    public float angle {
      get {
        return _elementAngle;
      }
    }

    /// <summary>
    /// The hinge-space resting pose of the element when the hinge is at its resting
    /// angle.
    /// 
    /// Multiply the hinge transform by this pose to get the world-space resting pose of
    /// the hinging element. This pose is calculated in Start.
    /// </summary>
    private Pose _elementRestPose_hinge;

    private Vector3? _interactorPos = null;
    /// <summary>
    /// Gets whether this hinge element is currently being hovered over by an
    /// interactor, such as a fingertip.
    /// </summary>
    public bool isHovered {
      get {
        return _interactorPos.HasValue;
      }
    }

    /// <summary>
    /// Public-facing state for other elements to know if this hinge element is currently
    /// interacting with (touching) an interactor (e.g. fingertip).
    /// </summary>
    private bool _isInteracting = false;
    public bool isInteracting {
      get {
        return _isInteracting;
      }
    }

    private Vector3? _lastInteractorPos = null;

    private float _interactorRadius = 0.0051f;
    
    private float _restCorrectionVelocity = 0f;

    // Debug data.
    private RingBuffer<LocalSegment3?> _interactorMotionsBuffer
      = new RingBuffer<LocalSegment3?>(8);

    private RingBuffer<bool> _wasMotionIntersectingBuffer
      = new RingBuffer<bool>(8);

    #endregion

    #region Unity Events

    private void Reset() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();

      if (_localButtonSurface.radii.x == 0 && _localButtonSurface.radii.y == 0) {
        _localButtonSurface = new LocalRect(Vector3.zero, Vector2.one * 0.05f);
      }
    }
    private void OnValidate() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }

    private void Start() {
      _elementRestPose_hinge = hingeTransform.ToPose().inverse * this.transform.ToPose();
    }

    private void Update() {

      // Calculate information about the current angle of the hinge element relative to
      // the hinge resting direction.
      _elementAngle = Vector3.SignedAngle(hingeRestAngleDir,
                                          this.transform.rotation * _elementLocalExtensionAxis,
                                          hingeAxis);

      // Apply resting forces to the elementAngle.
      float? restingAngleCorrection = null;
      int restwardPolarity = -_elementAngle > 0 ? 1 : -1;
      if (_elementAngle != 0) {
        // Rotate the element towards its resting angle.
        var restForce = _elementAngle * -1f * restSpringForce;

        _restCorrectionVelocity += restForce * Time.deltaTime;

        var friction
          = -_restCorrectionVelocity * restSpringFriction;
        _restCorrectionVelocity += friction * Time.deltaTime;

        var dragSign = _restCorrectionVelocity > 0 ? -1 : 1;
        var drag = dragSign * _restCorrectionVelocity.Squared() * restSpringDrag;
        _restCorrectionVelocity += drag * Time.deltaTime;

        if (_restCorrectionVelocity < 0.2f) {
          restwardPolarity = 0;
        }

        restingAngleCorrection = _restCorrectionVelocity * Time.deltaTime;

        _elementAngle += restingAngleCorrection.Value;
      }
      else {
        restwardPolarity = 0;
      }

      // Update interactors.
      LocalSegment3? interactorMotion = null;
      {
        if (_interactorPos.HasValue && _lastInteractorPos.HasValue) {
          _lastInteractorPos = Vector3.Lerp(_lastInteractorPos.Value,
                                            _interactorPos.Value,
                                            20f * Time.deltaTime);
        }
        else {
          _lastInteractorPos = _interactorPos;
        }

        if (restingAngleCorrection.HasValue && _interactorPos.HasValue) {
          // Shift the last interactor position by the translation of it produced by
          // the resting angle correct. This is akin to computing interactor sweeps in
          // hinge-element-local space, where the correction of the hinge element to rest
          // accounts for a sweep of the interactor position across its motion.
          var rotatedPos = _lastInteractorPos.Value
                             .RotatedAround(hingeTransform.position,
                                            ((float)restingAngleCorrection.Value) * 0.05f,
                                            hingeAxis);
          var toRotated = rotatedPos - _lastInteractorPos.Value;
          toRotated = toRotated.RotatedBy(Quaternion.AngleAxis(-30f, hingeAxis));
          _lastInteractorPos = _lastInteractorPos + toRotated * 20f;
        }

        _interactorPos = null;
        if (intObj.isPrimaryHovered) {
          _interactorPos = intObj.primaryHoveringControllerPoint;
        }
        if (_lastInteractorPos.HasValue && _interactorPos.HasValue) {
          var lastToCurrentDir
            = (_interactorPos.Value - _lastInteractorPos.Value).normalized;

          interactorMotion
            = new LocalSegment3(
              _lastInteractorPos.Value - lastToCurrentDir * _interactorRadius * 0.01f,
              _interactorPos.Value);
        }

        // Update debug data.
        _interactorMotionsBuffer.Add(interactorMotion);
      }

      // Update interactor collision.
      var isSweptInteractorColliding = false;
      var isCurrInteractorColliding = false;
      var currInteractor = (Sphere?)null;
      {
        if (interactorMotion.HasValue) {
          var segment = interactorMotion.Value;
          var motionSqrDist = segment.Intersect(buttonSurface);

          if (motionSqrDist < _interactorRadius * _interactorRadius) {
            isSweptInteractorColliding = true;
          }
        }
        if (_interactorPos.HasValue) {
          currInteractor = new Sphere(_interactorPos.Value, _interactorRadius);
          isCurrInteractorColliding = currInteractor.Value.Overlaps(buttonSurface);
        }
        _wasMotionIntersectingBuffer.Add(isSweptInteractorColliding);
      }

      // Apply depenetration forces to the elementAngle.
      if (currInteractor.HasValue) {
        var interactor = new Sphere(_interactorPos.Value, _interactorRadius);

        // Determine whether or not we should attempt to hinge the button to depenetrate
        // from the interacting sphere geometry.
        bool shouldDepenetrate = false;
        if (isCurrInteractorColliding || isSweptInteractorColliding) {
          shouldDepenetrate = true;
        }

        // Determine which way the element should depenetrate itself from the interacting
        // sphere -- here, we simply depenetrate to the closest side.
        var latestPos_button = this.transform.worldToLocalMatrix
                                   .MultiplyPoint3x4(_interactorPos.Value);
        var depenetrationPolarity
          = Vector3.Dot(latestPos_button, _elementLocalActivationAxis) > 0 ? 1 : -1;
        // However, if the button velocity is large enough, we actually should use the
        // sign of the curl from its velocity vector about the hinge axis.
        if (_lastInteractorPos.HasValue) {
          var angleToLastPos = Vector3.SignedAngle(
            _lastInteractorPos.Value  - _hingeTransform.position,
            _hingeTransform.rotation
              * Quaternion.AngleAxis((float)_elementAngle, _hingeLocalAxis)
              * _elementLocalExtensionAxis,
            hingeAxis);
          depenetrationPolarity = angleToLastPos > 0 ? 1 : -1;

          var velocity = (_lastInteractorPos.Value - _interactorPos.Value).magnitude
                          / Time.deltaTime;

          if (depenetrationPolarity == restwardPolarity && velocity < 1f) {
            shouldDepenetrate = false;
          }
        }

        // TODO: Uncomment this and test with an applicable hinge element (with the 
        // element extending over the hinge, notice how there is another sign problem,
        // where the facing of the element also flips).
        //// If the hinging element surface extends onto the other side of the hinge,
        //// the correct depenetration direction gets flipped when it is being pushed from
        //// that side.
        //if (Vector3.Dot(latestPos_button, _elementLocalExtensionAxis) < 0) {
        //  _depenetrationPolarity = -_depenetrationPolarity;
        //}

        // If we should depenetrate, we were "interacting" with the button. This is
        // publicly-exposed, read-only state.
        _isInteracting = shouldDepenetrate;

        // Depenetrate the button by moving it on its hinge.
        if (shouldDepenetrate) {
          var depenetratingAngle
            = performTangencyCalculation(interactor, depenetrationPolarity);
          _elementAngle += depenetratingAngle;

          _restCorrectionVelocity = 0f;
        }
      }
      else {
        _isInteracting = false;
      }

      // Update the transform to reflect the latest angle.
      {
        // Get the rotation about the hinge axis from the updated hinging element angle.
        var elementRotation
          = Quaternion.AngleAxis(_elementAngle, _hingeLocalAxis);

        // Start with the current hinge pose, rotate it with the element rotation, then
        // use the rotated hinge pose to recalculate where the hinging element should be
        // relative to it, using the rigid delta-pose that is calculated on Start.
        var hingeRotation = _hingeTransform.rotation * elementRotation;
        var rotatedHingePose = _hingeTransform.ToPose().WithRotation(hingeRotation);
        this.transform.SetPose(rotatedHingePose * _elementRestPose_hinge);
      }
    }

    private float performTangencyCalculation(Sphere interactor, int depenetrationPolarity) {
      // To reach tangency:
      // Aim at the sphere center, then rotate based on an arcsecant angle calculated
      // from the line segment to the sphere center (and sphere radius).
      // (There are two possible tangencies to the sphere, so we'll have to pick a
      // sign.)

      // The hinge plane is the plane defined by the hinge position with a normal
      // defined by the axis about which the hinge rotates.
      // This will produce an exact solution only if the sphere center is actually
      // floating over the hinging element, which, unlike an infinite plane, has a
      // bounded extent along the hinge axis.
      // This is the sphere position projected on and in the local space of the hinge 
      // plane.
      var interactorPos_hingePlane
            = Vector3.ProjectOnPlane((interactor.position - hingePosition),
                                      hingeAxis);

      var hingeToCenterAngle
            = Vector3.SignedAngle(interactorPos_hingePlane,
                                  elementDir,
                                  hingeAxis);

      // Accumulate angle information, to be clamped later.
      var aimAtSphereCenterAngle = -hingeToCenterAngle;

      // Tangency step. Use distance from sphere and sphere radius.
      float d = interactorPos_hingePlane.magnitude;
      float r = interactor.radius;

      // There is only a solution if the sphere (with its center projected onto the 
      // hinge plane) is not overlapping the hinge origin.
      if (d / r > 1f) {
        // Use an identity for arcsec: arcsec(1/x) = arccos(x)
        Func<float, float> arcsec = (x) => { return Mathf.Acos(1f / x); };
        var theta = arcsec(d / r);

        var hingeToTangencyAngle = (90f - theta * Mathf.Rad2Deg)
                                    * depenetrationPolarity;

        // Calculate final angle along the hinge.
        float depenetratingAngle = aimAtSphereCenterAngle + hingeToTangencyAngle;

        // Clamp this angle to based on the bounds of the hinge.
        depenetratingAngle = Mathf.Clamp((_elementAngle + depenetratingAngle),
                                         -minMaxHingeAngle.y, -minMaxHingeAngle.x)
                             - _elementAngle;

        // Soften the depenetrating angle so that it depenetrates over multiple
        // frames.
        var depenetrationStrength = (depenetratingAngle).Map(0f, 90f, 40f, 80f);
        depenetratingAngle *= (depenetrationStrength * Time.deltaTime).Clamped01();

        return depenetratingAngle;
      }

      // No depenetration can be calculated.
      return 0f;
    }

    #endregion

    #region Runtime Gizmos

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!drawDebug || !this.enabled || !this.gameObject.activeInHierarchy) return;

      drawHingeGizmo(drawer);

      //drawBeginPressVolume(drawer); // TODO: DELETEME

      drawSweepGizmos(drawer);

      drawButtonSurface(drawer);

      drawLatestKnownInteractingSphere(drawer);
    }

    private void drawSweepGizmos(RuntimeGizmoDrawer drawer) {
      for (int i = 0; i < _interactorMotionsBuffer.Count; i++) {
        var sweep = _interactorMotionsBuffer[i];
        var wasSweepIntersecting = _wasMotionIntersectingBuffer[i];

        if (sweep.HasValue) {
          if (wasSweepIntersecting) {
            drawer.color = LeapColor.red;
          }
          else {
            drawer.color = LeapColor.amber;
          }
          drawer.DrawLine(sweep.Value.a, sweep.Value.b);
        }
      }
    }

    private void drawHingeGizmo(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.purple;
      var currentPlane = this.currentPlane;
      float planeCircleRadius = 0.005f;
      for (int i = 0; i < 4; i++) {
        drawer.DrawWireArc(hingePosition, currentPlane.normal,
                           currentPlane.normal.Vec().Perpendicular(),
                           planeCircleRadius + i * -0.001f,
                           1.0f);
      }
    }

    private void drawButtonSurface(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.cerulean;
      buttonSurface.DrawRuntimeGizmos(drawer);

      //drawer.color = LeapColor.white;
      //hingedSurface.DrawRuntimeGizmos(drawer);
    }

    // TODO: DELETEME
    //private void drawBeginPressVolume(RuntimeGizmoDrawer drawer) {
    //  drawer.color = LeapColor.lime.Lerp(Color.white, 0.3f);

    //  entryVolume.DrawRuntimeGizmos(drawer);
    //}

    private void drawLatestKnownInteractingSphere(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.lime;

      if (!_wasMotionIntersectingBuffer.IsEmpty) {
        if (_wasMotionIntersectingBuffer.GetLatest()) {
          drawer.color = LeapColor.coral;
        }
      }

      if (_interactorPos.HasValue) {
        new Sphere(_interactorPos.Value, _interactorRadius).DrawRuntimeGizmos(drawer);
      }

      if (_lastInteractorPos.HasValue) {
        new Sphere(_lastInteractorPos.Value, _interactorRadius).DrawRuntimeGizmos(drawer);
      }
    }

    #endregion

  }

  #region Extensions

  public static class NullableExtensions {

    public static U UnwrapDo<T, U>(this T? nullable, Func<T, U> ifHasValue, U defaultIfNull)
                      where T : struct {
      if (nullable.HasValue) {
        return ifHasValue(nullable.Value);
      }
      return defaultIfNull;
    }

    //public static U UnwrapDo<T, U>(this T refValue, Func<T, U> ifNonNull, U defaultIfNull)
    //                  where T : class {
    //  if (refValue != null) {
    //    return ifNonNull(refValue);
    //  }
    //  return defaultIfNull;
    //}

  }

  #endregion

}

using Leap.Unity.Attributes;
using Leap.Unity.ColorPalettes;
using Leap.Unity.Interaction;
using Leap.Unity.Layout;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Portals;
using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Launcher {

  using IntObj = InteractionBehaviour;

  public class FingerSlidePanel : MonoBehaviour, IRuntimeGizmoComponent {

    public Portal portalObj;

    public Transform slideableObjectsRoot;

    [Header("Scrolling Surface")]

    public float thickness = 0.02f;

    public float depthOffset = -0.01f;

    [Header("Deadzone")]

    public float deadzoneWidth = 0.06f;

    public float minDeadzoneWidth = 0.03f;

    [Header("Momentum")]

    public float momentumDecayFriction = 7f;

    [Header("Scroll Bounds")]

    public bool infiniteWidth = false;
    
    [DisableIf("infiniteWidth", isEqualTo: true)]
    public float maxScrollWidth = 0.15f;

    public bool infiniteHeight = false;

    [DisableIf("infiniteHeight", isEqualTo: true)]
    public float maxScrollHeight = 0.10f;

    [Header("Debug")]
    public bool drawRectDebug = false;
    public bool drawInteractionDebug = false;

    private Vector3?[] _touchingFingerPositions = new Vector3?[10];
    private float[] _fingerStrengths = new float[10];
    private StablePositionsDelta _stableFingersDelta = new StablePositionsDelta(10);
    private bool _popped = false;
    private float _lerpedPopState = 0f;

    // Deadzone
    //private Vector3 _deadzoneOrigin = Vector3.zero; // no longer used
    private bool _useDeadzone = true;
    private float _deadzoneCoeff = 0f;
    private float _deadzoneAccumMoveDist = 0f;

    // Momentum & Smoothing
    /// <summary> 0: Own momentum only. 1: Hand's momentum only. </summary>
    private float _momentumBlend = 0f;
    private Vector3 _ownMomentum = Vector3.zero;

    // Display cache
    private Vector3[] _fingertipPositions = new Vector3[10];

    private void OnValidate() {
      onValidateDisplay2();
    }

    private void OnEnable() {
      if (displayType == 1) {
        initDisplay1();
      }
    }
    

    private void OnDisable() {

    }

    private InteractionHand getIntHand(InteractionManager manager, bool isLeft) {
      return manager.interactionControllers.Query().FirstOrDefault(c => c.intHand != null && c.intHand.leapHand.IsLeft == isLeft) as InteractionHand;
    }

    private void updateForHand(Hand hand, out bool areFingertipsOutsideSurface) {

      InteractionHand intHand = null;
      if (hand != null && InteractionManager.instance != null) {
        intHand = getIntHand(InteractionManager.instance, hand.IsLeft);
      }

      areFingertipsOutsideSurface = true;
      if (hand != null && (intHand == null || !intHand.isGraspingObject)) {
        for (int i = 0; i < hand.Fingers.Count; i++) {

          int fingerArrayBaseIdx = hand.IsLeft ? 0 : 5;

          var finger = hand.Fingers[i];
          var fingertipPosition = finger.TipPosition.ToVector3();
          _fingertipPositions[fingerArrayBaseIdx + i] = fingertipPosition;

          var portalPose = portalObj.transform.ToPose() + new Pose(portalObj.transform.forward * depthOffset);
          var isFingertipProjectionInRect = false;
          var sqrDistToRect = 0f;
          var clampedFingertip = fingertipPosition
                                    .ClampedToRect(portalPose, portalObj.width, portalObj.height,
                                                   out sqrDistToRect, out isFingertipProjectionInRect);
          
          // Check whether we should 'pop' because the user reached too far.
          var fingertipPlaneSpace = fingertipPosition.GetLocalPlanePosition(portalPose);
          var fingertipDepth = fingertipPlaneSpace.z;
          var popDepth = 0.02f; // TODO: Turn into param
          if (fingertipDepth > popDepth) {
            _popped = true;
          }
          
          // We detect whether all fingertips "leave the portal surface" either sideways or depth-wise
          // to eventually reset the "popped" state of the portal surface.
          if (isFingertipProjectionInRect && !(fingertipDepth < depthOffset - thickness)) {
            areFingertipsOutsideSurface = false;
          }

          if (!_popped) {
            var pressStrength = sqrDistToRect.Map(0f, thickness * thickness, 1f, 0f);

            if (pressStrength > 0f && isFingertipProjectionInRect) {
              _touchingFingerPositions[fingerArrayBaseIdx + i] = clampedFingertip;
              _fingerStrengths[fingerArrayBaseIdx + i] = pressStrength;

              if (drawInteractionDebug) {
                //DebugPing.Ping(clampedFingertip, LeapColor.amber, 0.10f);
              }
            }
          }

        }
      }

      if (intHand != null && intHand.isGraspingObject) {
        areFingertipsOutsideSurface = false;
      }
    }

    private void Update() {

      _touchingFingerPositions.ClearWithDefaults();
      _fingertipPositions.ClearWith(Vector3.negativeInfinity);
      _fingerStrengths.ClearWithDefaults();

      Vector3 movementFromHand = Vector3.zero;

      // Reset momentum blend, adjusted again if fingertips are nearby the portal plane.
      _momentumBlend = 0f;


      var leftHand = Hands.Left;
      var rightHand = Hands.Right;

      bool allFingertipsOutOfSurface = true;
      bool handFingertipsOutOfSurface = true;

      updateForHand(leftHand, out handFingertipsOutOfSurface);
      allFingertipsOutOfSurface &= handFingertipsOutOfSurface;

      updateForHand(rightHand, out handFingertipsOutOfSurface);
      allFingertipsOutOfSurface &= handFingertipsOutOfSurface;

      if (allFingertipsOutOfSurface && _popped) {
        _popped = false;
        //DebugPing.Ping(portalObj.transform.position, LeapColor.blue, 0.2f);
      }

      _lerpedPopState = Mathf.Lerp(_lerpedPopState, _popped ? 1 : 0, 20f * Time.deltaTime);

      // Calculate momentum blend increase based on total finger proximity.
      var fingerStrengthMax = _fingerStrengths.Query().Fold((acc, f) => (f > acc ? f : acc));
      var targetBlend = fingerStrengthMax.Map(0f, 0.2f, 0f, 1f);
      _momentumBlend = Mathf.Lerp(_momentumBlend, targetBlend, 50f * Time.deltaTime); // TODO: Expose
      //_momentumBlend = fingerStrengthMax.Map(0f, 0.4f, 0f, 1f);

      // TODO: have finger strengths be persistent and fading..?

      _stableFingersDelta.UpdateCentroidMovement(_touchingFingerPositions,
                                                _fingerStrengths,
                                                drawDebug: drawInteractionDebug);

      if (_stableFingersDelta.didCentroidAppear) {
        _deadzoneAccumMoveDist = 0f;
        _deadzoneCoeff = 0f;
        // deadzoneOrigin = _stableFingersDelta.centroid.Value; // no longer used
      }

      if (_stableFingersDelta.isMoving) {
        movementFromHand = _stableFingersDelta.movement;

        if (_useDeadzone) {
          _deadzoneAccumMoveDist += movementFromHand.magnitude;

          _deadzoneCoeff = _deadzoneAccumMoveDist.Map(minDeadzoneWidth, deadzoneWidth, 0f, 1f);

          //_deadzoneOrigin = _stableFingersDelta.centroid.Value; // no longer used


          if (_deadzoneCoeff >= 1f) {
            _useDeadzone = false;
            _deadzoneCoeff = 1f;
          }

          movementFromHand *= _deadzoneCoeff;
        }
      }
      else {
        _useDeadzone = true;
      }

      // Decay momentum via friction.
      var ownMomentumPostFriction = _ownMomentum - (_ownMomentum * momentumDecayFriction * Time.deltaTime);
      if (Vector3.Dot(_ownMomentum, ownMomentumPostFriction) < 0) {
        _ownMomentum = Vector3.zero;
      }
      else {
        _ownMomentum = ownMomentumPostFriction;
      }

      // Blend momentum with hand motion.
      _ownMomentum = Vector3.Lerp(_ownMomentum, movementFromHand, _momentumBlend);

      // Apply scroll boundary constraints.
      var curPos = slideableObjectsRoot.position;
      var portalPlanePose = portalObj.transform.ToPose();

      var curPosPortalPlaneSpace = curPos.GetLocalPlanePosition(portalPlanePose);
      curPosPortalPlaneSpace.z = portalObj.transform.InverseTransformPoint(curPos).z;

      var curMomentumPortalPlaneSpace = (portalPlanePose.position + _ownMomentum).GetLocalPlanePosition(portalPlanePose);

      bool applyMomentumConstraint = false;
      if (!infiniteWidth) {
        if (curPosPortalPlaneSpace.x >= maxScrollWidth / 2f && curMomentumPortalPlaneSpace.x > 0f) {
          curPosPortalPlaneSpace.x = maxScrollWidth / 2f;
          curMomentumPortalPlaneSpace.x = 0f;
          applyMomentumConstraint = true;
        }
        if (curPosPortalPlaneSpace.x <= -maxScrollWidth / 2f && curMomentumPortalPlaneSpace.x < 0f) {
          curPosPortalPlaneSpace.x = -maxScrollWidth / 2f;
          curMomentumPortalPlaneSpace.x = 0f;
          applyMomentumConstraint = true;
        }
      }
      if (!infiniteHeight) {
        if (curPosPortalPlaneSpace.y >= maxScrollHeight / 2f && curMomentumPortalPlaneSpace.y > 0f) {
          curPosPortalPlaneSpace.y = maxScrollHeight / 2f;
          curMomentumPortalPlaneSpace.y = 0f;
          applyMomentumConstraint = true;
        }
        if (curPosPortalPlaneSpace.y <= -maxScrollHeight / 2f && curMomentumPortalPlaneSpace.y < 0f) {
          curPosPortalPlaneSpace.y = -maxScrollHeight / 2f;
          curMomentumPortalPlaneSpace.y = 0f;
          applyMomentumConstraint = true;
        }
      }

      if (applyMomentumConstraint) {
        slideableObjectsRoot.transform.position = (portalPlanePose * curPosPortalPlaneSpace).position;
        _ownMomentum = (portalPlanePose * curMomentumPortalPlaneSpace).position.From(portalPlanePose.position);
      }

      // Apply momentum.
      slideableObjectsRoot.transform.position += _ownMomentum;

      // Update display.
      if (displayType == 1) {
        updateDisplay1(_fingertipPositions, _fingerStrengths);
      }
      else {
        updateDisplay2(_fingertipPositions, _fingerStrengths);
      }

    }

    #region Display

    [Header("Display Type")]
    [EditTimeOnly]
    public int displayType = 2;

    #region Display 1

    [Header("Display Elements (option 1)")]

    // Color Receiver Sequence
    [SerializeField]
    [ImplementsInterface(typeof(IGameObjectSequenceProvider))]
    private MonoBehaviour _colorReceivers;
    public IGameObjectSequenceProvider colorReceivers {
      get { return _colorReceivers as IGameObjectSequenceProvider; }
    }
    public IColorReceiver GetColorReceiver(int index) {
      return colorReceivers[index].GetComponent<IColorReceiver>();
    }
    public int numColorReceivers { get { return colorReceivers.Count; } }

    private IColorReceiver[] _colorReceiverCache;

    // Position Provider Sequence
    [SerializeField]
    [ImplementsInterface(typeof(IGameObjectSequenceProvider))]
    private MonoBehaviour _positionProviders;
    public IGameObjectSequenceProvider positionProviders {
      get { return _positionProviders as IGameObjectSequenceProvider; }
    }
    public IVector3Provider GetPositionProvider(int index) {
      return colorReceivers[index].GetComponent<IVector3Provider>();
    }
    public int numPositionProviders { get { return positionProviders.Count; } }

    private IVector3Provider[] _positionProviderCache;

    // Color
    public Color idleColor = LeapColor.gray;

    public Color proximityColor = Color.white;

    // Distance Lerp
    public float idleDistance = 0.08f;

    public float proximalDistance = 0.01f;

    private void initDisplay1() {
      _positionProviderCache = new IVector3Provider[numPositionProviders];
      for (int i = 0; i < positionProviders.Count; i++) {
        _positionProviderCache[i] = GetPositionProvider(i);
      }
      
      _colorReceiverCache = new IColorReceiver[numColorReceivers];
      for (int i = 0; i < colorReceivers.Count; i++) {
        _colorReceiverCache[i] = GetColorReceiver(i);
      }
    }

    /// <summary>
    /// Note: Sentinel value for "no fingertip data" is Vector3.negativeInfinity.
    /// </summary>
    private void updateDisplay1(Vector3[] fingertipPositions, float[] fingerStrengths) {

      float[] gracineEnergy = new float[_positionProviderCache.Length];
      gracineEnergy.ClearWith(0f);

      unsafe {

        float[] gracineEnergyCopy = new float[gracineEnergy.Length];
        gracineEnergyCopy.ClearWith(0f);

        int numPositionProviders = _positionProviderCache.Length;
        Vector3[] elementPositions = new Vector3[numPositionProviders];
        for (int i = 0; i < numPositionProviders; i++) {
          elementPositions[i] = _positionProviderCache[i].Get();
        }
        
        for (int i = 0; i < numPositionProviders; i++) {
          for (int f = 0; f < 5; f++) {

            var testSqrDist = (fingertipPositions[f] - elementPositions[i]).sqrMagnitude;
            var closeness = testSqrDist.Map(idleDistance * idleDistance,
                                            proximalDistance * proximalDistance,
                                            0f, 1f);

            gracineEnergyCopy[i] += closeness * fingerStrengths[f];
          }
        }

        for (int i = 0; i < gracineEnergyCopy.Length; i++) {
          gracineEnergy[i] = gracineEnergyCopy[i];
        }
      }
      
      for (int i = 0; i < _colorReceiverCache.Length; i++) {
        _colorReceiverCache[i].Receive(Color.Lerp(idleColor, proximityColor, gracineEnergy[i]));
      }
      
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (this.enabled && this.gameObject.activeInHierarchy && drawRectDebug) {
        drawer.color = LeapColor.jade;

        drawer.PushMatrix();
        drawer.matrix = Matrix4x4.TRS(portalObj.transform.TransformPoint(Vector3.forward * depthOffset), portalObj.transform.rotation, portalObj.transform.lossyScale);

        drawer.DrawWireCube(Vector3.zero, new Vector3(portalObj.width, portalObj.height, thickness));
        drawer.DrawWireCube(Vector3.zero, new Vector3(portalObj.width * 0.8f, portalObj.height * 0.8f, thickness));

        drawer.PopMatrix();

        drawer.PushMatrix();
        drawer.matrix = Matrix4x4.TRS(portalObj.transform.TransformPoint(Vector3.forward * depthOffset), portalObj.transform.rotation, Vector3.one);

        drawer.color = LeapColor.magenta;
        if (!infiniteWidth || !infiniteHeight) {
          var width = 100000f;
          var height = 100000f;

          if (!infiniteWidth) {
            width = maxScrollWidth;
          }
          if (!infiniteHeight) {
            height = maxScrollHeight;
          }

          drawer.DrawWireCube(Vector3.zero, new Vector3(width, height, thickness));
        }

        drawer.PopMatrix();
      }
    }

    #endregion

    #region Display 2

    [Header("Display option 2")]
    
    public Renderer portalSurfaceRenderer;

    [SerializeField, OnEditorChange("portalGridOffsetParamName")]
    private string _portalGridOffsetParamName = "_OffsetAndPopState";
    public string portalGridOffsetParamName {
      get { return _portalGridOffsetParamName; }
      set {
        _portalGridOffsetParamName = value;
        _portalGridOffsetParamId = Shader.PropertyToID(value);
      }
    }
    [SerializeField, Disable]
    private int _portalGridOffsetParamId = 0;

    [SerializeField, OnEditorChange("slideSurfaceGlowOffsetParamName")]
    [Tooltip("With a valid parameter name, this shader parameter will be set on the attached portal surface renderer: "
             + "Its Z coordinate will match the 'Depth Offset' setting (local space).")]
    private string _slideSurfaceGlowOffsetParamName = "_SurfaceGlowOffset";
    public string slideSurfaceGlowOffsetParamName {
      get { return _slideSurfaceGlowOffsetParamName; }
      set {
        _slideSurfaceGlowOffsetParamName = value;
        _slideSurfaceGlowOffsetParamId = Shader.PropertyToID(value);
      }
    }
    [SerializeField, Disable]
    private int _slideSurfaceGlowOffsetParamId = 0;

    //[SerializeField, OnEditorChange("worldToObjectMatrixParamName")]
    //[Tooltip("Shaders that need a world to object matrix can have this parameter specified here.")]
    //private string _worldToObjectMatrixParamName = "_WorldToObjectMatrix";
    //public string worldToObjectMatrixParamName {
    //  get { return _slideSurfaceGlowOffsetParamName; }
    //  set {
    //    _worldToObjectMatrixParamName = value;
    //    _worldToObjectMatrixParamId = Shader.PropertyToID(value);
    //  }
    //}
    //[SerializeField, Disable]
    //private int _worldToObjectMatrixParamId = 0;

    [SerializeField, Disable]
    private int _colorParamId;

    private void onValidateDisplay2() {
      _portalGridOffsetParamId = Shader.PropertyToID(_portalGridOffsetParamName);
      _slideSurfaceGlowOffsetParamId = Shader.PropertyToID(_slideSurfaceGlowOffsetParamName);
      SetSurfaceGlowOffsetVector(depthOffset);

      _colorParamId = Shader.PropertyToID("_Color");
    }

    private void updateDisplay2(Vector3[] fingertipPositions, float[] fingerStrengths) {

      // Update material offset based on sliding movement.
      var materialOffset = slideableObjectsRoot.transform.position.InLocalSpace(portalObj.transform);
      SetOffsetVector(materialOffset);

      // Update slider surface offset in case the setting has changed.
      SetSurfaceGlowOffsetVector(depthOffset);
    }

    public void SetOffsetVector(Vector2 offset) {
      if (portalSurfaceRenderer != null) {
        if (Application.isEditor) {
          portalSurfaceRenderer.sharedMaterial.SetVector(_portalGridOffsetParamId,
            new Vector3(offset.x,
                        offset.y,
                        _lerpedPopState));
          portalSurfaceRenderer.sharedMaterial.SetVector("_PopStateBackup", Vector4.one * _lerpedPopState);

          // TODO: For some reason the normal pop state vector channels are TOTALLY NOT WORKING in builds!!!!
          // Instead, use the alpha channel of the shader _Color to store the popped state, this fixes builds >_<
          portalSurfaceRenderer.sharedMaterial.SetColor(_colorParamId, portalSurfaceRenderer.sharedMaterial.GetColor(_colorParamId).WithAlpha(_lerpedPopState));

          //Debug.LogError("UPLOADED POP STATE: " + _lerpedPopState + ", " + (new Vector3(offset.x, offset.y, _lerpedPopState)));
        }
        else {
          portalSurfaceRenderer.sharedMaterial.SetVector(_portalGridOffsetParamId,
            new Vector3(offset.x,
                        offset.y,
                        _lerpedPopState));
          portalSurfaceRenderer.sharedMaterial.SetVector("_PopStateBackup", Vector4.one * _lerpedPopState);
          portalSurfaceRenderer.material.SetColor(_colorParamId, portalSurfaceRenderer.material.GetColor(_colorParamId).WithAlpha(_lerpedPopState));
        }
      }
    }

    public void SetSurfaceGlowOffsetVector(float depthOffset) {
      if (portalSurfaceRenderer != null) {
        portalSurfaceRenderer.sharedMaterial.SetVector(_slideSurfaceGlowOffsetParamId, new Vector4(0f, 0f, depthOffset, 0f));
      }
    }

    #endregion

    #endregion

    #region Support

    public class StablePositionsDelta {

      private Vector3?[] _lastPositions = null;

      private Vector3? _centroid = null;
      public Vector3? centroid {
        get { return _centroid; }
      }

      private bool _isMoving = false;
      public bool isMoving {
        get { return _isMoving; }
      }

      private bool _didCentroidAppear = false;
      public bool didCentroidAppear {
        get { return _didCentroidAppear; }
      }

      private bool _didCentroidTeleport = false;
      public bool didCentroidTeleport {
        get { return _didCentroidTeleport; }
      }

      private bool _didCentroidDisappear = false;
      public bool didCentroidDisappear {
        get { return _didCentroidDisappear; }
      }

      private Vector3 _avgDelta;
      public Vector3 movement { get { return _avgDelta; } }
       
      public StablePositionsDelta(int maxPositions) {
        _lastPositions = new Vector3?[maxPositions];
      }

      public void UpdateCentroidMovement(Vector3?[] positions,
                                         float[] strengths = null,
                                         bool drawDebug = false) {
        if (strengths != null && positions.Length != strengths.Length) {
          throw new InvalidOperationException(
            "positions and strengths Indexables must have the same Count.");
        }


        bool[] useableIndices = new bool[_lastPositions.Length];

        _didCentroidAppear = false;
        _didCentroidDisappear = false;

        int numLastValidPositions = CountValid(_lastPositions);
        int numCurValidPositions = CountValid(positions);

        if (numLastValidPositions == 0 && numCurValidPositions > 0) {
          _didCentroidAppear = true;
        }
        if (numLastValidPositions > 0 && numCurValidPositions == 0) {
          _didCentroidDisappear = true;
        }

        // Useable indices have valid positions in both the "last" and "current" arrays.
        for (int i = 0; i < _lastPositions.Length; i++) {
          if (i >= positions.Length) break;

          var lastV = _lastPositions[i];
          var curV = positions[i];

          if (lastV.HasValue && curV.HasValue) {
            useableIndices[i] = true;
          }
          else if (!lastV.HasValue && !curV.HasValue) {
            // One index has a value in one array and no value in the other;
            // this means the Centroid is going to teleport.
            _didCentroidTeleport = true;
          }
        }

        _isMoving = false;
        _avgDelta = Vector3.zero;
        int count = 0;
        for (int i = 0; i < useableIndices.Length; i++) {
          if (useableIndices[i]) {
            _isMoving = true;
            var addedDelta = (positions[i] - _lastPositions[i]).Value;
            if (strengths != null) {
              addedDelta *= strengths[i];
            }
            _avgDelta += addedDelta;
            count++;
          }
        }
        if (count > 0) {
          _avgDelta /= count;
        }


        // Update centroid state.

        if (_didCentroidAppear) {
          _centroid = positions.Query()
                               .Select(maybeV => maybeV.GetValueOrDefault())
                               .Fold((acc, v) => acc + v)
                      / numCurValidPositions;

          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.cyan, 0.20f);
          }
        }

        if (_centroid != null) {
          _centroid += _avgDelta;

          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.green, 0.15f);
          }
        }

        if (_didCentroidDisappear) {
          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.black, 0.20f);
          }

          _centroid = null;
        }


        // Set last positions with the current positions.

        for (int i = 0; i < _lastPositions.Length; i++) {
          if (i >= positions.Length) {
            _lastPositions[i] = null;
          }
          else {
            _lastPositions[i] = positions[i];
          }
        }
      }

      public void UpdateCentroidMovement(Vector3?[] positions,
                                         bool drawDebug = false) {
        UpdateCentroidMovement(positions, null, drawDebug);
      }

      private int CountValid(Vector3?[] positions) {
        return positions.Query().Where(v => v.HasValue).Count();
      }

      private int CountValid(IIndexable<Vector3?> positions) {
        //return positions.Query().Where(v => v.HasValue).Count();
        int count = 0;
        foreach (var pos in positions.GetEnumerator()) {
          if (pos.HasValue) count++;
        }
        return count;
      }
    }

    #endregion

  }

}

using Leap.Unity.Animation;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity;
using Leap.Unity.Attachments;
using System;
using System.Linq;
using UnityEngine.Events;


namespace LeapSingleHandedShortcuts {
  public class PowerBall : MonoBehaviour {

    public Transform VRCamera;
    //All UIs
    public InteractionHand interactionHand;
    public Transform PowerBallXform;
    public Collider PoswerBallCollider;
    public Transform PowerBallRestPosition;
    private Vector3 PowerBallActivePosition;
    public float FacingThreshold = .8f;
    public float CameraWorldUpWeight = .7f;

    public float DeadZone = .025f;
    [Header("Palm Facing State Change Thresholds")]
    public float RestingToMovingThreshold = 0f;
    public float MovingToRestingThreshold = 0f;
    public float ReadyToMovingThreshold = .76f;
    public float MovingToReadyThreshold = .95f;
    [Space(10)]
    [Range(0f, 1f)]
    public float FacingAffordanceBlendWeight;
    public float PinchPositionMultiplier = 1.3f;
    public Vector3 PowerBallAimOffset;
    public Transform Palm;
    public Transform Wrist;

    public AnimationCurve PositionX;
    public AnimationCurve PositionY;

    public Transform PowerBallUI;
    public Transform PowerBallUI_ArcCurve;
    public Transform PowerBallUI_TArcs;
    public Transform PowerBallUI_ArcVolumes;
    public Transform PowerBallUI_Quadrants;
    public Transform PowerBallUI_SlideRing;
    public Transform PowerBallToggleButtonXform;
    public InteractionToggle interactionToggle;

    public bool DeParentWhenOpen;
    public bool PositionWhenDeParented;

    private int currentShortCut;
    public int CurrentShortCut {
      get {
        return currentShortCut;
      }
      set {
        if (currentShortCut != value) {
          currentShortCut = value;
          OnSelectShortCut.Invoke(CurrentShortCut);
        }
      }
    }

    public int PowerBallStateIndex;

    [System.Serializable]
    public class customBoolEvent : UnityEvent<bool> { }

    [System.Serializable]
    public class customFloatEvent : UnityEvent<float> { }

    [System.Serializable]
    public class customIntEvent : UnityEvent<int> { }

    public customIntEvent OnSelectShortCut;
    public customIntEvent OnActivatePower;
    public customIntEvent OnPowerBallStateChange;
    public customBoolEvent OnActivateDeActivatePowerBall;
    public customBoolEvent OnHiLightPowerBall;
    public customBoolEvent OnMove;
    public UnityEvent OnRest;
    public customBoolEvent OnMakeReady;
    public UnityEvent OnOpen;
    public UnityEvent OnAudioTick;
    public customFloatEvent OnPinchStrength;

    private float powerBallPinchStrength;
    public float PowerBallPinchStrength {
      get {
        return powerBallPinchStrength;
      }
      set {
        powerBallPinchStrength = value;
        OnPinchStrength.Invoke(value);
      }
    }
    private bool isBlocked = true;
    public bool IsBlocked {
      get {
        return isBlocked;
      }
      set {
        if(powerBallFSM.State != PowerBallFSM.States.Disabled) BlockUnBlockPowerBall(value);
        isBlocked = value;
      }
    }

    public bool IsPowerBallActive {
      get {
        return isPowerBallActive;
      }

      set {
        ActivateDeActivatePowerBall(value);
        isPowerBallActive = value;
      }
    }

    public bool UseForward {
      get {
        return useForward;
      }

      set {
        useForward = value;
        rebuildIsQuandrantActive();
      }
    }

    public bool UseRight {
      get {
        return useRight;
      }

      set {
        useRight = value;
        rebuildIsQuandrantActive();
      }
    }

    public bool UseBack {
      get {
        return useBack;
      }

      set {
        useBack = value;
        rebuildIsQuandrantActive();
      }
    }

    public bool UseLeft {
      get {
        return useLeft;
      }

      set {
        Debug.Log("1");
        useLeft = value;
        rebuildIsQuandrantActive();
      }
    }

    public bool UseUp {
      get {
        return useUp;
      }

      set {
        useUp = value;
        rebuildIsQuandrantActive();
      }
    }

    public bool UseDown {
      get {
        return useDown;
      }

      set {
        useDown = value;
        rebuildIsQuandrantActive();
      }
    }

    private bool isPowerBallActive = true;

    public Chirality Handedness;

    private bool isPinching = false;
    private float chiralityFlip;
    private int lastRestShortcut;
    public PowerBallFSM powerBallFSM;

    private List<Collider> optionTriggersColliders;

    public enum UItype { ArcVolumes, ArcCurve, TArcs, Quadrants, SlideRing }
    [Space(10)]
    public UItype UIStyle;

    //Arc Volume UI
    [Header("Arc Volume UI")]
    public List<Collider> VolumeTriggerColliders;

    private List<PowerBallUICurve> UICurves = new List<PowerBallUICurve>();

    public int CloseCoolDown = 30;
    private int pinchHysteresisFrame = 0;
    private Vector3 lastDotPosition = Vector3.zero;

    //Quadrants
    [Header("Quadrants")]
    //popuate in Inspector - the loop to auto fill List of Lists DotGroups
    public List<Transform> QuadrantDotsParents;
    public List<List<Transform>> DotGroups = new List<List<Transform>>();
    public List<Transform> DotsForward;
    public List<Transform> DotsRight;
    public List<Transform> DotsBack;
    public List<Transform> DotsLeft;
    public List<Transform> DotsUp;
    public List<Transform> DotsDown;

    public int OptionsTotal = 3;

    private bool useForward = true;
    private bool useRight = true;
    private bool useBack = true;
    private bool useLeft = true;
    private bool useUp = false;
    private bool useDown = false;

    private List<bool> IsQuadrantActive;// = new List<bool> {UseForward, Use}

    //Debug
    [Header("Debug")]
    public TextMesh DebugFacingText;
    public TextMesh DebugStateText;
    public TextMesh DebugOptionText;
    public TextMesh DebugPinchText;
    public TextMesh DebugPinchPosition;

    private void Awake() {
      powerBallFSM = new PowerBallFSM(this);
    }

    private void rebuildIsQuandrantActive() {
      IsQuadrantActive = new List<bool> { UseForward, UseRight, UseBack, UseLeft, UseUp, UseDown };
    }

    void Start() {
      rebuildIsQuandrantActive();
      InteractionHand[] hands = FindObjectsOfType<InteractionHand>();
      foreach (InteractionHand h in hands) {
        if (h.handDataMode == HandDataMode.PlayerLeft && Handedness == Chirality.Left) interactionHand = h;
        if (h.handDataMode == HandDataMode.PlayerRight && Handedness == Chirality.Right) interactionHand = h;
      }
      VRCamera = Camera.main.transform;
      AttachmentPointBehaviour[] attachPoints = transform.parent.parent.parent.GetComponentsInChildren<AttachmentPointBehaviour>();
      foreach (AttachmentPointBehaviour a in attachPoints) {
        if (a.attachmentPoint == AttachmentPointFlags.Wrist) Wrist = a.transform;
        if (a.attachmentPoint == AttachmentPointFlags.Palm) Palm = a.transform;
      }
      initializePowerBall();
      Move(false);
      chiralityFlip = interactionHand.isRight == true ? 1f : -1f;
      CurrentShortCut = 0;
      lastRestShortcut = 1;
    }

    private void initializePowerBall() {
      if (UIStyle == UItype.ArcVolumes) {
        PowerBallUI = PowerBallUI_ArcVolumes;
        PositionWhenDeParented = true;
        optionTriggersColliders = VolumeTriggerColliders;
        OptionsTotal = 3;
      }
      if (UIStyle == UItype.ArcCurve) {
        PowerBallUI = PowerBallUI_ArcCurve;
        PositionWhenDeParented = true;
        //build curve array
        var curves = PowerBallUI.GetComponentsInChildren<PowerBallUICurve>();
        foreach (PowerBallUICurve curve in curves) {
          UICurves.Add(curve);
        }
        OptionsTotal = 4;
      }
      if (UIStyle == UItype.TArcs) {
        PowerBallUI = PowerBallUI_TArcs;
        PositionWhenDeParented = true;
        //build curve array
        var curves = PowerBallUI.GetComponentsInChildren<PowerBallUICurve>();
        foreach (PowerBallUICurve curve in curves) {
          UICurves.Add(curve);
        }
        OptionsTotal = 3;
      }
      if (UIStyle == UItype.Quadrants) {
        PowerBallUI = PowerBallUI_Quadrants;
        DotGroups = new List<List<Transform>> { DotsForward, DotsRight, DotsBack, DotsLeft, DotsUp, DotsDown };
        for (int i = 0; i < 6; i++) {
          foreach (Transform t in QuadrantDotsParents[i].GetComponentsInChildren<Transform>()) {
            if (t.parent == QuadrantDotsParents[i]) DotGroups[i].Add(t);//only get 1st level children, not grandchildren
          }
        }
        PositionWhenDeParented = false;
        Close();
        Rest();
        OptionsTotal = 4;
      }
      if(UIStyle == UItype.SlideRing) {
        PowerBallUI = PowerBallUI_SlideRing;
        PositionWhenDeParented = true;

      }
    }

    public void SwapUI(UItype type) {
      MakeReady(false);
      UIStyle = type;
      initializePowerBall();
    }

        void Update() {
            if (Input.GetKeyUp(KeyCode.L)) UseLeft = true;
            if (Input.GetKeyUp(KeyCode.B)) IsBlocked = true;
            if (Input.GetKeyUp(KeyCode.V)) IsBlocked = false;
            if (Input.GetKeyUp(KeyCode.A)) IsPowerBallActive = true;
            if (Input.GetKeyUp(KeyCode.D)) IsPowerBallActive = false;
            if (Input.GetKeyUp(KeyCode.M)) SwapUI(UItype.ArcVolumes);
            if (Input.GetKeyUp(KeyCode.N)) SwapUI(UItype.ArcCurve);
            if (Input.GetKeyUp(KeyCode.O)) SwapUI(UItype.Quadrants);

            DebugStateText.text = powerBallFSM.State.ToString();
            //DebugPinchText.text = PowerBallPinchStrength.ToString();

            if (interactionHand.isTracked == false && powerBallFSM.State != PowerBallFSM.States.Disabled) {
                OnDisablePowerBall();
            }

            //else if (interactionHand.isTracked == true && powerBallFSM.State != PowerBallFSM.States.Disabled) OnEnablePowerBall();
            if (!interactionHand.isGraspingObject) { 
                switch (powerBallFSM.State) {
                    case PowerBallFSM.States.Disabled:
                        //disable Powerball itself
                        if (interactionHand.isTracked == true && IsPowerBallActive == true) {
                            OnEnablePowerBall();
                        }
                        break;
                    case PowerBallFSM.States.Blocked:
                        //afford blockage
                        PowerBallXform.position = PowerBallToggleButtonXform.position;
                        PowerBallPinchStrength = interactionHand.leapHand.PinchStrength;
                        break;
                    case PowerBallFSM.States.Resting:
                        PowerBallXform.position = PowerBallToggleButtonXform.position;
                        PowerBallXform.rotation = PowerBallToggleButtonXform.rotation;

                        PowerBallPinchStrength = interactionHand.leapHand.PinchStrength;
                        if (CurrentShortCut != lastRestShortcut) {
                            OnActivatePower.Invoke(CurrentShortCut);
                            lastRestShortcut = CurrentShortCut;
                        }
                        if (GetPalmFacing() > RestingToMovingThreshold) {
                            Move(true);
                        }
                        break;
                    case PowerBallFSM.States.Moving:
                        //check for palm facing
                        if (GetPalmFacing() < MovingToRestingThreshold) {
                            Rest();
                        }
                        //Adding pinch check to block opening if pinch starts when facing away
                        if (GetPalmFacing() > MovingToReadyThreshold && interactionHand.leapHand.IsPinching() == false) {
                            MakeReady(true);
                        }
                        ConstrainBall();
                        PowerBallPinchStrength = 0f;
                        DebugFacingText.text = GetPalmFacing().ToString("F2");
                        if (Mathf.Clamp01(GetPalmFacing()) < 0.001f) PowerBallXform.position = PowerBallToggleButtonXform.position;
                        break;
                    case PowerBallFSM.States.Ready:
                        if (GetPalmFacing() < ReadyToMovingThreshold) {
                            Move(false);
                        }
                        DebugFacingText.text = GetPalmFacing().ToString("F2");

                        //listen for completed pinch
                        ConstrainBall();
                        PowerBallPinchStrength = interactionHand.leapHand.PinchStrength;
                        //check for pinch point in powerball
                        if (isPinching != GetIsPinchingInBall()) {
                            isPinching = !isPinching;
                            if (GetIsPinchingInBall() == true) {
                                Open();
                            }
                        }
                        break;
                    case PowerBallFSM.States.Open:
                        Vector3 pinchPosition = interactionHand.leapHand.GetPinchPosition();
                        PowerBallPinchStrength = interactionHand.leapHand.PinchStrength;

                        if (PowerBallPinchStrength > .8f) CurrentShortCut = GetCurrentShortCut();
                        //DebugOptionText.text = CurrentShortCut.ToString();

                        if (UIStyle == UItype.ArcVolumes) AimBallAlongWristAxis();

                        if (UIStyle == UItype.ArcCurve && PowerBallPinchStrength > .6f) {
                            PowerBallXform.position = UICurves[0].NearestDotOnCurve(pinchPosition).position;
                            AimBallAlongWristAxis();
                        }
                        if (UIStyle == UItype.TArcs && PowerBallPinchStrength > .6f) {
                            Transform nearestDot = NearestDot(pinchPosition);
                            PowerBallXform.position = nearestDot.position;
                            PowerBallXform.rotation = nearestDot.rotation;
                        }
                        if (UIStyle == UItype.Quadrants) {
                            PowerBallXform.position = NearestDot(pinchPosition).position;
                            if (PowerBallXform.position != lastDotPosition) OnAudioTick.Invoke();
                            lastDotPosition = PowerBallXform.position;
                            PowerBallXform.rotation = PowerBallUI.rotation;
                        }
                        if (UIStyle == UItype.SlideRing) {
                            PowerBallUI.rotation = AlignFacingGoal(0f); // * Quaternion.Euler(90f, 0f, 0f);
                            /* PinchClamp() to morph based on pinch
                                * TiltRing() to pass through pinch point
                                * if( pinching over a threshold with hysteresis)
                                *   RotateRing() based on pinchpoint's relative rotation around ring center
                                * else
                                *   ReturnClamp() (and not ring) to default position
                                * */

                        }
                        if (PositionWhenDeParented) {
                            PowerBallUI.transform.localPosition = this.transform.position;
                            if (UIStyle == UItype.ArcVolumes && interactionHand.isLeft) {
                                PowerBallUI.position += Palm.forward * .06f;
                                PowerBallUI.position += Palm.right * .02f;
                                PowerBallUI.position += Palm.up * .02f;
                            }
                        }
                        //Closing logic
                        if (UIStyle == UItype.SlideRing) {

                        }
                        else {
                            if (isPinching != GetIsPinchingInBall()) {
                                isPinching = !isPinching;
                                if (GetIsPinchingInBall() == false) {
                                    pinchHysteresisFrame = Time.frameCount;
                                }
                            }
                            if (GetIsPinchingInBall() == true) pinchHysteresisFrame = Time.frameCount;
                            if (Time.frameCount - pinchHysteresisFrame > CloseCoolDown) MakeReady(false);
                        }

                        break;
                }
        }


      DebugFacingText.text = GetPalmFacing().ToString("F2");
    }

    private void PowerBallEvent(PowerBallFSM.Events newEvent) {
      powerBallFSM.ProcessEvent(newEvent);
      OnPowerBallStateChange.Invoke((int)powerBallFSM.State);
      PowerBallStateIndex = (int)powerBallFSM.State;
    }

    public bool GetIsPinchingInBall() {
      bool result = false;
      float threshold;
      if (powerBallFSM.State == PowerBallFSM.States.Open) threshold = .2f;
      else threshold = .9f;
      //if (PoswerBallCollider.bounds.Contains(interactionHand.leapHand.GetPinchPosition()) && interactionHand.leapHand.PinchStrength > threshold) result = true;
      if (interactionHand.leapHand.PinchStrength > threshold) result = true;

      return result;
    }

    public void OnDisablePowerBall() {
      OnActivateDeActivatePowerBall.Invoke(false);
      Close();
      PowerBallEvent(PowerBallFSM.Events.Disable);
    }

    public void OnEnablePowerBall() {
      OnActivateDeActivatePowerBall.Invoke(true);

      float palmFacingOnEnable = GetPalmFacing();
      if (palmFacingOnEnable > MovingToReadyThreshold) {
        MakeReady(false);
        return;
      }
      else if (palmFacingOnEnable > ReadyToMovingThreshold) {
        Move(false);
        return;
      }
      else {
        Rest();
      }
    }

    public void ActivateDeActivatePowerBall(bool isActive) {
      if (isActive) {
        PowerBallEvent(PowerBallFSM.Events.Rest);
        OnActivateDeActivatePowerBall.Invoke(true);
      }
      else {
        Close();
        PowerBallEvent(PowerBallFSM.Events.Disable);
        OnActivateDeActivatePowerBall.Invoke(false);
      }
    }

    public void BlockUnBlockPowerBall(bool isBlocked) {
      if (isBlocked) {
        Close();
        PowerBallEvent(PowerBallFSM.Events.Block);
        //OnEnableDisablePowerBall.Invoke(false);
      }
      else {
        PowerBallEvent(PowerBallFSM.Events.Rest);
        //OnEnableDisablePowerBall.Invoke(true);
      }
    }



    private void Rest() {
      PowerBallEvent(PowerBallFSM.Events.Rest);
      OnRest.Invoke();
    }

    private void Move(bool activateFacingAffordance) {
      PowerBallEvent(PowerBallFSM.Events.Move);
      OnMove.Invoke(activateFacingAffordance);
    }
    private void MakeReady(bool doAffordance) {
      Close();
      OnMakeReady.Invoke(doAffordance);
      PowerBallEvent(PowerBallFSM.Events.MakeReady);
    }

    private void Close() {
      //reparenting
      if (DeParentWhenOpen || PositionWhenDeParented) {
        if (PowerBallUI != null) {
          PowerBallUI.transform.parent = transform.parent;
          PowerBallUI.localPosition = Vector3.zero;
          PowerBallUI.rotation = PowerBallRestPosition.rotation;
        }
      }
      PowerBallUI.gameObject.SetActive(false);
      //TODO verify this is needed here
      OnSelectShortCut.Invoke(CurrentShortCut);
      OnHiLightPowerBall.Invoke(false);
    }

    private void Open() {
      if (DeParentWhenOpen) {
        if (UIStyle == UItype.ArcVolumes && interactionHand.isLeft) {
          PowerBallUI.localEulerAngles += new Vector3(0f, 0f, 180f);
        }
        PowerBallUI.transform.parent = null;
        if (UIStyle == UItype.Quadrants) {
          PowerBallUI.position = interactionHand.leapHand.GetPinchPosition();
          PowerBallUI.rotation = AlignFacingGoal(0f) * Quaternion.Euler(90f, 0f, 0f);
        }
      }
      //deploy UI
      PowerBallUI.gameObject.SetActive(true);
      OnHiLightPowerBall.Invoke(true);
      OnOpen.Invoke();
      PowerBallEvent(PowerBallFSM.Events.Open);
 
      if (UIStyle == UItype.ArcCurve) {
        foreach (PowerBallUICurve c in UICurves) {
          ActivateArcCurve(true, c);
        }
      }
      if (UIStyle == UItype.TArcs) {
          ActivateTArcsCurves(true);
      }
      //Call this here so PowerBall hi lights correct color
      OnSelectShortCut.Invoke(CurrentShortCut);
    }

    //Constraints
    private void ConstrainBall() {
      Vector3 pinchPosition = interactionHand.leapHand.GetPinchPosition();
      Vector3 indexTipDirection = pinchPosition - interactionHand.leapHand.Fingers[1].TipPosition.ToVector3();
      PowerBallActivePosition = pinchPosition - indexTipDirection * .1f;

      float lerpWeight = Mathf.Clamp01(GetPalmFacing()).Map(RestingToMovingThreshold, 1f, 0f, 1f);
      Vector3 powwerBallTargetPosition = Vector3.Lerp(PowerBallRestPosition.position, PowerBallActivePosition, lerpWeight);

      powwerBallTargetPosition += Palm.transform.right * PositionX.Evaluate(lerpWeight) * chiralityFlip;
      powwerBallTargetPosition += Palm.transform.up * PositionY.Evaluate(lerpWeight);

      PowerBallXform.position = Vector3.Lerp(PowerBallXform.position, powwerBallTargetPosition, .4f);
      PowerBallXform.rotation = Quaternion.Lerp(PowerBallRestPosition.rotation, AimBallAlongPinchAxis(), lerpWeight);
    }

    private Quaternion AimBallAlongPinchAxis() {
      Quaternion aimedRotation = Quaternion.LookRotation(interactionHand.leapHand.Fingers[2].TipPosition.ToVector3() - interactionHand.leapHand.Fingers[0].TipPosition.ToVector3());
      aimedRotation = aimedRotation * Quaternion.Euler(PowerBallAimOffset);
      return aimedRotation;
    }

    private void AimBallAlongWristAxis() {
      PowerBallXform.rotation = Palm.rotation;
    }

    //Activation Heuristics
    public float GetPalmFacing() {
      Vector3 facingVector;
      if (UIStyle == UItype.SlideRing) facingVector = Camera.main.transform.right;
      else facingVector = Vector3.Lerp(Vector3.up * -1f, Camera.main.transform.forward, FacingAffordanceBlendWeight);
      //Vector3 facingVector = Camera.main.transform.right;
      float facingWeight = 0f;
      facingWeight = Vector3.Dot(facingVector, Palm.transform.up);
      if (facingWeight > FacingThreshold) facingWeight = 1;
      return facingWeight;
    }

    private int GetCurrentShortCut() {
      int shortCut = CurrentShortCut;
      Vector3 pinchPosition = interactionHand.leapHand.GetPinchPosition();
      if (UIStyle == UItype.ArcVolumes) {
        for (int i = 0; i < optionTriggersColliders.Count; i++) {
          if (optionTriggersColliders[i].bounds.Contains(pinchPosition)) {
            shortCut = i;
            break;
          }
        }
      }
      if (UIStyle == UItype.ArcCurve) {
        float closestPoint = UICurves[0].CurvePoints.IndexOf(UICurves[0].NearestDotOnCurve(pinchPosition));
        float newValue = closestPoint.Map(0, UICurves[0].CurvePoints.Count, 0, OptionsTotal);
        shortCut = (int)newValue;
      }
      if (UIStyle == UItype.TArcs) {
        Vector3 pinchDirection = UICurves[0].ControlPoints[0].position - pinchPosition;
        float pinchDistance = pinchDirection.magnitude;
        if (pinchDistance > DeadZone) {
          int orthogonalDir = ClosestDirection(pinchDirection, UICurves[0].ControlPoints[0]);
          //DebugPinchPosition.text = orthogonalDir.ToString();
          DebugPinchPosition.transform.position = pinchPosition;
          shortCut = orthogonalDir;
          CurrentShortCut = shortCut;
        }
        return currentShortCut;
      }
      if (UIStyle == UItype.Quadrants) {
        Vector3 pinchDirection = PowerBallUI.position - pinchPosition;
        float pinchDistance = pinchDirection.magnitude;
        if (pinchDistance > DeadZone) {
          int orthogonalDir = ClosestDirection(pinchDirection, PowerBallUI);
          //DebugPinchPosition.text = orthogonalDir.ToString();
          DebugPinchPosition.transform.position = pinchPosition;
          shortCut = orthogonalDir;
          CurrentShortCut = shortCut;
        }
        return currentShortCut;
      }
      return shortCut;
    }

    private int ClosestDirection(Vector3 pinchDirection, Transform center) {
      List<Vector3> compass = new List<Vector3> { center.forward, center.right, center.forward * -1, center.right * -1, center.up, center.up * -1 };
      //IsQuadrantActive = new List<bool> { UseForward, UseRight, UseBack, UseLeft, UseUp, UseDown };
      int index = -1;
      var maxDot = -Mathf.Infinity;
      Vector3 ret = center.forward * -1;
      for (int i = 0; i < compass.Count; i++) {
        float t = Vector3.Dot(pinchDirection, compass[i]);
        if (t > maxDot && IsQuadrantActive[i] == true) {
          ret = compass[i];
          maxDot = t;
          index = i;
        }
      }
      return index;
    }

    private bool isHandHoveringAnIEObject() {
      return false;
    }



    public class PowerBallFSM {
      private PowerBall powerBall;

      public enum States { Disabled, Blocked, Resting, Moving, Ready, Open };
      public States State { get; set; }

      public enum Events { Disable, Block, Rest, Move, MakeReady, Open, };

      private Action[,] fsm;

    public PowerBallFSM(PowerBall powerBall) {
      this.powerBall = powerBall;
      this.fsm = new Action[6, 6]
        {
          //Disable,           Block,            Rest,                 Standy,            MakeReady,             Open,       
          {this.DoNothing,    this.Block,        this.Rest,            this.Move,        this.MakeReady,      null                 },       //Disabled
          {this.Disable,      this.DoNothing,    this.Rest,            this.Move,        this.MakeReady,      null                 },       //Blocked
          {this.Disable,      this.Block,        this.DoNothing,       this.Move,        this.MakeReady,      null                 },       //Resting
          {this.Disable,      this.Block,        this.Rest,            this.DoNothing,   this.MakeReady,      null                 },       //Moving
          {this.Disable,      this.Block,        this.Rest,            this.Move,        this.DoNothing,      this.Open            },       //Ready
          {this.Disable,      this.Block,        this.Rest,            this.Move,        this.MakeReady,      this.DoNothing       }        //Open
        };
      }

      public void ProcessEvent(Events theEvent) {
        if (this.fsm[(int)this.State, (int)theEvent] != null) {
          this.fsm[(int)this.State, (int)theEvent].Invoke();
          //Debug.Log(this.State.ToString());
        }
        else Debug.LogError("Null Command - Current State: " + this.State.ToString() + " | Command: " + theEvent.ToString());
      }
      private void Disable() { this.State = States.Disabled; }
      private void Block() { this.State = States.Blocked; }
      private void Rest() {
        this.State = States.Resting;
      }
      private void Move() {
        //this.powerBall.OnMove();
        this.State = States.Moving; }
      private void MakeReady() { this.State = States.Ready; }
      private void Open() { this.State = States.Open; }
      private void DoNothing() { }
    }

    //curve methods
    private void ActivateArcCurve(bool showHide, PowerBallUICurve curve) {
      if (showHide) {
        float chiralityRotation = interactionHand.isLeft ? 180f : 0f;
        Vector3 outerCurvePoint = new Vector3(-.11f, -.07f, .02f);
        float handleLength = .06f;
        PositionControlPoints(UICurves[0], Quaternion.Euler(0, chiralityRotation + -90, 0), outerCurvePoint, handleLength);
      }
      curve.ActivateCurve(showHide);
    }

    private void ActivateTArcsCurves(bool showHide) {
      if (showHide) {
        float chiralityRotation = interactionHand.isLeft ? 180f : 0f;
        Vector3 outerCurvePoint = new Vector3(-.045f, -.03f, 0f);
        float handleLength = .02f;
        PositionControlPoints(UICurves[0], Quaternion.Euler(0, chiralityRotation  + - 90, 0),outerCurvePoint, handleLength);
        PositionControlPoints(UICurves[1], Quaternion.Euler(0, chiralityRotation +  0, 0),   outerCurvePoint, handleLength);
        PositionControlPoints(UICurves[2], Quaternion.Euler(0, chiralityRotation + 90, 0), outerCurvePoint, handleLength);
      }
      for (int i = 0; i < UICurves.Count; i++) UICurves[i].ActivateCurve(showHide);
    }

    private void PositionControlPoints(PowerBallUICurve c, Quaternion rotate, Vector3 outerCurvePoint, float handleLength) {
      Vector3 pinchPosition = interactionHand.leapHand.GetPinchPosition();
      c.transform.position = pinchPosition;
      c.transform.rotation = rotate; 
      //1:  Red: At pinch point on opening UI
      c.ControlPoints[0].localPosition = Vector3.zero;
      //2:  Blue: Aligned to palm X out from pinch point
      c.ControlPoints[1].localPosition = Vector3.zero + new Vector3(-handleLength, 0, 0);
      //4:  Red: along palm x
      c.ControlPoints[3].localPosition = Vector3.zero + outerCurvePoint;
      //3:  Green: out from end point along palm -Y
      c.ControlPoints[2].localPosition = c.ControlPoints[3].localPosition + Vector3.zero + new Vector3(0, handleLength, 0);
    }


    private Transform NearestDot(Vector3 originalPosition) {
      List<Transform> dots = new List<Transform>();
      if (UIStyle == UItype.Quadrants) {
        dots = DotGroups[CurrentShortCut];
      }
      if (UIStyle == UItype.TArcs) {
        for (int i = 0; i < UICurves.Count; i++) {
          dots.AddRange(UICurves[i].CurvePoints);
        }
      }
      Transform nearestDot = dots[0]; //this is the dots' PARENT position!!!
      float closestDistance = (originalPosition - dots[0].position).magnitude;
      for (int i = 0; i < dots.Count; i++) {
        float testDistance = (originalPosition - dots[i].position).magnitude;
        if (testDistance < closestDistance) {
          closestDistance = testDistance;
          nearestDot = dots[i];
        }
      }
      return nearestDot;
    }

    private void OnDisable() {
      OnDisablePowerBall();
    }

    private void OnEnable() {
      OnEnablePowerBall();
    }

    public Quaternion AlignFacingGoal(float facingblendWeight) {
      Vector3 cameraDirection = VRCamera.position - Palm.position;
      Quaternion facingUp = Quaternion.LookRotation(Vector3.up);

      Vector3 projectedArmDirection = Palm.position - interactionHand.leapHand.Arm.Direction.ToVector3();
      Vector3 HandDirectionAtHandHeight = new Vector3(projectedArmDirection.x, Palm.position.y, projectedArmDirection.z) - Palm.position;

      Quaternion upHandAligned = Quaternion.LookRotation(HandDirectionAtHandHeight, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);//this can be used if using CamDirectionAtHandHeight * Quaternion.Euler(0f, 0f, -60f);
      Quaternion blendedRotation = Quaternion.Lerp(upHandAligned, Quaternion.LookRotation(cameraDirection), facingblendWeight);
      return blendedRotation;
    }
  }
}

using Leap.Unity.Animation;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity;
using System;
using System.Linq;

namespace LeapSingleHandedShortcuts {
  [RequireComponent(typeof(PowerBall))]
  public class PowerBallAffordance : MonoBehaviour {

    private AudioSource AudioOneShots;
    private AudioSource AudioFlip;
    private AudioSource AudioSquish;
    private AudioSource AudioOptionHum;
    public AudioClip flippingSound;             //Continuous during Moving state
    public AudioClip ReadySound;                //Move => Ready
    public AudioClip SquishSound;               //Continuous during Ready
    public AudioClip PowerBallActivationSound;  //Ready => Open - OnHiLightPowerBall
    public AudioClip DragTickSound;             //Continuous during Open
    public AudioClip PowerSelectionSound;       //OnSelectShortCut
    public AudioClip PowerActivationSound;      //OnPowerActivate
    public AudioClip OptionHum;
    public float HumVolumeMax = 1.0f;
    public float OptionHumPitchSpeed = .05f;

    public Renderer HandMeshRenderer;
    public PowerBall powerBall;
    public SkinnedMeshRenderer PowerBallSkinnedMeshRenderer;
    public Renderer PowerBallFrameRenderer;
    public Material PowerBallMaterial;
    public Color PowwerBallReadyColor;
    public Color PowerBallPinchColor;
    public Color PowerBallEmissiveColor;
    public Color PowerBallBlockedColor;
    public float PowerBallOpacity;
    public float PowerBallVisualScale;
    public float PowerBallBlendWeight;
    public Transform FacingAffordance;

    public TextMesh DebugHiLiteOption;

    public float PowerBallHeightOffset;
    private Color powerBallColor;
    public Color PowerBallColor {
      get {
        return powerBallColor;
      }
      set {
        powerBallColor = value;
        PowerBallMaterial.color = value;
      }
    }

    public bool IsVisible {
      get {
        return isVisible;
      }
      set {
        isVisible = value;
        //PowerBallSkinnedMeshRenderer.enabled = value;
        PowerBallFrameRenderer.enabled = true;
      }
    }

    private bool affordanceIsVisible;
    public bool AffordanceIsVisible {
      get {
        return affordanceIsVisible;
      }
      set {
        affordanceIsVisible = value;
        HandAffordanceRenderer.enabled = value;
      }
    }

    private Texture2D powerBallTexture;
    public Texture2D PowerBallTexture {
      get {
        return powerBallTexture;
      }
      set {
        powerBallTexture = value;
        PowerBallMaterial.mainTexture = value;
      }
    }

    public AnimationCurve ActivationGlowStrength;
    public AnimationCurve MoveVolumeCurve;

    private bool isVisible;

    private List<Material> OptionMaterials = new List<Material>();
    private List<SkinnedMeshRenderer> optionsSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
    public Color OptionHiLightColor;
    public Color OptionStanbyColor;
    public bool ColorPerHiLight;
    public List<Color> OptionHiLiteColors;
    public List<Color> OptionStandbyColors;
    public List<Color> OptionDisabledColors;

    [Space(10)]
    public float FingerTipGlowMaxAmount = 1f;
    public float PowerBallGlowMaxAmount = .5f;
    [Header("Palm Facing Affordance")]
    public Renderer HandAffordanceRenderer;
    public Transform HandAffordanceTransform;
    public Color HandAffordanceColor;

    [Space(10)]
    [Header("Arc Volume UI Spcific")]
    public List<GameObject> OptionObjects;

    [Space(10)]
    [Header("Quadrants Specific")]
    public List<int> BlendMapping;
    public List<Texture2D> PowerIcons;

    public Material SliderRingMat;
    public Transform SliderRing;
    public Transform SliderGrip;
    public SkinnedMeshRenderer SliderGripRenderer;

    private void Awake() {
      powerBall = GetComponent<PowerBall>();
      AudioOneShots = gameObject.AddComponent<AudioSource>() as AudioSource;
      AudioFlip = gameObject.AddComponent<AudioSource>() as AudioSource;
      AudioFlip.clip = flippingSound;
      AudioFlip.loop = true;
      AudioSquish = gameObject.AddComponent<AudioSource>() as AudioSource;
      AudioSquish.clip = SquishSound;
      AudioSquish.loop = true;
      AudioOptionHum = gameObject.AddComponent<AudioSource>() as AudioSource;
      AudioOptionHum.clip = OptionHum;
      AudioOptionHum.loop = true;
    }

    void Start() {
      PowerBallMaterial = PowerBallSkinnedMeshRenderer.GetComponentInChildren<Renderer>().material;
      foreach(GameObject obj in OptionObjects) {
        OptionMaterials.Add(obj.GetComponent<Renderer>().material);
        optionsSkinnedMeshRenderers.Add(obj.GetComponent<SkinnedMeshRenderer>());
      }
      powerBall.OnSelectShortCut.AddListener(HiLightOption);
      powerBall.OnActivatePower.AddListener(PowerActivate);
      powerBall.OnActivateDeActivatePowerBall.AddListener(EnableAffordance);
      powerBall.OnHiLightPowerBall.AddListener(HiLightPowerBall);
      powerBall.OnRest.AddListener(Rest);
      powerBall.OnMove.AddListener(Move);
      powerBall.OnMakeReady.AddListener(MakeReady);
      powerBall.OnOpen.AddListener(Open);
      powerBall.OnAudioTick.AddListener(Tick);
      
      TurnOffFacingAffordance();
    }

    [HideInInspector]
    public int OptionsTotal = 3;

    private void Update() {
      if (IsVisible == false && powerBall.PowerBallStateIndex != 0) IsVisible = true;

      switch (powerBall.PowerBallStateIndex) {
        case 0: //Disabled
          break;
        case 1: //Blocked
          DoPinchGlow();
          TurnOffFacingAffordance();
          break;
        case 2: //Resting
          DoOptionBlends(false);
          DoPinchSquish(false);
          DoPinchGlow();
          if (powerBall.PowerBallXform.localScale.z > .1f) powerBall.PowerBallXform.localScale = new Vector3(1f, 1f, .1f);
          break;
        case 3: //Moving
          DoPinchSquish(true);
          DoAccordianBlend(true);
          DoOptionBlends(false);
          float newZScale = powerBall.GetPalmFacing().Map(-.7f, .7f, .1f, 1f);
          powerBall.PowerBallXform.localScale = new Vector3(1f, 1f, newZScale);
          AffordanceAlphaFromFacing();

          float powerBallFlipPitch = Mathf.Clamp01(powerBall.GetPalmFacing()).Map(powerBall.RestingToMovingThreshold, 1f, 0f, 1f);
          AudioFlip.pitch = powerBallFlipPitch.Map(0f, 1f, 1f, 3f);
          AudioFlip.volume = MoveVolumeCurve.Evaluate(powerBallFlipPitch);
          break;
        case 4: //Ready
          if(PowerBallTexture != null) PowerBallTexture = null;
          DoOptionBlends(false);
          DoPinchSquish(true);
          DoAccordianBlend(true);
          DoPinchGlow();
          HiLightPowerBall(false);
          DoOptionAudioHum(powerBall.CurrentShortCut, 0f);
          if (powerBall.PowerBallXform.localScale.z < .8f) powerBall.PowerBallXform.localScale = Vector3.one;

          break;
        case 5: //
          DoAccordianBlend(false);
          DoOptionBlends(true);
          DoPinchSquish(true);
          DoPinchGlow();
          HiLightPowerBall(true);
          DoOptionAudioHum(powerBall.CurrentShortCut, HumVolumeMax);

          if (powerBall.UIStyle == PowerBall.UItype.SlideRing) {

            Vector3 ParentX = powerBall.Palm.position + Camera.main.transform.right * -1f;
            SliderRingMat.SetVector("_ParentZeroDirection", ParentX);
            ParentXDebug.position = ParentX;

            AimSliderParent();
            PinchGrip();
            AimSliderGrip();
            if (powerBall.interactionHand.leapHand.PinchStrength > .8f) {
              RotateRingToFollowGrip();
            }
            else {
              //ReturnSliderGrip();
              RingMomentum();
            }
          }
          break;
      }
    }
    public Transform PinchDebug;
    public TextMesh AngleDebug;
    public TextMesh DirectionDebug;
    public Transform ParentXDebug;

    private void PinchGrip() {
      SliderGripRenderer.SetBlendShapeWeight(0, powerBall.interactionHand.leapHand.PinchStrength * 100f);
    }

    private void ReturnSliderGrip() {
      SliderGrip.localEulerAngles = Vector3.Lerp(SliderGrip.localEulerAngles, new Vector3(0f, 90f, 0f), .05f);
      previousSliderGripRotation = SliderGrip.localRotation;
    }

    private void RingMomentum() {
      //rotate based on angular velocity
      SliderRing.transform.Rotate(angularVelocity);
      //decriment angular velocity
      if (angularVelocity.y > 0f) angularVelocity.y -= .1f;
      if (angularVelocity.y < 0f) angularVelocity.y += .1f;
    }

    Quaternion previousSliderGripRotation = Quaternion.identity;
    Vector3 angularVelocity = Vector3.zero;

    private void RotateRingToFollowGrip() {
      //get angular delta of sliderGrip
      Quaternion deltaGripRotation = SliderGrip.localRotation * Quaternion.Inverse(previousSliderGripRotation);

      //Calculate angular velocity
      float angle = 0.0f;
      Vector3 axis = Vector3.zero;
      deltaGripRotation.ToAngleAxis(out angle, out axis);
      angle *= Mathf.Deg2Rad;
      angularVelocity = axis * angle * (1.0f / Time.deltaTime);

      previousSliderGripRotation = SliderGrip.localRotation;

      //apply delta to ring
      SliderRing.localRotation = SliderRing.localRotation * deltaGripRotation;
    }

    private void AimSliderParent() {
      if (SliderGrip != null) {
        Vector3 pinchPosition = SliderGrip.transform.parent.parent.InverseTransformPoint(powerBall.interactionHand.leapHand.GetPinchPosition());
        pinchPosition.z = 0f;
        pinchPosition = SliderGrip.transform.parent.parent.TransformPoint(pinchPosition);
        Vector3 pinchDirection = (pinchPosition - powerBall.Palm.position).normalized;
        //Debug.DrawRay(powerBall.Palm.position, pinchDirection);
        float aimAngle = Vector3.Angle(pinchDirection, SliderGrip.transform.parent.parent.right);
        if (aimAngle < 0f || aimAngle > 80) aimAngle = 0f;
        if (aimAngle > 70f && aimAngle < 80) aimAngle = 70f;

        Quaternion ringParentTargetRotation = Quaternion.Euler(0f, 0f, aimAngle);
        SliderGrip.transform.parent.localRotation = Quaternion.Slerp(SliderGrip.transform.parent.localRotation, ringParentTargetRotation, .1f);

        PinchDebug.position = pinchPosition;
        AngleDebug.text = aimAngle.ToString("F2");
        //DirectionDebug.text = pinchDirection.ToString("F2");
        //PowerBallSkinnedMeshRenderer.enabled = false;
      }
    }

    private void AimSliderGrip() {
      if (SliderGrip != null) {
        Vector3 pinchPosition = SliderGrip.transform.parent.InverseTransformPoint(powerBall.interactionHand.leapHand.GetPinchPosition());
        pinchPosition.y = 0f;
        pinchPosition = SliderGrip.transform.parent.TransformPoint(pinchPosition);
        Vector3 pinchDirection = (pinchPosition - powerBall.Palm.position).normalized;
        //Debug.DrawRay(powerBall.Palm.position, pinchDirection);
        float aimAngle = Vector3.Angle(pinchDirection, SliderGrip.transform.parent.forward);
        Quaternion targetGripRotation = Quaternion.Euler(0f, aimAngle, 0f);
        SliderGrip.transform.localRotation = Quaternion.Slerp(SliderGrip.transform.localRotation, targetGripRotation, .5f);

        //PinchDebug.position = pinchPosition;
        //AngleDebug.text = aimAngle.ToString("F2");
        DirectionDebug.text = pinchDirection.ToString("F2");
        PowerBallSkinnedMeshRenderer.enabled = false;
      }
    }

    private void DoOptionAudioHum(float option, float targetVolume) {
      float targetPitch = 1 + powerBall.CurrentShortCut * .1F;
      AudioOptionHum.pitch = Mathf.Lerp(AudioOptionHum.pitch, targetPitch, OptionHumPitchSpeed);
      AudioOptionHum.volume = Mathf.Lerp(AudioOptionHum.volume, targetVolume * 2f, OptionHumPitchSpeed);

    }

    private void LateUpdate() {
      if (powerBall.interactionHand.isTracked) {
        FacingAffordance.rotation = powerBall.AlignFacingGoal(powerBall.FacingAffordanceBlendWeight);
        FacingAffordance.position = powerBall.Palm.position;
      }
    }

    private void Rest() {
      powerBall.PowerBallXform.localScale = new Vector3(1f, 1f, .1f);
      PowerBallTexture = PowerIcons[powerBall.CurrentShortCut];
      TurnOffFacingAffordance();
      AudioFlip.Stop();
      StartCoroutine(fadeOptionVolume());
    }

    //Called by PowerBallFSM state transitions
    private void Move(bool activateFacingAffordance) {
      //AudioFlip.clip = flippingSound;
      AudioFlip.Play();
      if(activateFacingAffordance) TurnOnFacingAffordance();
      StartCoroutine(fadeOptionVolume());
    }
    private void MakeReady(bool doAffordance) {
      AudioOneShots.PlayOneShot(ReadySound);
      //AudioFlipSquish.volume = 0f;
      //StartCoroutine(fadeFlipVolume());
      AudioFlip.Stop();
      StartCoroutine(fadeOptionVolume());
      //AudioFlip.clip = SquishSound;
      AudioSquish.Play();
      if (doAffordance) CompleteFacingAffordance();
      if (SliderRingMat != null) StartCoroutine(OpenCloseSiderRing(.01f, 1f));

    }
    private IEnumerator fadeOptionVolume() {
      float duration = .3f;
      float startVolume = AudioOptionHum.volume;
      float targetVolume = 0;
      float elapsedTime = 0f;

      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        AudioOptionHum.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
        yield return null;
      }
      AffordanceIsVisible = false;
    }
    private void Open() {
      AudioOneShots.PlayOneShot(PowerBallActivationSound);
      AudioFlip.Stop();
      AudioSquish.Stop();
      AudioOptionHum.volume = 0f;
      AudioOptionHum.Play();
      if (SliderRingMat != null) StartCoroutine(OpenCloseSiderRing(5.0f, 0f));
    }

    private void Tick() {
      AudioOneShots.PlayOneShot(DragTickSound);
    }

    private void DoPinchGlow() {
      if (HandMeshRenderer != null)
        {
          if (powerBall.PowerBallStateIndex == 0)
          {
              HandMeshRenderer.material.SetFloat("_IndexGradientAmount", 0f);
              HandMeshRenderer.material.SetFloat("_ThumbGradientAmount", 0f);
          }
          else if (powerBall.CurrentShortCut == 0 && powerBall.PowerBallStateIndex == 2)
          {
              HandMeshRenderer.material.SetFloat("_IndexGradientAmount", 0f);
              HandMeshRenderer.material.SetFloat("_ThumbGradientAmount", 0f);
          }
          else
          {
              HandMeshRenderer.material.SetFloat("_IndexGradientAmount", powerBall.PowerBallPinchStrength.Map(0f, 1f, 0f, FingerTipGlowMaxAmount));
              HandMeshRenderer.material.SetFloat("_ThumbGradientAmount", powerBall.PowerBallPinchStrength.Map(0f, 1f, 0f, FingerTipGlowMaxAmount));
          }
        }
    }

    private void DoPinchSquish(bool onOff) {
      if (onOff == true) {
        PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[5], powerBall.PowerBallPinchStrength.Map(0f, 1f, 0f, 100f));
        AudioSquish.pitch = powerBall.PowerBallPinchStrength.Map(0f, 1f, .75f, 2.0f);
        AudioSquish.volume = powerBall.PowerBallPinchStrength.Map(0f, 1f, 0f, 2.0f);
      }
      else {
        PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[5], Mathf.Lerp(PowerBallSkinnedMeshRenderer.GetBlendShapeWeight(BlendMapping[5]), 0f, .1f));
      }
    }

    private void DoAccordianBlend(bool onOff) {
      if (powerBall.GetPalmFacing() > .65f && onOff == true) {
        PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[4], powerBall.GetPalmFacing().Map(.65f, .7f, 0f, 80f));
        if(powerBall.GetPalmFacing() > .75f) PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[4], powerBall.PowerBallPinchStrength.Map(0f, .5f, 80f, 0f));
      }
      else PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[4], Mathf.Lerp(PowerBallSkinnedMeshRenderer.GetBlendShapeWeight(BlendMapping[4]), 0f, .1f));
    }

    private void DoOptionBlends(bool onOff) {
      for (int i = 0; i < 4; i++) {
        if (i == powerBall.CurrentShortCut && onOff == true) PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[i], Mathf.Lerp(PowerBallSkinnedMeshRenderer.GetBlendShapeWeight(BlendMapping[i]), 100, .1f));
        else PowerBallSkinnedMeshRenderer.SetBlendShapeWeight(BlendMapping[i], Mathf.Lerp(PowerBallSkinnedMeshRenderer.GetBlendShapeWeight(BlendMapping[i]), 0, .1f));
      }
    }
    private void AffordanceAlphaFromFacing() {
      float alpha = powerBall.GetPalmFacing().Map(.25f, .9f, 0f, .5f);
      Color targetColor = HandAffordanceRenderer.material.color;
      targetColor.a = alpha;
      HandAffordanceRenderer.material.color = targetColor;
    }
    public void HiLightOption(int newOption) {
      if (powerBall != null) {

        DebugHiLiteOption.text = newOption.ToString();
        for (int i = 0; i < powerBall.OptionsTotal; i++) {
          if (i == newOption) {
            if (ColorPerHiLight) {
              PowerBallColor = OptionHiLiteColors[i];
              PowerBallPinchColor = OptionHiLiteColors[i];
              HandAffordanceRenderer.material.color = OptionHiLiteColors[i];
              if(HandMeshRenderer!=null){
                HandMeshRenderer.material.SetColor("_GradientColor", PowerBallColor);
                HandMeshRenderer.material.color = OptionHiLiteColors[i];
              }
              if (powerBall.UIStyle == PowerBall.UItype.ArcVolumes) {
                OptionMaterials[i].color = ColorPerHiLight ? OptionHiLiteColors[i] : OptionHiLightColor;
                AudioOneShots.PlayOneShot(PowerSelectionSound);
              }

            }
          }
          else if (powerBall.UIStyle == PowerBall.UItype.ArcVolumes) {
            OptionMaterials[i].color = ColorPerHiLight ? OptionStandbyColors[i] : OptionStanbyColor;
          }
        }
      }
    }

    public void HiLightPowerBall(bool isHiLighting) {
      if (isHiLighting) {
        PowerBallColor = PowerBallPinchColor;
        PowerBallMaterial.SetColor("_EmissionColor", PowerBallPinchColor * powerBall.PowerBallPinchStrength.Map(.5f, 1f, 0f, PowerBallGlowMaxAmount) );
      }
      else {
        PowerBallColor = PowerBallPinchColor;
        PowerBallMaterial.SetColor("_EmissionColor", Color.black);

      }
    }


    public void SetDotColors(bool isEnabled, List<Transform> dotsGroup, int option) {
      foreach(Transform t in dotsGroup) {
        if (t.GetComponent<Renderer>() != null) {
          Material mat = t.GetComponent<Renderer>().material;
          if (isEnabled) mat.color = OptionStandbyColors[option];
          else mat.color = OptionDisabledColors[option];
        }
      }
    }

    public void PowerActivate(int newPower) {
      Debug.Log("PowerActivate()");
      HandMeshRenderer.material.SetColor("_BackHandColor", PowerBallColor);
      StartCoroutine(activationGlow(PowerBallPinchColor));
      AudioOneShots.PlayOneShot(PowerActivationSound);
    }

    public void TurnOffFacingAffordance() {
      HandAffordanceRenderer.enabled = false;
    }
    public void TurnOnFacingAffordance() {
      HandAffordanceRenderer.enabled = true;
      HandAffordanceTransform.localScale = new Vector3(.05f, .05f, .05f);
    }
    private IEnumerator fadePalmAffordanceCoroutine;
    public void CompleteFacingAffordance() {
      if(fadePalmAffordanceCoroutine != null)  StopCoroutine(fadePalmAffordanceCoroutine);
      fadePalmAffordanceCoroutine = FadeOutFacingAffordance();
      StartCoroutine(fadePalmAffordanceCoroutine);
    }

    //fade in hand affordance
    private IEnumerator FadeInFacingAffordance() {
      float duration = .5f;
      float startAlpha = 0f;
      float elapsedTime = 0f;

      Color startColor = HandAffordanceColor;
      startColor.a = startAlpha;

      HandAffordanceTransform.localScale = new Vector3(.05f, .05f, .05f);
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        HandAffordanceRenderer.material.color = Color.Lerp(startColor, HandAffordanceColor, elapsedTime / duration);
        yield return null;
      }
    }

    //fado out and scale hand affordance
    private IEnumerator FadeOutFacingAffordance() {
      float duration = .3f;
      float startAlpha = 0f;
      float targetAlpha = .5f;
      float elapsedTime = 0f;

      Color targetColor = HandAffordanceRenderer.material.color;
      targetColor.a = 0f;

      Vector3 startScale = new Vector3(.05f, .05f, .05f);
      Vector3 targetScale = new Vector3(.05f, .05f, .05f) * 1.5f;

      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        HandAffordanceRenderer.material.color = Color.Lerp(HandAffordanceColor, targetColor, elapsedTime / duration);
        HandAffordanceTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
        yield return null;
      }
      AffordanceIsVisible = false;
    }

    private IEnumerator activationGlow(Color color) {
      float duration = 1;
      float elapsedTime = 0f;
      Color targetColor = color;
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        if(HandMeshRenderer!=null)
          HandMeshRenderer.material.SetColor("_EmissionBase", color * ActivationGlowStrength.Evaluate(elapsedTime / duration));
        yield return null;
      }
    }

    public void ResetHandColor() {
      StopAllCoroutines();
      if (HandMeshRenderer != null)
      {
        HandMeshRenderer.material.SetColor("_BackHandColor", OptionHiLiteColors[powerBall.CurrentShortCut]);
        HandMeshRenderer.material.SetColor("_EmissionBase", Color.black);
      }
                
    }

    private void EnableAffordance(bool isEnabled) {
      if (isEnabled == true) {
        Debug.Log("EnableAffordance(true)");

        PowerBallColor = PowerBallPinchColor;
        ResetHandColor();
        StopAllCoroutines();
        StartCoroutine(activationGlow(PowerBallPinchColor));
        IsVisible = true;
        AffordanceIsVisible = false;
        AudioFlip.enabled = true;
        AudioOneShots.enabled = true;
      }
      if(isEnabled == false) {
        Debug.Log("EnableAffordance(false)");
        StopAllCoroutines();
        StartCoroutine(activationGlow(Color.white));
        if (HandMeshRenderer != null)
          HandMeshRenderer.material.SetColor("_BackHandColor", PowerBallBlockedColor);
        PowerBallColor = PowerBallBlockedColor;
        IsVisible = false;
        TurnOffFacingAffordance();
        AudioFlip.enabled = false;
        AudioOneShots.enabled = true;
        StartCoroutine(fadeOptionVolume());
      }
    }

    private IEnumerator OpenCloseSiderRing(float duration, float targetAngle) {
      float elapsedTime = 0f;
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        SliderRingMat.SetFloat("_TopAngle", Mathf.Lerp(SliderRingMat.GetFloat("_TopAngle"), targetAngle, (elapsedTime / duration)));
        yield return null;
      }
    }

  }
}

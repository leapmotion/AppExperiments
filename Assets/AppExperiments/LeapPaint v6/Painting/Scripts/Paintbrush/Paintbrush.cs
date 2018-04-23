using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Gestures;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Streams;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Leap.Unity.UserContext;

namespace Leap.Unity.Drawing {

  public class Paintbrush : MonoBehaviour,
                            IRuntimeGizmoComponent,
                            IStreamReceiver<Pose>,
                            IStream<StrokePoint>   {

    #region Inspector

    [Header("Activation Gesture")]
    [SerializeField]
    private MonoBehaviour _activationGesture;
    public IGesture activationGesture {
      get { return _activationGesture as IGesture; }
    }

    [Header("Eligibility Switch")]
    [SerializeField]
    [ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _eligibilitySwitch;
    public IPropertySwitch eligibilitySwitch {
      get { return _eligibilitySwitch as IPropertySwitch; }
    }

    [Header("Brush Settings")]
    [Range(0.005f, 0.04f)]
    public float radius = 0.04f;
    public Color color = Color.red;

    [Header("Ucon Input Channels")]
    public ColorChannel  colorChannelIn = new ColorChannel("brush/color");
    public FloatChannel  radiusChannelIn = new FloatChannel("brush/radius");
    [Tooltip("Output mesh reference frame that the brush outputs to.\n"
           + "This is necessary for TRS painting to work.")]
    public MatrixChannel outputFrameChannelIn = new MatrixChannel("brush/frame");

    [Header("Brush Tip (Optional)")]
    public Transform tipTransform = null;

    [Header("Feedback")]

    [Tooltip("If non-null, a material instance will be created from this renderer's "
           + "material and it will be set to the brush's current color on Update.")]
    public Renderer tipRendererForColor = null;
    private Color? _lastColor = null;
    private Material _tipMaterialInstance = null;

    [Tooltip("If non-null, a material instance will be created from this renderer's "
           + "material and it will be set to the brush's current color on Update.")]
    public Renderer brushHeadColorRenderer = null;
    public string brushHeadColorPropertyName = "_OutlineColor";
    private int _brushHeadColorPropId = -1;
    private Material _brushHeadMaterialInstance = null;
    public Color nonPaintingBrushHeadColor = Color.white;

    public UnityEvent OnPaintingBeginEvent;

    [Header("Debug")]
    public bool drawDebug = false;
    public bool drawDebugIdlePaths = false;

    #endregion

    #region Paintbrush

    public bool isPainting {
      get {
        return _isStreamOpen;
      }
    }

    public Pose GetLeftEdgePose() {
      return GetLeftEdgePose(transform.ToPose());
    }
    public Pose GetLeftEdgePose(Pose brushPose) {
      var tipPose = GetTipPose(brushPose);
      var edgePosition = tipPose.position + tipPose.rotation * -Vector3.right * radius;
      return new Pose(edgePosition, tipPose.rotation);
    }

    public Pose GetRightEdgePose() {
      return GetRightEdgePose(transform.ToPose());
    }
    public Pose GetRightEdgePose(Pose brushPose) {
      var tipPose = GetTipPose(brushPose);
      var edgePosition = tipPose.position + tipPose.rotation * -Vector3.right * radius;
      return new Pose(edgePosition, tipPose.rotation);
    }

    public Pose GetTipPose() {
      return GetTipPose(transform.ToPose());
    }
    public Pose GetTipPose(Pose brushPose) {
      var tipOffset = Pose.identity;
      if (tipTransform != null) {
        tipOffset = tipTransform.ToPose().From(transform.ToPose());
      }
      return brushPose.Then(tipOffset);
    }

    #endregion

    #region Unity Events

    protected virtual void Start() {
      colorChannelIn.Set(this.color);
    }

    protected virtual void OnEnable() {
      if (eligibilitySwitch != null && activationGesture != null) {
        if (!activationGesture.isEligible) {
          eligibilitySwitch.OffNow();
        }
      }

      if (_brushHeadColorPropId == -1) {
        _brushHeadColorPropId = Shader.PropertyToID(brushHeadColorPropertyName);
      }


    }

    protected virtual void Update() {
      if (eligibilitySwitch != null && activationGesture != null) {
        var shouldBeOn = activationGesture.isEligible;
        if (eligibilitySwitch.GetIsOnOrTurningOn() && !shouldBeOn) {
          eligibilitySwitch.Off();
        }
        else if (eligibilitySwitch.GetIsOffOrTurningOff() && shouldBeOn) {
          eligibilitySwitch.On();
        }
      }

      // Update the color at the color receiving channel.
      this.color = colorChannelIn.Get();

      // Update the radius at the radius receiving channel.
      this.radius = radiusChannelIn.Get();

      // If we have a tip renderer reference, always set its color to match the current
      // color of the brush.
      if (_lastColor.HasValue && this.color != _lastColor) {
        if (tipRendererForColor != null) {
          _tipMaterialInstance = tipRendererForColor.material;
        }
        if (_tipMaterialInstance != null) {
          _tipMaterialInstance.color = this.color;
        }
      }
      else {
        _lastColor = this.color;
      }

      // If we have brush head renderer reference, set its color depending on whether
      // or not we're painting.
      var targetBrushHeadColor = nonPaintingBrushHeadColor;
      if (isPainting) {
        targetBrushHeadColor = this.color;
      }
      if (brushHeadColorRenderer != null) {
        _brushHeadMaterialInstance = brushHeadColorRenderer.material;
      }
      if (_brushHeadMaterialInstance != null) {
        _brushHeadMaterialInstance.SetColor(_brushHeadColorPropId, targetBrushHeadColor);
      }
    }

    #endregion

    #region IStream<StrokePoint>

    public event Action OnOpen  = () => { };
    public event Action<StrokePoint> OnSend = (strokePoint) => { };
    public event Action OnClose = () => { };

    private bool _isStreamOpen = false;

    #endregion

    #region IStreamReceiver<Pose>

    public void Open() {
      _debugPoseBuffer.Clear();
    }

    public void Receive(Pose data) {
      transform.SetPose(data);

      bool shouldBePainting = false;
      if (this.enabled && this.gameObject.activeInHierarchy) {
        shouldBePainting = activationGesture != null && activationGesture.isActive;

        /* Debug */
        {
          var tipPose = GetTipPose(data);
          _debugPoseBuffer.Add(tipPose);
          _debugActivatedBuffer.Add(shouldBePainting);
        }
      }

      if (shouldBePainting) {
        if (!_isStreamOpen) {
          OnOpen();
          _isStreamOpen = true;

          OnPaintingBeginEvent.Invoke();
        }

        var refFrame = outputFrameChannelIn.Get();

        var tipPose = GetTipPose(data);
        OnSend(new StrokePoint() {
          pose = tipPose,
          radius = radius,
          color = color,
          temp_refFrame = refFrame,
        });
      }
      else if (_isStreamOpen) {
        OnClose();
        _isStreamOpen = false;
      }
    }

    public void Close() {

    }

    #endregion

    #region Runtime Gizmos

    private const int DEBUG_BUFFER_SIZE = 16;
    private RingBuffer<Pose> _debugPoseBuffer = new RingBuffer<Pose>(DEBUG_BUFFER_SIZE);
    private RingBuffer<bool> _debugActivatedBuffer = new RingBuffer<bool>(DEBUG_BUFFER_SIZE);

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy || !drawDebug) return;

      drawer.color = LeapColor.purple;

      var poseRadius = 0.004f;

      if (!Application.isPlaying) {
        _debugPoseBuffer.Clear();
        _debugPoseBuffer.Add(GetTipPose());
        _debugActivatedBuffer.Clear();
        _debugActivatedBuffer.Add(false);
      }
      for (int i = 0; i < _debugPoseBuffer.Count; i++) {
        var pose = _debugPoseBuffer.Get(i);
        var isActive = _debugActivatedBuffer.Get(i);

        var a = pose.position + pose.rotation * Vector3.right * radius;
        var b = pose.position - pose.rotation * Vector3.right * radius;

        var multiplier = 1f;
        if (isActive) multiplier = 2.5f;

        if (isActive || drawDebugIdlePaths || !Application.isPlaying) {
          drawer.DrawPose(new Pose(a, pose.rotation), poseRadius * multiplier);
          drawer.DrawPose(new Pose(b, pose.rotation), poseRadius * multiplier);
        }
      }
    }

    #endregion

    #region Public Methods
    
    public void SetRadius(float radius) {
      this.radius = radius;
    }

    private Action<Color> _setColorFunc = null;
    public Action<Color> setColorFunc {
      get { return _setColorFunc = _setColorFunc ?? SetColor; }
    }
    public void SetColor(Color color) {
      this.color = color;
    }

    #endregion

  }

}
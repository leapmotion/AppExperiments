//using Leap.Unity.Attributes;
//using Leap.Unity.Interaction;
//using UnityEngine;

//namespace Leap.Unity {

//  //public class TabButton : MonoBehaviour {

//  //  public InteractionBehaviour intObj;

//  //  private Rigidbody _body;

//  //  [Header("Angle Comparison Activation")]

//  //  public Vector3 tabLocalVector;
//  //  public Vector3 parentLocalVector;
//  //  public float toggleWhenAngleExceeds = 50f;
//  //  public float relaxWhenAngleLessThan = 20f;

//  //  private bool _requiresRelaxation = false;

//  //  private bool _isToggled;

//  //  [Header("Feedback")]

//  //  [EditTimeOnly]
//  //  public Renderer driveRenderer = null;
//  //  [EditTimeOnly]
//  //  public string colorPropertyName = "_Color";

//  //  private Material _materialInstance;
//  //  private int _colorPropertyId = -1;

//  //  public float colorSpeedPerSec = 0.10f;

//  //  public Color _restColor = Color.black;
//  //  public Color _primaryHoverColor = Color.gray;
//  //  public Color _toggledColor = Color.white;
//  //  public Color _toggledPrimaryHoverColor = Color.white;

//  //  private Color _currentColor;

//  //  private void Reset() {
//  //    if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
//  //  }
//  //  private void OnValidate() {
//  //    if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
//  //  }

//  //  private void Start() {
//  //    if (driveRenderer != null) {
//  //      _materialInstance = driveRenderer.material;
//  //    }

//  //    _colorPropertyId = Shader.PropertyToID(colorPropertyName);
//  //  }

//  //  private void FixedUpdate() {
//  //    var thisVector = (_body.GetPose() * tabLocalVector).position;
//  //    var parentVector = (this.transform.parent.ToPose() * parentLocalVector).position;

//  //    var angle = Vector3.Angle(thisVector, parentVector);

//  //    if (!_requiresRelaxation) {
//  //      if (angle > toggleWhenAngleExceeds) {
//  //        _isToggled = true;

//  //        _requiresRelaxation = true;
//  //      }
//  //    }
//  //    else {
//  //      if (angle < relaxWhenAngleLessThan) {
//  //        _requiresRelaxation = false;
//  //      }
//  //    }
//  //  }

//  //  private void Update() {
//  //    //updateColor();
//  //  }

//  //  //private void updateColor() {
//  //  //  var targetColor = _restColor;

//  //  //  bool isToggled = this._isToggled;

//  //  //  if (intObj.isPrimaryHovered) {
//  //  //    if (isToggled) {
//  //  //      targetColor = _toggledPrimaryHoverColor;
//  //  //    }
//  //  //    else {
//  //  //      targetColor = _primaryHoverColor;
//  //  //    }
//  //  //  }
//  //  //  else {
//  //  //    if (isToggled) {
//  //  //      targetColor = 
//  //  //    }
//  //  //  }
//  //  }

//  }

//}

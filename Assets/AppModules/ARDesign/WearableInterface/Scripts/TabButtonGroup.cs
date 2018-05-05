using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class TabButtonGroup : MonoBehaviour {

    public HingeElement[] hingeElements;

    [Header("Palette Colors (Elements)")]
    public int restingColorIdx = 1;
    public int activeColorIdx = 3;
    public int hoverColorIdx = 1;
    public int activeHoverColorIdx = 3;

    [Header("Palette Colors (Text)")]
    public int restingTextColorIdx = 4;
    public int activeTextColorIdx = 1;
    public int hoverTextColorIdx = 5;
    public int activeHoverTextColorIdx = 1;

    private IPaletteColorReceiver[] _colorReceivers;
    private IPaletteColorReceiver[] _textColorReceivers;

    private float _activationAngle = -20f;

    //private bool[] _isInteracting;
    private float[] _prevAngles;
    private float[] _angles;

    private int _activeIdx = -1;

    private List<IPaletteColorReceiver> _colorReceiversBuffer
      = new List<IPaletteColorReceiver>();

    void Start() {
      //_isInteracting = new bool[hingeElements.Length];
      _prevAngles = new float[hingeElements.Length];
      _angles = new float[hingeElements.Length];

      // Find color receivers and text color receievers.
      _colorReceivers = new IPaletteColorReceiver[hingeElements.Length];
      _textColorReceivers = new IPaletteColorReceiver[hingeElements.Length];
      for (int i = 0; i < hingeElements.Length; i++) {
        hingeElements[i].GetComponentsInChildren(_colorReceiversBuffer);

        foreach (var colorReceiver in _colorReceiversBuffer) {
          var monoBehaviour = colorReceiver as MonoBehaviour;
          if (monoBehaviour != null) {
            var text = monoBehaviour.GetComponent<TextMesh>();
            if (text != null) {
              _textColorReceivers[i] = colorReceiver;
            }
            else {
              if (_colorReceivers[i] == null) {
                // We only want the _first_ color receiver component, this
                // is just known based on our component setup. This script
                // is super not general!!
                _colorReceivers[i] = colorReceiver;
              }
            }
          }
        }
      }
    }
    
    void Update() {

      if (Input.GetKeyDown(KeyCode.X)) {
        _activeIdx = -1;
      }

      for (int i = 0; i < hingeElements.Length; i++) {
        var hingeElement = hingeElements[i];
        _prevAngles[i] = _angles[i];
        _angles[i] = hingeElement.angle;
        //_isInteracting[i] = hingeElement.isInteracting;

        var prevAngle = _prevAngles[i];
        var angle = _angles[i];
        //var isInteracting = _isInteracting[i];
        var isHovered = hingeElements[i].isHovered;

        if (prevAngle >= _activationAngle && angle < _activationAngle && isHovered) {
          _activeIdx = i;
        }

        var colorReceiver = _colorReceivers[i];
        if (colorReceiver != null) {
          if (isHovered) {
            colorReceiver.Receive(hoverColorIdx);
          }
          else {
            colorReceiver.Receive(restingColorIdx);
          }
        }

        var textColorReceiver = _textColorReceivers[i];
        if (textColorReceiver != null) {
          if (isHovered) {
            textColorReceiver.Receive(hoverTextColorIdx);
          } else {
            textColorReceiver.Receive(restingTextColorIdx);
          }
        }
      }

      if (_activeIdx != -1) {
        var activeElement = hingeElements[_activeIdx];
        var isHovered = activeElement.isHovered;
        var activeColorReceiver = _colorReceivers[_activeIdx];
        if (activeColorReceiver != null) {
          if (isHovered) {
            activeColorReceiver.Receive(activeHoverColorIdx);
          }
          else {
            activeColorReceiver.Receive(activeColorIdx);
          }
        }
        var activeTextReceiver = _textColorReceivers[_activeIdx];
        if (activeTextReceiver != null) {
          if (isHovered) {
            activeTextReceiver.Receive(activeHoverTextColorIdx);
          }
          else {
            activeTextReceiver.Receive(activeTextColorIdx);
          }
        }
      }
    }

  }

}

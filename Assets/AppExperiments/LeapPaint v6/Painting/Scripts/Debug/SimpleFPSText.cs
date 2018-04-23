using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFPSText : MonoBehaviour {

  public TextMesh textOutput;

  private float _updateInterval = 0.5f;
  private float _accumFPS = 0;
  private int   _framesDrawn = 0;
  private float _updateTimer = 0f;

  void Update() {
    _updateTimer += Time.deltaTime;
    _accumFPS += Time.timeScale / Time.deltaTime;
    _framesDrawn += 1;

    if (_updateTimer >= _updateInterval) {
      var avgFPS = _accumFPS / _framesDrawn;

      textOutput.text = "FPS: " + avgFPS.ToString("F2");

      _accumFPS = 0f;
      _framesDrawn = 0;
      _updateTimer = 0f;
    }

    var targetFPS = Application.targetFrameRate;
  }

}

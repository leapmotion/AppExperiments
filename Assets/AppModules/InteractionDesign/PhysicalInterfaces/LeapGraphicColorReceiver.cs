using Leap.Unity.GraphicalRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class LeapGraphicColorReceiver : MonoBehaviour, IColorReceiver {

    public LeapGraphic graphic;

    public float colorChangeSpeed = 20f;

    private Color _targetColor;

    private void Reset() {
      graphic = GetComponent<LeapGraphic>();
    }

    public void Receive(Color color) {
      _targetColor = color;
    }

    void Update() {
      var color = graphic.GetRuntimeTint();

      var sqrColorDist = getSqrColorDist(color, _targetColor);

      if (sqrColorDist == 0f) return;
      else if (sqrColorDist < 0.01f * 0.01f) {
        color = _targetColor;
      }
      else {
        color = Color.Lerp(color, _targetColor, colorChangeSpeed * Time.deltaTime);
      }

      graphic.SetRuntimeTint(color);
    }

    private float getSqrColorDist(Color a, Color b) {
      var d = (a - b);
      return (d.r * d.r) + (d.g * d.g) + (d.b * d.b);
    }

  }

}

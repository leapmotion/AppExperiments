using Leap.Unity.Attributes;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class RendererReceiveColor_UconChannel : PeriodicBehaviour {

    public Renderer rendererToSet;
    private Material _materialInstance = null;

    [EditTimeOnly]
    public string shaderPropertyName = "_Color";
    private int _shaderPropertyId = -1;

    [Header("Ucon Color Channel In")]
    public ColorChannel colorChannel = new ColorChannel("tool/color");

    [Disable]
    public Color lastReceivedColor;

    private void Reset() {
      if (rendererToSet == null) {
        rendererToSet = GetComponentInChildren<Renderer>();
      }
    }

    private void Start() {
      _shaderPropertyId = Shader.PropertyToID(shaderPropertyName);

      if (rendererToSet != null) {
        _materialInstance = rendererToSet.material;
      }

      this.updatePeriod = 4;
    }

    public override void PeriodicUpdate() {
      lastReceivedColor = colorChannel.Get();
      
      if (_materialInstance != null) {
        _materialInstance.SetColor(_shaderPropertyId, lastReceivedColor);
      }
    }
  }

}

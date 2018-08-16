using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.AppExperiments.TrueVolumetrics {

  /// <summary>
  /// Most HandModels have a HandEnableDisable script to automatically
  /// turn on and off hand rendering as tracking is gained and lost.
  /// This script specifically handles the same behavior for Volumetric Hands.
  /// </summary>
  public class TrueVolumetricHandEnableDisable : HandTransitionBehavior {

    public Renderer trueVolumetricHandRenderer;

    [EditTimeOnly]
    public string trackedFloatName = "_IsTracked";
    
    private Material _material;
    private int _trackedFloatPropId = -1;

    protected virtual void Start() {
      if (trueVolumetricHandRenderer != null) {
        _material = trueVolumetricHandRenderer.sharedMaterial;
        _trackedFloatPropId = Shader.PropertyToID(trackedFloatName);
      }
    }

    protected override void HandReset() {
      // Hand begun tracking.
      trueVolumetricHandRenderer.enabled = true;
      _material.SetFloat(_trackedFloatPropId, 1);
    }

    protected override void HandFinish() {
      // Hand stopped tracking.
      trueVolumetricHandRenderer.enabled = false;
      _material.SetFloat(_trackedFloatPropId, 0);
    }

  }

}

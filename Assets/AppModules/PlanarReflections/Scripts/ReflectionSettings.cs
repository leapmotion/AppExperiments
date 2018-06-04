using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.DevGui;

public class ReflectionSettings : MonoBehaviour {

  [Range(0, 1)]
  [DevValue]
  public float blurriness = 0.5f;

  [Range(0, 1)]
  [DevValue]
  public float minBlur = 0;

  [Range(0, 0.5f)]
  [DevValue]
  public float blurDistance = 0.1f;

  [Range(0, 1)]
  [DevValue]
  public float strength = 0.05f;

  private Material _material;
  private PlanarReflections _reflection;

  private void Awake() {
    _material = GetComponent<Renderer>().material;
    _reflection = GetComponent<PlanarReflections>();
  }

  private void Update() {
    _material.SetFloat("_Bunpiness", blurriness);
    _material.SetFloat("_MinBumpiness", minBlur);
    _material.color = _material.color.WithAlpha(1.0f - strength);
    _reflection.distortDistance = blurDistance;
  }

}

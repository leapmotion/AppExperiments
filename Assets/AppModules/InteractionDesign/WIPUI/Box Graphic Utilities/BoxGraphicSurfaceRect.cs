using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class BoxGraphicSurfaceRect : MonoBehaviour {

  public LeapBoxGraphic parentBox;

  public float extraDepth = 0.0001F;

  [SerializeField, Disable]
  private float _parentBoxDepth = 0F;

  private RectTransform _rectTransform;

  void Reset() {
    _rectTransform = GetComponent<RectTransform>();
    if (_rectTransform == null) {
      _rectTransform = this.gameObject.AddComponent<RectTransform>();
    }

    parentBox = this.transform.parent.GetComponent<LeapBoxGraphic>();
  }

  void Update() {
    if (parentBox != null) {
      _parentBoxDepth = parentBox.size.z;
      this.transform.localPosition = new Vector3(this.transform.localPosition.x,
                                                 this.transform.localPosition.y,
                                                 -_parentBoxDepth - extraDepth);
    }
  }

}

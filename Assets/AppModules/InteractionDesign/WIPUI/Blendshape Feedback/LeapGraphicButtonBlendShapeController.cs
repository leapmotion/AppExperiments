using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapGraphicButtonBlendShapeController : MonoBehaviour {

  public InteractionButton button;
  public LeapGraphic graphic;

  [SerializeField, OnEditorChange("setPulsatorSpeed")]
  [MinValue(0.001F)]
  private float _speed = 3F;
  public float speed { get { return _speed; } set { _speed = value; setPulsatorSpeed(); } }

  protected Pulsator _scalePulsator;

  protected virtual void Reset() {
    button = GetComponent<InteractionButton>();
    graphic = GetComponent<LeapGraphic>();
  }

  protected virtual void OnEnable() {
    _scalePulsator = Pool<Pulsator>.Spawn().SetValues(0F, 0.2F, 0F);

    button.OnPress   += onPress;
    button.OnUnpress += onUnpress;
  }

  private void setPulsatorSpeed() {
    if (_scalePulsator != null) _scalePulsator.SetSpeed(3F);
  }

  protected virtual void OnDisable() {
    Pool<Pulsator>.Recycle(_scalePulsator);

    button.OnPress -= onPress;
    button.OnUnpress -= onUnpress;
  }

  protected virtual void Update() {
    if (graphic != null && button != null) {
      try {
        graphic.SetBlendShapeAmount(button.pressedAmount.Map(0F, 1F, 0F, 0.8F) + _scalePulsator.value);
      }
      catch (System.Exception) {
        Debug.LogError("Error setting blend shape. Does the attached graphic have a blend shape feature?", this);
      }
    }
  }

  private void onPress() {
    _scalePulsator.Pulse();
  }

  private void onUnpress() {
    _scalePulsator.Relax();
  }

}

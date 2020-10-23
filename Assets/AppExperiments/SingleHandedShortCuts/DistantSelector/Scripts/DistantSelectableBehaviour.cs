using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace LeapSingleHandedShortcuts {
  public class DistantSelectableBehaviour : MonoBehaviour {
    private Renderer renderer;
    private Color startColor;
    public Color SelectedColor;
    public DistantSelector distantSelector;

    // Use this for initialization
    void Start() {
      renderer = gameObject.GetComponent<Renderer>();
      distantSelector = GameObject.FindObjectOfType<DistantSelector>();
      distantSelector.OnDeselectAll.AddListener(OnDistantDeselect);
      startColor = renderer.material.color;
      SelectedColor = distantSelector.SelectedRayColor.Evaluate(1);
    }

    // Update is called once per frame
    void Update() {

    }
    public void OnDistantHoverBegin() { }
    public void OnDistantHoverEnd() { }
    public void OnDistantSelect() {
      renderer.material.color = SelectedColor;
      renderer.material.SetColor("_EmissionColor", Color.blue);
    }
    public void OnDistantDeselect() {
      renderer.material.color = startColor;
      renderer.material.SetColor("_EmissionColor", Color.black);
    }
  }
}



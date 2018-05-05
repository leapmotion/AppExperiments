using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class HideWhenXScaleVerySmall : MonoBehaviour
  {

    public Renderer toToggle;

    private void Reset()
    {
      if (toToggle == null) toToggle = GetComponent<Renderer>();
    }

    private void Start()
    {
      Updater.instance.OnUpdate += onUpdate;
    }

    private void onUpdate()
    {
      if (this.transform.lossyScale.x < 0.001f) {
        toToggle.enabled = false;
      }
      else {
        toToggle.enabled = true;
      }
    }

  }

}

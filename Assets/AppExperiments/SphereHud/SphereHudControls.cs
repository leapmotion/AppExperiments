using UnityEngine;
using Leap.Unity;
using Leap.Unity.DevGui;

public class SphereHudControls : MonoBehaviour {

  [DevValue, Range(0.2f, 1.0f)]
  public float radius;

  [DevValue, Range(-0.5f, 0.5f)]
  public float xOffset;

  [DevValue, Range(-0.5f, 0.5f)]
  public float yOffset;

  [DevValue, Range(-0.5f, 0.5f)]
  public float zOffset;

  [DevValue]
  public bool autoFollowHead;

  [DevValue, Range(0, 0.2f)]
  public float minDist = 0.01f;

  [DevValue, Range(0, 0.2f)]
  public float maxDist = 0.06f;

  private void Update() {
    transform.localScale = Vector3.one * radius * 2;

    if (autoFollowHead) {
      updateSphereLocation();
    }

    GetComponent<Renderer>().material.SetVector("_ProximityMapping", new Vector4(minDist, maxDist, 1, 0));
  }

  [DevButton("Recenter")]
  private void updateSphereLocation() {
    Transform target = Hands.Provider.transform;

    transform.position = target.position + target.right * xOffset +
                                           target.up * yOffset +
                                           target.forward * zOffset;
  }

}

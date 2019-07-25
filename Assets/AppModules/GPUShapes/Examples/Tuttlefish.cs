using UnityEngine;
using Leap.Unity.Attachments;


public class Tuttlefish : MonoBehaviour {
  void Start() {

  }

  void OnRenderObject() {
    CommandBufferRendering.Render(Camera.current);
  }

  void Update() {

  }

  private void OnDisable() {
    StaticMaterials.Destroy();
    StaticComputeBuffers.Dispose();
  }
}
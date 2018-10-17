using UnityEngine;

public class UpdateSkinnedMeshOnly : MonoBehaviour {

  public SkinnedMeshRenderer trueVolumetricHandRenderer;

  protected virtual void Start() {
    if (trueVolumetricHandRenderer == null) {
      trueVolumetricHandRenderer = GetComponent<SkinnedMeshRenderer>();
    }
  }

  void Update() { trueVolumetricHandRenderer.enabled = true; }

  void OnWillRenderObject() { trueVolumetricHandRenderer.enabled = false; }

}

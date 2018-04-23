using UnityEngine;
using UnityEngine.Networking;

public class CameraDisabler : NetworkBehaviour {
  public Camera camToDisable;
  public Behaviour[] toDisable;
  public MeshRenderer[] toEnable;
  public SkinnedMeshRenderer[] toSwapMaterials;
  public Material toSwap;
	void Start () {
    camToDisable.eventMask = 0;
    camToDisable.enabled = isLocalPlayer;
    foreach (Behaviour component in toDisable) {
      component.enabled = isLocalPlayer;
    }
    foreach (MeshRenderer component in toEnable) {
      component.enabled = !isLocalPlayer;
    }
    if (!isLocalPlayer) {
      foreach (SkinnedMeshRenderer mesh in toSwapMaterials) {
        mesh.material = toSwap;
      }
    }
	}
}

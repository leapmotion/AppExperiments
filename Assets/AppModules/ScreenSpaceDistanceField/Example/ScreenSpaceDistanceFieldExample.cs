using UnityEngine;

[ExecuteInEditMode]
public class ScreenSpaceDistanceFieldExample : MonoBehaviour {

  public Material compositeMat;
  public JumpFlood jumpFlood;

  private void OnRenderImage(RenderTexture source, RenderTexture destination) {
    var distanceTex = jumpFlood.BuildDistanceField(source);

    Graphics.Blit(source, destination);
    Graphics.Blit(distanceTex, destination, compositeMat);

    RenderTexture.ReleaseTemporary(distanceTex);
  }
}

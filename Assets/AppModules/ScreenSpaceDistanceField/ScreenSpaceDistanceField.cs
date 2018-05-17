using System;
using UnityEngine;

[Serializable]
public class JumpFlood {
  public const int PASS_INIT = 0;
  public const int PASS_JUMP = 1;

  [Tooltip("The number of jump steps that should be performed.  Each extra " +
           "step roughly doubles the distance from the surface at which you " +
           "can extract accurate distance information.")]
  [Range(0, 32)]
  public int steps = 12;

  private Material _material;

  private bool tryInitMaterial() {
    if (_material != null) {
      return true;
    }

    var shader = Shader.Find("Hidden/ScreenSpaceDistanceField");
    if (shader == null) {
      return false;
    }

    _material = new Material(shader);
    _material.hideFlags = HideFlags.HideAndDontSave;
    _material.name = "Distance Field Generation Material";
    return true;
  }

  private RenderTexture getTemp(RenderTexture sourceTex) {
    var tex = RenderTexture.GetTemporary(sourceTex.width,
                                         sourceTex.height,
                                         0,
                                         RenderTextureFormat.ARGBFloat,
                                         RenderTextureReadWrite.Linear);
    tex.wrapMode = TextureWrapMode.Clamp;
    return tex;
  }

  /// <summary>
  /// Given a render texture, returns a render texture of the same dimensions.
  /// The returned render texture has the type ARGBFloat.  The components have
  /// the following meanings:
  ///   X: x distance to closest surface point
  ///   Y: y distance to closest surface point
  ///   Z: squared distance to closest surface point
  ///   W: zero if outside the surface, nonzero if inside
  ///   
  /// The units of X Y and Z are all normalized to the HEIGHT of the texture.
  /// If Z is a distance of 1, that means that the closest surface is HEIGHT
  /// pixels away.
  /// 
  /// The surface of the input texture is determined by thresholding the alpha
  /// component against 0.5.  Values less than 0.5 are considered outside, and
  /// values greater than 0.5 are considered inside.
  /// 
  /// The texture returned is a TEMPORARY texture, and must be released once
  /// you are finished with it.
  /// </summary>
  public RenderTexture BuildDistanceField(RenderTexture sourceTex) {
    if (!tryInitMaterial()) {
      return null;
    }

    steps = Mathf.Clamp(steps, 0, 32);

    var tex0 = getTemp(sourceTex);
    var tex1 = getTemp(sourceTex);

    Graphics.Blit(sourceTex, tex0, _material, PASS_INIT);

    int step = Mathf.RoundToInt(Mathf.Pow(steps - 1, 2));
    while (step != 0) {
      _material.SetFloat("_Step", step);
      Graphics.Blit(tex0, tex1, _material, PASS_JUMP);

      var tmp = tex0;
      tex0 = tex1;
      tex1 = tmp;

      step /= 2;
    }

    RenderTexture.ReleaseTemporary(tex1);
    return tex0;
  }
}

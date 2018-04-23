using UnityEngine;

public static class GradientUtils {

  public static Texture2D ToTexture(this Gradient gradient, int resolution = 256, TextureFormat format = TextureFormat.ARGB32) {
    Texture2D tex = new Texture2D(resolution, 1, format, mipmap: false, linear: true);
    tex.filterMode = FilterMode.Bilinear;
    tex.wrapMode = TextureWrapMode.Clamp;
    tex.hideFlags = HideFlags.HideAndDontSave;

    for (int i = 0; i < resolution; i++) {
      float t = i / (resolution - 1.0f);
      tex.SetPixel(i, 0, gradient.Evaluate(t));
    }
    tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
    return tex;
  }
}

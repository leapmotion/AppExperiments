using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class TrueVolumetricRenderer : MonoBehaviour {

  [SerializeField]
  private Renderer[] _toDraw;

  [SerializeField]
  private Material _blendMaterial;

  private CommandBuffer _buffer;

  private void OnEnable() {
    if (_toDraw == null || _toDraw.Length == 0 || _toDraw.Any(t => t == null) ||
        _blendMaterial == null) {
      enabled = false;
      return;
    }

    _buffer = new CommandBuffer();
    _buffer.name = "TrueVolumetrics Render";

    int volumeInfoBufferId =
      Shader.PropertyToID("_OcclusionHandsTex");

    var camera = GetComponent<Camera>();
    int width = camera.pixelWidth;
    int height = camera.pixelHeight;

    RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width,
      height, RenderTextureFormat.RFloat, 0);
    descriptor.msaaSamples = 1;
    descriptor.bindMS = false;
    descriptor.sRGB = false;
    descriptor.useMipMap = false;
    
    _buffer.GetTemporaryRT(volumeInfoBufferId, descriptor);

    RenderTargetIdentifier volumeInfoRenderTargetId =
      new RenderTargetIdentifier(volumeInfoBufferId);

    // Clear any volume info in the buffer, and render from each renderer in
    // the renderers list to the current render target: A volume info buffer.
    //
    // Note that there's no guarantee that each renderer is actually writing
    // _volume_ information into this buffer! Currently, this is brittle, and
    // will only function correctly if every each renderer's attached material
    // is using a shader that outputs volume information instead of e.g.
    // color information.
    _buffer.SetRenderTarget(volumeInfoRenderTargetId);
    _buffer.ClearRenderTarget(clearDepth: false, clearColor: true,
      backgroundColor: new Color(0, 0, 0, 0));
    foreach (var renderer in _toDraw) {
      if (renderer == null || renderer.sharedMaterial == null) {
        continue;
      }
      _buffer.DrawRenderer(renderer, renderer.sharedMaterial);
    }
    
    // Now that we have a screen-sized buffer containing volume info for our
    // scene, blit from that buffer onto the screen using a shader that defines
    // how to blend from a pixel of volume data to a pixel on the screen.
    RenderTargetIdentifier currentlyRenderingTargetId =
      new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    _buffer.Blit(volumeInfoRenderTargetId, currentlyRenderingTargetId,
      _blendMaterial);

    camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, _buffer);
  }

  private void OnDisable() {
    if (_buffer != null) {
      GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, _buffer);
      _buffer.Dispose();
      _buffer = null;
    }
  }
}

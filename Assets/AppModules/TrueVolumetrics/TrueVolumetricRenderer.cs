using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class TrueVolumetricRenderer : MonoBehaviour {
  private const string TEX_PROPERTY = "_OcclusionHandsTexture";

  [SerializeField]
  private Renderer[] _toDraw;

  [SerializeField]
  private Material _blendMaterial;

  private CommandBuffer _buffer;

  private void OnEnable() {
    if (_toDraw == null || _toDraw.Length == 0 || _toDraw.Any(t => t == null) || _blendMaterial == null) {
      enabled = false;
      return;
    }

    _buffer = new CommandBuffer();

    int propertyId = Shader.PropertyToID(TEX_PROPERTY);

    var camera = GetComponent<Camera>();
    int width = camera.pixelWidth;
    int height = camera.pixelHeight;

    RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0);
    descriptor.msaaSamples = 1;
    descriptor.bindMS = false;
    descriptor.sRGB = false;
    descriptor.useMipMap = false;

    RenderTargetIdentifier identifier = new RenderTargetIdentifier(propertyId);
    RenderTargetIdentifier regular = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

    _buffer.GetTemporaryRT(propertyId, descriptor);
    _buffer.SetRenderTarget(identifier);
    _buffer.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: new Color(0, 0, 0, 0));

    foreach (var renderer in _toDraw) {
      _buffer.DrawRenderer(renderer, renderer.sharedMaterial);
    }

    _buffer.Blit(identifier, regular, _blendMaterial);

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

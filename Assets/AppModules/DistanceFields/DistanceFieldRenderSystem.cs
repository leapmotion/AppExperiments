using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Leap.Unity.DistanceFields.DistanceFieldsImpl;
using UnityEngine.SceneManagement;

namespace Leap.Unity.DistanceFields {

  public class DistanceFieldRenderSystem : MonoBehaviour {

    public ComputeShader rayTracingShader;

    private RenderTexture _target;
    private Camera _camera;

    public static DistanceFieldRenderSystem instance = null;

    private void Awake() {
      _camera = GetComponent<Camera>();

      instance = this;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
      setShaderParameters();

      render(destination);
    }

    private void setShaderParameters() {
      rayTracingShader.SetMatrix("_CameraToWorld",
        _camera.cameraToWorldMatrix);
      rayTracingShader.SetMatrix("_CameraInverseProjection",
        _camera.projectionMatrix.inverse);
    }

    private void render(RenderTexture destination) {
        initRenderTexture();

        // Set the target and dispatch the compute shader
        rayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        Graphics.Blit(_target, destination);
    }

    private void initRenderTexture() {
      if (_target == null || _target.width != Screen.width ||
        _target.height != Screen.height)
      {
        // Release render texture if we already have one.
        if (_target != null) {
          _target.Release();
        }

        // Get a render target for ray tracing.
        _target = new RenderTexture(Screen.width, Screen.height, 0,
            RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        _target.enableRandomWrite = true;
        _target.Create();
      }
    }

    // private CommandBuffer _commandBuffer;

    private List<DistanceFieldOp> _ops = new List<DistanceFieldOp>();
    
    private void OnEnable() {
      //var camera = GetComponent<Camera>();
      // if (camera != null) {
      //   _commandBuffer = createDistanceFieldCommandBuffer(camera);
      //   camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, _commandBuffer);
      // }
    }
    
    private void OnDisable() {
      // if (_commandBuffer != null) {
      //   GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.AfterForwardAlpha,
      //     _commandBuffer);
      //   _commandBuffer.Dispose();
      //   _commandBuffer = null;
      // }
    }

    private List<IDistanceField> _distanceFieldsBuffer =
      new List<IDistanceField>(128);
    private CommandBuffer createDistanceFieldCommandBuffer(Camera camera) {
      var buffer = new CommandBuffer();
      var width = camera.pixelWidth;
      var height = camera.pixelHeight;

      foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects()) {
        _distanceFieldsBuffer.Clear();
        rootObj.GetComponentsInChildren<IDistanceField>(_distanceFieldsBuffer);
        foreach (var distanceField in _distanceFieldsBuffer) {
          distanceField.GetOps(_ops);
        }
      }

      // foreach (var op in _ops) {
      //   switch op.type:
      //     case OpType.Sphere:
      //       buffer.
      // }

      return buffer;
    }

  }

}

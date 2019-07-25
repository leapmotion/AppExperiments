using UnityEngine;
using UnityEngine.Rendering;


public static class CommandBufferRendering 
{
  private static bool[] _command_flag = new bool[StaticCommandBuffers.COUNT];

  public static Matrix4x4 model_view_projection_matrix = Matrix4x4.identity;

  private static bool hack = true;
  public static void SetCommandBufferFlags() 
  {
    _command_flag[(int)StaticCommandBuffers.Enum.GeneratePoints]      = hack;
    _command_flag[(int)StaticCommandBuffers.Enum.RenderPoints]        = true;
    hack = false;
  }

  //todo (?) : implement material properties block
  public static void SetData(StaticCommandBuffers.Enum command_buffer) {
    switch (command_buffer) {
      case StaticCommandBuffers.Enum.GeneratePoints:
        StaticComputeShaders.shader[(int)StaticComputeShaders.Enum.GeneratePoints].SetBuffer(0, "_Point", StaticComputeBuffers.buffer[(int)StaticComputeBuffers.Enum.Point]);
        break;
      case StaticCommandBuffers.Enum.RenderPoints:
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetBuffer("_Point", StaticComputeBuffers.buffer[(int)StaticComputeBuffers.Enum.Point]);

        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetFloat("_PointSize", 1.0f);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetColor("_ColorTint", Color.white);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_DestBlend", (int)UnityEngine.Rendering.BlendMode.DstAlpha);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_ColorMask", (int)UnityEngine.Rendering.ColorWriteMask.All);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_ZTest", (int)UnityEngine.Rendering.OpaqueSortMode.Default);
        StaticMaterials.material[(int)StaticMaterials.Enum.Point].SetInt("_ZWrite", 1);
        break;
    }
  }

  public static void Render(Camera camera) {
    camera.RemoveAllCommandBuffers();

    SetCommandBufferFlags();

    model_view_projection_matrix = Camera.current.worldToCameraMatrix * Camera.current.transform.localToWorldMatrix;


    for (int i = 0; i < _command_flag.Length; i++) {
      if (_command_flag[i]) {
        SetData((StaticCommandBuffers.Enum)i);

        if (StaticCommandBuffers.configuration[i].asynchronous) {
          camera.AddCommandBufferAsync(StaticCommandBuffers.configuration[i].camera_event, StaticCommandBuffers.command[i], StaticCommandBuffers.configuration[i].queue_type);
        } else {
          camera.AddCommandBuffer(StaticCommandBuffers.configuration[i].camera_event, StaticCommandBuffers.command[i]);
        }
      }
    }
  }
}
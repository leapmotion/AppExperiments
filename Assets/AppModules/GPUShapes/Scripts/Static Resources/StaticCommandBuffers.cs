using UnityEngine;
using UnityEngine.Rendering;


public static class StaticCommandBuffers {
  public const int COUNT = 2;

  public enum Enum {
    GeneratePoints,
    RenderPoints
  }


  public struct CommandBufferConfiguration {
    public Enum command;
    public CameraEvent camera_event;
    public ComputeQueueType queue_type;
    public bool asynchronous;

    public CommandBufferConfiguration(Enum c, CameraEvent e, ComputeQueueType q, bool a) {
      command = c;
      camera_event = e;
      queue_type = q;
      asynchronous = a;
    }
  }


  public static CommandBufferConfiguration[] configuration = new CommandBufferConfiguration[COUNT]
  {
    new CommandBufferConfiguration(Enum.GeneratePoints,                 CameraEvent.BeforeForwardOpaque, ComputeQueueType.Default, false),
    new CommandBufferConfiguration(Enum.RenderPoints,                   CameraEvent.BeforeForwardOpaque, ComputeQueueType.Default, false)
  };


  public static CommandBuffer[] command { get { return _command; } }
  private static CommandBuffer[] _command = _command ?? new CommandBuffer[COUNT]
  {
    Instance((Enum)0),
    Instance((Enum)1)
  };


  static CommandBuffer Instance(Enum command) {
    CommandBuffer command_buffer = new CommandBuffer();

    switch (command) {
      case Enum.GeneratePoints:
        command_buffer.name = "Generate Points";
        int groupdim = (int)(Mathf.Pow((float)StaticComputeBuffers.THREADS, 1.0f / 3.0f) / ((float)StaticComputeBuffers.GROUPSIZE));
        // command_buffer.DispatchCompute(StaticComputeShaders.shader[(int)StaticComputeShaders.Enum.GeneratePoints], 0, StaticComputeBuffers.GROUPSIZE, StaticComputeBuffers.GROUPSIZE, StaticComputeBuffers.GROUPSIZE);	       
        command_buffer.SetComputeBufferParam(StaticComputeShaders.shader[(int)StaticComputeShaders.Enum.GeneratePoints], 0, "_Point", StaticComputeBuffers.buffer[(int)StaticComputeBuffers.Enum.Point]);
        command_buffer.DispatchCompute(StaticComputeShaders.shader[(int)StaticComputeShaders.Enum.GeneratePoints], 0, groupdim, groupdim, groupdim);
        break;
      case Enum.RenderPoints:
        command_buffer.name = "Render Points";
        command_buffer.SetGlobalBuffer("_Point", StaticComputeBuffers.buffer[(int)StaticComputeBuffers.Enum.Point]);
        command_buffer.DrawProcedural(CommandBufferRendering.model_view_projection_matrix, StaticMaterials.material[(int)StaticMaterials.Enum.Point], 0, MeshTopology.Points, StaticComputeBuffers.THREADS, 0);
        break;
      default:
        break;
    }

    return command_buffer;
  }


  public static bool Initialized() {
    bool initialized = true;
    for (int i = 0; i < _command.Length; i++) {
      if (_command[i] == null) {
        Debug.Log("Command Buffer " + ((Enum)i).ToString() + " not initialized.");
        initialized = false;
      } else {
        Debug.Log("Command Buffer " + ((Enum)i).ToString() + " initialized.");
      }
    }

    return initialized;
  }
}




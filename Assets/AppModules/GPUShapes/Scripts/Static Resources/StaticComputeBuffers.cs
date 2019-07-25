using UnityEngine;


public static class StaticComputeBuffers {
  public const int COUNT      = 1;
  public const int GROUPSIZE  = 4;
  public const int THREADS    = 65536;


  public enum Enum {
    Point
  }


  //type of compute memory, size in bytes, element count, associated name used in compute shader
  public struct Configuration {
    public ComputeBufferType type;
    public int bytes;
    public int count;
    public string name;

    public Configuration(ComputeBufferType t, int b, int c, string n) {
      type = t;
      bytes = b;
      count = c;
      name = n;
    }
  }


  //buffer configurations keep buffer names and aligned with buffer indices 
  public static Configuration[] configuration = new Configuration[COUNT]
  {
      new Configuration(ComputeBufferType.Default,         32,                THREADS, "_"+((Enum)0).ToString()), //Points
  };


  //buffer allocation - return the existing buffer if it isnt null, else create a new one from the config
  private static ComputeBuffer Instance(Enum compute_buffer_enum) {
    int index             = (int)compute_buffer_enum;
    ComputeBuffer buffer = new ComputeBuffer(configuration[index].count, configuration[index].bytes, configuration[index].type);

   /*
    Vector4[] data    = new Vector4[configuration[index].count * 2];
    for (uint i = 0; i < data.Length; i++)
		{
			data[i]	= Random.Range(-4.0f, 4.0f) * Vector4.one;
		}
		buffer.SetData(data);
    */

    return buffer;
  }


  //buffer array and public accessor
  public static ComputeBuffer[] buffer { get { return _buffer; } }
  private static ComputeBuffer[] _buffer = _buffer ?? new ComputeBuffer[COUNT]
  {
      Instance((Enum)0)
  };


  //verification that buffers exist - just in case
  public static bool Initialized() {
    bool initialized = true;

    for (int i = 0; i < _buffer.Length; i++) {
      if (_buffer[i] == null) {
        Debug.Log("Compute Buffer " + configuration[i].name + " not initialized.");
        initialized = false;
      }
    }

    return initialized;
  }

  //binds buffers to compute shaders
  public static void Bind(StaticComputeShaders.Enum shader, Enum buffer, int kernel) {
    StaticComputeShaders.shader[(int)shader].SetBuffer(kernel, StaticComputeBuffers.configuration[(int)buffer].name, StaticComputeBuffers.buffer[(int)buffer]);
  }


  //binds buffers to compute shaders
  public static void Bind(StaticComputeShaders.Enum shader, Enum buffer, string name, int kernel) {
    StaticComputeShaders.shader[(int)shader].SetBuffer(kernel, name, _buffer[(int)buffer]);
  }


  //release and disposal of buffers
  public static void Dispose() {
    for (int i = 0; i < _buffer.Length; i++) {
      _buffer[i].Dispose();
      _buffer[i].Release();
    }

    Debug.Log("Compute Buffers Released");
  }
}



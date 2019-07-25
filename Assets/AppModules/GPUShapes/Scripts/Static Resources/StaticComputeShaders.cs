using UnityEngine;

public static class StaticComputeShaders {
  public const int COUNT = 1;
  private const string _PATH = "GPU/";
  private const string _EXTENSION = ".compute";

  public enum Enum {
    GeneratePoints
  };


  public struct ComputeShaderConfiguration {
    public string path;
    public int kernels;

    public ComputeShaderConfiguration(string p, int k) {
      path = p;
      kernels = k;
    }
  }


  public static ComputeShaderConfiguration[] configuration = new ComputeShaderConfiguration[COUNT]
  {
    new ComputeShaderConfiguration(_PATH + "GeneratePoints", 1)
  };


  public static ComputeShader[] shader { get { return _shader; } }
  private static ComputeShader[] _shader = _shader ?? new ComputeShader[COUNT]
  {
    Instance(0)
  };


  //buffer allocation - return the existing buffer if it isnt null, else create a new one from the config
  private static ComputeShader Instance(int index) {
    return (ComputeShader)Resources.Load(configuration[index].path);
  }


  //verification that buffers exist - just in case
  public static bool Initialized() {
    bool initialized = true;
    for (int i = 0; i < _shader.Length; i++) {
      if (_shader[i] == null) {
        Debug.Log("Compute Shader at " + configuration[i].path + " not initialized.");
        initialized = false;
      }
    }

    return initialized;
  }
}
using UnityEngine;

public static class StaticMaterials {
  public const int COUNT = 1;

  public enum Enum {
    Point
  };


  public struct MaterialConfiguration {
    public string name;
    public string shader_name;
    public Enum material_enum;

    public MaterialConfiguration(string n, string s, Enum m) {
      name = n;
      shader_name = s;
      material_enum = m;
    }
  }


  public static MaterialConfiguration[] configuration = new MaterialConfiguration[COUNT]
  {
    new MaterialConfiguration("Point", "Point", Enum.Point)
  };


  public static Material[] material { get { return _material; } }
  private static Material[] _material = _material ?? new Material[COUNT]
  {
    Instance((Enum)0)
  };


  private static Material Instance(Enum material_enum) {
    int index                 = (int)material_enum;
    Material material         = new Material(Shader.Find(configuration[index].shader_name));
    material.name             = configuration[index].name;
    material.hideFlags        = HideFlags.HideAndDontSave;
    material.doubleSidedGI    = false;
    material.enableInstancing = true;

    return material;
  }


  public static bool Initialized() {
    bool initialized = true;

    for (int i = 0; i < _material.Length; i++) {
      if (_material[i] == null) {
        Debug.Log("Material " + configuration[i].name + " not initialized.");
        initialized = false;
      }
    }

    return initialized;
  }


  public static void Destroy() {
    for (int i = 0; i < _material.Length; i++) {
      Material.DestroyImmediate(_material[i]);
    }

    Debug.Log("Materials Destroyed");
  }
}
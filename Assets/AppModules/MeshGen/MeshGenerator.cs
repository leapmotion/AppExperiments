using UnityEngine;

namespace Leap.Unity.MeshGen {
  
  public abstract class MeshGenerator : ScriptableObject {

    /// <summary>
    /// Fills the provided mesh with data based on the generator's current configuration.
    /// </summary>
    public abstract void Generate(Mesh mesh);

  }

}
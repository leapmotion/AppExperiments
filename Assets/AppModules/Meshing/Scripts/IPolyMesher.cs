using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public interface IPolyMesher<T> {

    /// <summary>
    /// Given the input object, fill the positions, polygons, and smooth edges
    /// lists that are sufficient to construct a PolyMesh mesh representation of
    /// the input object.
    /// 
    /// The output colors list can be left empty or null, to represent a lack of any
    /// specific mesh colors. However, if any colors are added, the number of colors must
    /// match the number of positions (these represent vertex colors).
    /// </summary>
    void FillPolyMeshData(T inputObject,
                          List<Vector3> outPositions,
                          List<Polygon> outPolygons,
                          List<Edge> outSmoothEdges,
                          List<Color> outColors);

  }

}

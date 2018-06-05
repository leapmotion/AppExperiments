using System;
using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Edges are automatically constructed and managed by PolyMesh operations. They
  /// represent a connection between two position indices (vertices) of its mesh;
  /// however, they do not maintain a reference to any specific Polygon on their own.
  /// 
  /// Edges, by default, are always directed from the lower indexed position to the
  /// higher indexed position.
  /// 
  /// Polygons are not defined by Edges, so Edge directionality is irrelevant to them;
  /// their 'edges' are implcitily defined by their position index arrays. (However, they
  /// do provide Edge enumerators for convenience; these Edges are constructed on the fly,
  /// and DO have a consistent winding order from A to B around the polygon. Keep in mind
  /// that two Edges are considered equal even if their As and Bs are swapped.
  /// 
  /// The mesh itself stores and manages the data for face-adjacency with any given Edge,
  /// and edge-adjacency for any given face (polygon), and an Edge struct will remain
  /// valid over polygon-modification operations to the mesh as long as those operations
  /// themselves don't destroy all of the polygons that connect a given pair of mesh
  /// positions.
  /// 
  /// (The SplitEdgeAddVertex operation is an important example of an Edge-invalidating
  /// operation. A PokePolygon operation, by contrast, will invalidate a Polygon but
  /// never an Edge.)
  /// </summary>
  public struct Edge : IEquatable<Edge> {
    public PolyMesh mesh;
    
    private int _a, _b;
    public int a { get { return _a; } }
    public int b { get { return _b; } }
    
    public Edge(int a, int b) : this(null, a, b, false) { }
    public Edge(PolyMesh mesh, int a, int b, bool literalOrder = false) {
      this.mesh = mesh;

      if (!literalOrder) {
        if (a > b) {
          Utils.Swap(ref a, ref b);
        }
      }

      _a = a;
      _b = b;
    }

    public int this[int idx] {
      get {
        if (idx != 0 && idx != 1) {
          throw new System.IndexOutOfRangeException("Invalid edge index.");
        }
        if (idx == 0) return a;
        return b; }
    }

    #region Equality

    public override int GetHashCode() {
      int hashA, hashB;
      if (a > b) { hashA = a; hashB = b; }
      else       { hashA = b; hashB = a; }
      return new Hash() { mesh, hashA, hashB };
    }

    public bool Equals(Edge other) {
      return this.mesh == other.mesh
          && ((a == other.a && b == other.b)
              || (a == other.b && b == other.a));
    }
    public override bool Equals(object obj) {
      if (obj is Edge) {
        return Equals((Edge)obj);
      }
      return base.Equals(obj);
    }

    public static bool operator ==(Edge thisEdge, Edge otherEdge) {
      return thisEdge.Equals(otherEdge);
    }

    public static bool operator !=(Edge thisEdge, Edge otherEdge) {
      return !(thisEdge.Equals(otherEdge));
    }

    #endregion

    private Vector3 P(int vertIdx) { return mesh.GetPosition(vertIdx); }

    public Vector3 GetPositionAlongEdge(float amountAlongEdge,
                                        EdgeDistanceMode mode = EdgeDistanceMode.Normalized) {
      var pA = P(a);
      var pB = P(b);
      var lineVec = (pB - pA);
      var mag = lineVec.magnitude;
      var dir = lineVec / mag;

      switch (mode) {
        case EdgeDistanceMode.Normalized:
          return pA + dir * amountAlongEdge * mag;
        case EdgeDistanceMode.Absolute:
        default:
          return pA + dir * amountAlongEdge;
      }
    }

    public Vector3 GetPositionA() {
      return mesh.GetPosition(a);
    }

    public Vector3 GetPositionB() {
      return mesh.GetPosition(b);
    }

    /// <summary>
    /// Given a normalized amount along the edge (from 0 (a) to 1 (b)), returns the
    /// world distance of that edge point to the edge endpoint to which it is closest.
    /// </summary>
    public float GetWorldDistanceFromEdgeEndpoint(float normalizedAmountAlongEdge) {
      var t = normalizedAmountAlongEdge;
      var mag = (P(b) - P(a)).magnitude;
      if (t > 0.5f) {
        // Distance from edge point b.
        return mag - (t * mag);
      }
      else {
        // Distance from edge point a.
        return (t * mag);
      }
    }

    /// <summary>
    /// Returns whether this Edge has the argument vertex index at either A or B.
    /// </summary>
    public bool ContainsVertex(int vertIndex) {
      return a == vertIndex || b == vertIndex;
    }

    public float GetSqrLength() {
      return (P(b) - P(a)).sqrMagnitude;
    }

    /// <summary>
    /// Assumes the provided position is on the line defined by this Edge and returns
    /// the amount along the edge in normalized space (0 is A, 1 is B). Doesn't clamp
    /// the result between 0 and 1.
    /// 
    /// If the world position provided is not on this edge, this function doesn't care,
    /// but don't expect a useable result.
    /// </summary>
    public float GetNormalizedAmountAlongEdge(Vector3 worldPosition) {
      var pA = P(a);
      var pB = P(b);
      var edgeMag = (pB - pA).magnitude;

      var pMag = (worldPosition - pA).magnitude;

      if (pMag < PolyMath.POSITION_TOLERANCE) {
        return 0f;
      }
      else if (edgeMag - pMag < PolyMath.POSITION_TOLERANCE) {
        return 1f;
      }

      return pMag / edgeMag;
    }

    public override string ToString() {
      return "[Edge | a: " + a + ", b: " + b + ", mesh: " + mesh + "]";
    }

    #region Debug Rendering

    public static void Render(Edge edge, float sizeMult = 1f) {
      Render(edge, LeapColor.mint, sizeMult);
    }

    public static void Render(Edge edge, Color color, float sizeMult = 1f) {
      RenderLiteral(edge.GetPositionA(), edge.GetPositionB(), color, sizeMult);
    }

    public static void RenderLiteral(Vector3 pA, Vector3 pB, Color color, float sizeMult = 1f) {
      RuntimeGizmos.RuntimeGizmoDrawer drawer;
      if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
        drawer.color = color;
        
        drawer.DrawWireCapsule(pA, pB, 0.04f * sizeMult);
      }
    }

    #endregion

  }

  public enum EdgeDistanceMode { Normalized, Absolute }

  public struct EdgeSequence {

    // TODO: This is supposed to help in dealing with the bunch of edges that are
    // returns as the edges of a cutting operation.

    //public PolyMesh polyMesh;
    //public int[] verts;

    //public bool isSingleEdge { get { return verts.Length == 2; } }
    //public Edge ToSingleEdge() {
    //  return new Edge() {
    //    mesh = polyMesh,
    //    a = verts[0],
    //    b = verts[1]
    //  };
    //}

    ///// <summary>
    ///// Merges a soup of edges into a soup of edge sequences. Any edge that connects to
    ///// another edge will wind up in the same EdgeSequence; any edge that has no
    ///// connected edges will be placed into a degenerate single-edge EdgeSequence.
    ///// </summary>
    //public static void Merge(List<Edge> edges, List<EdgeSequence> intoSequenceList) {
    //  throw new System.NotImplementedException();
    //}

  }

  //public struct EdgeLoop {
  //  public PolyMesh polyMesh;
  //  public int[] verts;

  //  private Vector3 P(int loopIdx) {
  //    return polyMesh.GetPosition(verts[loopIdx]);
  //  }

  //  public InternalPolyEnumerator insidePolys;
  //  public InternalPolyEnumerator outsidePolys;

  //  public struct InternalPolyEnumerator {
  //    PolyMesh polyMesh;

  //    public InternalPolyEnumerator(bool rightHanded) {
  //      throw new System.NotImplementedException();
  //    }

  //    /// <summary>
  //    /// Clears and fills the positions and polygons of the provided PolyMesh
  //    /// with the polygons defined by this polygon enumerator.
  //    /// </summary>
  //    public void Fill(PolyMesh intoMesh) {
  //      throw new System.NotImplementedException();
  //    }
  //  }
  //}

}
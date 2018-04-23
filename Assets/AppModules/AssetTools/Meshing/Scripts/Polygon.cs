using Leap.Unity.Query;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Struct representing an N-gon face. Vertices are stored as indices, not positions.
  /// To get positions, the Polygon must exist in the context of a PolyMesh.
  /// 
  /// The positions indexed by the face must be planar and convex.
  /// </summary>
  public struct Polygon : System.IEquatable<Polygon> {

    public PolyMesh mesh;

    private List<int> _verts;

    #region Vertices

    public List<int> verts {
      get { return _verts; }
      set {
        _verts = value;
      }
    }
    
    /// <summary>
    /// Indexing a polygon directly has _slightly_ more overhead than indexing its verts
    /// directly, but indexes its vertices cyclically.
    /// </summary>
    public int this[int idx] {
      get {
        while (idx < 0) idx += verts.Count;
        return verts[idx % verts.Count];
      }
      set {
        while (idx < 0) idx += verts.Count;
        verts[idx % verts.Count] = value;
      }
    }

    public int Count { get { return _verts.Count; } }

    #endregion

    #region Standard Construction (Pooled Verts)

    /// <summary>
    /// Creates a new (empty) Polygon with Pooled vertex indices. Add valid indices to
    /// positions in a PolyMesh, then add the Polygon to that PolyMesh via AddPolygon.
    /// </summary>
    public static Polygon SpawnEmpty() {
      Polygon polygon = new Polygon();
      polygon.verts = Pool<List<int>>.Spawn();
      polygon.verts.Clear();
      return polygon;
    }

    /// <summary>
    /// Creates a new quadrilateral Polygon with Pooled vertex indices. Use indices that
    /// index into the positions of a PolyMesh, then add the Polygon to that PolyMesh via
    /// AddPolygon.
    /// 
    /// You can optionally pass an indexOffset to be added to each argument index.
    /// 
    /// Polygons are always assumed to have coplanar vertices.
    /// </summary>
    public static Polygon SpawnQuad(int a, int b, int c, int d,
                                    int indexOffset = 0) {
      Polygon polygon = new Polygon();
      polygon.verts = Pool<List<int>>.Spawn();
      polygon.verts.Clear();
      polygon.verts.Add(a + indexOffset);
      polygon.verts.Add(b + indexOffset);
      polygon.verts.Add(c + indexOffset);
      polygon.verts.Add(d + indexOffset);
      return polygon;
    }

    #endregion

    #region Operations

    /// <summary>
    /// Clears the List-int _verts tracked by this Polygon and returns it to the List-int
    /// Pool.
    /// 
    /// USE WITH CAUTION. After calling this method, this Polygon is no longer useable.
    /// </summary>
    public void RecycleVerts() {
      _verts.Clear();
      Pool<List<int>>.Recycle(_verts);
    }

    /// <summary>
    /// Copies this Polygon, returning a Polygon object with a new underlying vert list.
    /// </summary>
    public Polygon Copy() {
      var list = new List<int>();
      list.AddRange(_verts);
      return new Polygon() { mesh = this.mesh, _verts = list };
    }

    private Vector3 P(int vertIndex) {
      return mesh.GetPosition(vertIndex);
    }

    /// <summary>
    /// Returns the position of the vertex at the argument vertIndex in the _mesh_ of
    /// this polygon. This method is a shortcut for polygon.mesh.GetPosition(vertIndex);
    /// the provided index is _not_ the index of a vertex in this polygon, but the index
    /// of the vertex in the mesh!
    /// e.g. Don't do polygon.GetPosition(0), do polygon.GetPosition(polygon[0]).
    /// </summary>
    public Vector3 GetMeshPosition(int meshVertIndex) {
      return P(meshVertIndex);
    }

    /// <summary>
    /// Adds the argument amount to each vertex index in this polygon definition, and
    /// also returns this polygon for convenience. This function modifies this Polygon.
    /// 
    /// Warning: This is not guaranteed to result in a valid mesh polygon.
    /// </summary>
    public Polygon IncrementIndices(int byAmount) {
      for (int i = 0; i < _verts.Count; i++) {
        _verts[i] += byAmount;
      }

      return this;
    }

    /// <summary>
    /// Inserts a new vertex index into this Polygon between the indices specified by the
    /// argument Edge. The edge must be a valid edge of this Polygon. This function
    /// modifies this polygon and returns the polygon for convenience.
    /// 
    /// Warning: This is not guaranteed to result in a valid mesh polygon.
    /// </summary>
    public Polygon InsertEdgeVertex(Edge edge, int newVertIndex) {

      // Handle edge-case where the edge is between the first and last indices.
      if ((_verts[0] == edge.b && _verts[_verts.Count - 1] == edge.a)
          || (_verts[0] == edge.a && _verts[_verts.Count - 1] == edge.b)) {

        _verts.Add(newVertIndex);

        return this;
      }
      
      for (int i = 0; i < _verts.Count; i++) {
        if (_verts[i] == edge.a || _verts[i] == edge.b) {
          _verts.Insert(i + 1, newVertIndex);

          return this;
        }
      }

      return this;
    }

    /// <summary>
    /// Given a PolyMesh to resolve this polygon's vertex indices to positions, returns
    /// the normal vector of this polygon definition.
    /// 
    /// Obviously if the polygon vertices are non-planar, this won't work! But that's an
    /// assumption we make about all Polygons.
    /// </summary>
    public Vector3 GetNormal() {
      Vector3 normal = Vector3.zero;
      Vector3 a, b, c;
      for (int i = 0; i < _verts.Count; i++) {
        a = P(this[i]);
        b = P(this[i + 1]);
        c = P(this[i + 2]);
        normal = Vector3.Cross(b - a, c - a);
        if (normal != Vector3.zero) {
          return normal.normalized;
        }
      }
      return normal;
    }

    public Vector3 GetCentroid() {
      var sum = Vector3.zero;
      foreach (var vert in verts) {
        sum += P(vert);
      }
      return sum / verts.Count;
    }

    /// <summary>
    /// Calculates and returns whether this polygon is truly convex. The polygon's verts
    /// are assumed to be planar. (Not sure if your polygon is planar? Call IsPlanar())
    /// 
    /// Polygons that are added to meshes are always assumed to be convex; adding a
    /// non-convex polygon to a mesh via AddPolygon is an error. However, this method
    /// is useful when constructing new polygons manually.
    /// </summary>
    public bool CheckConvex() {
      if (_verts.Count < 3) {
        throw new System.InvalidOperationException("Polygons must have 3 or more vertices.");
      }

      if (_verts.Count == 3) {
        // Points cannot be colinear.
        var a = P(this[0]);
        var b = P(this[1]);
        var c = P(this[2]);
        return Vector3.Cross(b - a, c - a) != Vector3.zero;
      }

      // Compare the cross products of (i -> i + 1) and (i -> i + 2) around the polygon;
      // if the cross products' ever flip direction with respect to one another, the
      // polygon must be non-convex. (Straight lines are OK!)
      Maybe<Vector3> lastNonZeroCrossProduct = Maybe.None;
      for (int i = 0; i < Count; i++) {
        var a = P(this[i + 0]);
        var b = P(this[i + 1]);
        var c = P(this[i + 2]);

        var ab = b - a;
        var ac = c - a;

        var abXac = Vector3.Cross(ab, ac);

        if (!lastNonZeroCrossProduct.hasValue) {
          lastNonZeroCrossProduct = abXac;
          if (lastNonZeroCrossProduct == Vector3.zero) {
            lastNonZeroCrossProduct = Maybe.None;
          }
        }
        else {
          if (abXac == Vector3.zero) continue;
          else {
            if (Vector3.Dot(abXac, lastNonZeroCrossProduct.valueOrDefault) < 0f) {
              return false;
            }
          }
        }
      }

      if (!lastNonZeroCrossProduct.hasValue) {
        // No non-zero cross product implies that all of the points in the poylgon are
        // colinear. This is not a valid polygon, so we'll return false for convexity.
        return false;
      }

      return true;
    }

    /// <summary>
    /// Calculates and returns whether this polygon is truly planar.
    /// 
    /// Polygons are always assumed to be planar; this method is useful for a
    /// develepor for debugging purposes when implementing new polygon or mesh operations.
    /// </summary>
    public bool CheckPlanar() {
      if (verts.Count < 2) {
        throw new System.InvalidOperationException(
          "Polygon only has one or fewer vertex indices.");
      }

      if (verts.Count == 2) return true;

      if (verts.Count == 3) return true;

      Maybe<Vector3> lastCrossProduct = Maybe.None;
      for (int i = 0; i < verts.Count - 3; i++) {
        var a = P(this[i]);
        var b = P(this[i + 1]);
        var c = P(this[i + 2]);
        var ab = b - a;
        var ac = c - a;

        var curCrossProduct = Vector3.Cross(ab, ac);
        if (lastCrossProduct.hasValue) {
          // We expect every cross product of ab and ac (for a around the polygon)
          // to point in the same direction; by crossing them together, any deviation
          // in direction will produce a non-zero 'cross-cross-product.'
          var productWithLast = Vector3.Cross(curCrossProduct, lastCrossProduct.valueOrDefault);
          if (productWithLast.x > PolyMath.POSITION_TOLERANCE
              || productWithLast.y > PolyMath.POSITION_TOLERANCE
              || productWithLast.z > PolyMath.POSITION_TOLERANCE) {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Returns whether the given polygon vertex array indices (NOT mesh position
    /// indices) are an edge of this polygon.
    /// </summary>
    public bool HasPolyIdxEdge(int polyIdx0, int polyIdx1) {
      return ((polyIdx0 + 1) % _verts.Count) == polyIdx1
          || ((polyIdx1 + 1) % _verts.Count) == polyIdx0;
    }

    /// <summary>
    /// Returns whether this polygon contains an edge between the two mesh vertex indices.
    /// </summary>
    public bool HasVertIdxEdge(int vertIdx0, int vertIdx1) {
      int indexOf0 = _verts.IndexOf(vertIdx0);
      int indexOf1 = _verts.IndexOf(vertIdx1);
      if (indexOf0 != -1 && indexOf1 != -1) {
        return HasPolyIdxEdge(indexOf0, indexOf1);
      }
      return false;
    }

    /// <summary>
    /// Returns whether the argument verts are on a colinear sequence of edges in this
    /// polygon, and if so, adds all of the colinear edges in that colinear sequence to
    /// the outColinearEdges list argument.
    /// </summary>
    public bool AreVertsOnColinearSequence(int vertIdx0, int vertIdx1,
                                           List<Edge> outColinearEdges,
                                           bool includeWholeColinearSequence) {
      var testEdgeSequence = Pool<List<Edge>>.Spawn();
      testEdgeSequence.Clear();
      try {
        // Start at vertIdx0. Start going around until we stop getting colinear edges.
        Maybe<Vector3> edgeDir = Maybe.None;
        int startIdx = _verts.IndexOf(vertIdx0);
        if (startIdx == -1) throw new System.Exception("Vert index not in this polygon.");
        bool hitIdx1 = false;
        for (int i = startIdx; i < startIdx + _verts.Count; i++) {
          var a = P(this[i + 0]);
          var b = P(this[i + 1]);
          hitIdx1 |= this[i + 1] == vertIdx1;
          Vector3 testDir = b - a;
          var thisEdge = new Edge(mesh, this[i], this[i+1]);
          if (edgeDir.hasValue) {
            if (Vector3.Cross(testDir, edgeDir.valueOrDefault) == Vector3.zero) {
              // Colinear, add this edge tentatively and keep going.
              testEdgeSequence.Add(thisEdge);
            }
            else {
              // Not colinear, stop.
              break;
            }
          }
          else {
            // Establish edge direction and add this edge.
            testEdgeSequence.Add(thisEdge);
            edgeDir = testDir;
          }
        }
        // After the first loop, if we hit index 1, we're done.
        if (hitIdx1) {
          if (testEdgeSequence.Count == 1) {
            // We didn't find a sequence of colinear edges.
            return false;
          }
          else {
            // We found a sequence of colinear edges!
            if (outColinearEdges != null) {
              if (includeWholeColinearSequence) {
                outColinearEdges.AddRange(testEdgeSequence);
              }
              else {
                // Just add edges up to the vertIdx1 edge
                foreach (var edge in testEdgeSequence) {
                  outColinearEdges.Add(edge);
                  if (edge.a == vertIdx1 || edge.b == vertIdx1) break;
                }
              }
            }
            return true;
          }
        }
        else {
          // Try the same process, but in the other direction around the polygon.
          hitIdx1 = false;
          for (int i = startIdx + _verts.Count;
                   i > startIdx; i--) {
            var a = P(this[i - 0]);
            var b = P(this[i - 1]);
            hitIdx1 |= this[i - 1] == vertIdx1;
            Vector3 testDir = b - a;
            var thisEdge = new Edge(this.mesh, this[i = 0], this[i - 1]);
            if (edgeDir.hasValue) {
              if (Vector3.Cross(testDir, edgeDir.valueOrDefault) == Vector3.zero) {
                // Colinear, add this edge tentatively and keep going.
                testEdgeSequence.Add(thisEdge);
              }
              else {
                // Not colinear, stop.
                break;
              }
            }
            else {
              // Establish edge direction and add this edge.
              testEdgeSequence.Add(thisEdge);
              edgeDir = testDir;
            }
          }
          // After the second loop, we're definitely done.
          if (hitIdx1) {
            if (testEdgeSequence.Count == 1) {
              // We didn't find a sequence of colinear edges.
              return false;
            }
            else {
              // We found a sequence of colinear edges!
              if (outColinearEdges != null) {
                if (includeWholeColinearSequence) {
                  outColinearEdges.AddRange(testEdgeSequence);
                }
                else {
                  // Just add edges up to the vertIdx1 edge
                  foreach (var edge in testEdgeSequence) {
                    outColinearEdges.Add(edge);
                    if (edge.a == vertIdx1 || edge.b == vertIdx1) break;
                  }
                }
              }
              return true;
            }
          }
          else {
            // No luck, we never found the next index along a colinear edge.
            return false;
          }
        }
      }
      finally {
        testEdgeSequence.Clear();
        Pool<List<Edge>>.Recycle(testEdgeSequence);
      }
    }

    #endregion

    #region Triangulation

    public struct PolyTriangle {
      public int a, b, c;
      public Edge? polyEdge0, polyEdge1, polyEdge2;
    }

    public PolyTriangleEnumerator polyTris {
      get { return new PolyTriangleEnumerator(this); }
    }

    public struct PolyTriangleEnumerator {
      private Polygon _polygon;
      private int _curIdx;

      public PolyTriangleEnumerator(Polygon polygon) {
        _polygon = polygon;
        _curIdx = -1;
      }

      public PolyTriangle Current {
        get {
          int a = _polygon[0];
          int b = _polygon[_curIdx + 1];
          int c = _polygon[_curIdx + 2];
          return new PolyTriangle() {
            a = a,
            b = b,
            c = c,
            polyEdge0 = (_polygon.HasPolyIdxEdge(a, b)) ? new Edge(a, b) : (Edge?)null,
            polyEdge1 = (_polygon.HasPolyIdxEdge(b, c)) ? new Edge(b, c) : (Edge?)null,
            polyEdge2 = (_polygon.HasPolyIdxEdge(c, a)) ? new Edge(c, a) : (Edge?)null,
          };
        }
      }

      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx + 2 < _polygon.Count;
      }

      public PolyTriangleEnumerator GetEnumerator() { return this; }
    }

    public TriangleEnumerator tris {
      get { return new TriangleEnumerator(this); }
    }

    public struct TriangleEnumerator {

      private Polygon _polygon;
      private int _curIdx;

      public TriangleEnumerator(Polygon polygon) {
        _polygon = polygon;

        _curIdx = -1;
      }

      public Triangle Current {
        get {
          return new Triangle() {
            a = _polygon[0],
            b = _polygon[_curIdx + 1],
            c = _polygon[_curIdx + 2]
          };
        }
      }

      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx + 2 < _polygon.Count;
      }

      public TriangleEnumerator GetEnumerator() { return this; }

    }

    #endregion

    #region Edge Traversal

    public EdgeEnumerator edges { get { return new EdgeEnumerator(this); } }

    public struct EdgeEnumerator {
      Polygon _poly;
      int _curIdx;
      public EdgeEnumerator(Polygon polygon) {
        _poly = polygon;
        _curIdx = -1;
      }
      public Edge Current {
        get { return new Edge(_poly.mesh,
                              _poly[_curIdx],
                              _poly[_curIdx + 1],
                              literalOrder: true);
        }
      }
      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx < _poly.Count;
      }
      public EdgeEnumerator GetEnumerator() { return this; }
    }

    #endregion

    #region Equality

    public override int GetHashCode() {
      var hash = new Hash();
      hash.Add(mesh);
      foreach (var id in verts) { hash.Add(id); }
      return hash;
    }

    public bool Equals(Polygon otherPoly) {
      if (this.mesh != otherPoly.mesh) return false;
      if (_verts != null && otherPoly._verts == null) return false;
      if (otherPoly._verts != null && _verts == null) return false;
      if (otherPoly._verts == null && _verts == null) return true;

      if (this._verts.Count != otherPoly._verts.Count) return false;

      // Utils.AreEqualUnordered(verts, otherPoly.verts); perhaps?
      // (would also need to sort before hashing)
      for (int i = 0; i < _verts.Count; i++) {
        if (_verts[i] != otherPoly._verts[i]) return false;

        // TODO DELETEME
        if (i >= 60) {
          throw new System.InvalidOperationException(
            "This polygon has WAY too many verts!");
        }
      }
      return true;
    }

    public override bool Equals(object obj) {
      if (obj is Polygon) {
        return Equals((Polygon)obj);
      }
      return base.Equals(obj);
    }

    public static bool operator ==(Polygon thisPoly, Polygon otherPoly) {
      return thisPoly.Equals(otherPoly);
    }
    public static bool operator !=(Polygon thisPoly, Polygon otherPoly) {
      return !(thisPoly.Equals(otherPoly));
    }

    #endregion

    #region Static PolyMesh Generator
    
    public static PolyMesh CreatePolyMesh(int numVerts) {
      var mesh = new PolyMesh();
      FillPolyMesh(numVerts, mesh);
      return mesh;
    }

    public static void FillPolyMesh(int numVerts, PolyMesh mesh) {
      numVerts = Mathf.Max(numVerts, 3);

      var positions = Pool<List<Vector3>>.Spawn();
      positions.Clear();
      try {
        Quaternion rot = Quaternion.AngleAxis(360f / numVerts, -Vector3.forward);
        Vector3 radial = Vector3.right;
        positions.Add(radial);
        for (int i = 1; i < numVerts; i++) {
          radial = rot * radial;
          positions.Add(radial);
        }
        
        var indices = Values.Range(0, numVerts);
        var polygon = new Polygon() { mesh = mesh, verts = indices.ToList() };
        
        mesh.Fill(positions, polygon);
      }
      finally {
        positions.Clear();
        Pool<List<Vector3>>.Recycle(positions);
      }
    }

    #endregion

    #region Debug Rendering
    
    public static void Render(Polygon p, Color color) {
      RuntimeGizmos.RuntimeGizmoDrawer drawer;
      if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
        drawer.color = color;

        var normal = p.GetNormal();

        for (int i = 0; i < p.Count; i++) {
          var z = p.GetMeshPosition(p[i - 1]);
          var a = p.GetMeshPosition(p[i + 0]);
          var b = p.GetMeshPosition(p[i + 1]);
          var c = p.GetMeshPosition(p[i + 2]);

          var az = z - a;
          var ab = b - a;
          var ba = a - b;
          var bc = c - b;

          var zabDir = (Quaternion.AngleAxis(Vector3.Angle(az, ab) * 0.5f, normal) * ab).normalized;
          var abcDir = (Quaternion.AngleAxis(Vector3.Angle(ba, bc) * 0.5f, normal) * bc).normalized;

          var e0 = a + (zabDir * 0.005f);
          var e1 = b + (abcDir * 0.005f);

          drawer.DrawWireCapsule(e0, e1, 0.001f);
          drawer.DrawWireCapsule(e0, e1, 0.0015f);
        }
      }
    }

    #endregion

  }

}
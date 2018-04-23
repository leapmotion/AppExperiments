

namespace Leap.Unity.Meshing {

  public struct Triangle {
    public int a, b, c;

    public int this[int idx] {
      get {
        if (idx == 0) return a;
        if (idx == 1) return b;
        return c;
      }
    }

    public IndexEnumerator GetEnumerator() { return new IndexEnumerator(this); }

    public struct IndexEnumerator {
      private int _curIdx;
      private Triangle _tri;
      public IndexEnumerator(Triangle tri) {
        _tri = tri;
        _curIdx = -1;
      }
      public int Current {
        get {
          return _tri[_curIdx];
        }
      }
      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx < 3;
      }
    }
  }

}
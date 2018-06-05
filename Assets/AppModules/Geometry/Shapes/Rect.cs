using Leap.Unity.Infix;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  using UnityRect = UnityEngine.Rect;

  public struct Rect {
    
    public static readonly Vector3 PLANE_NORMAL = new Vector3(0, 0, -1);
    public static readonly Color DEFAULT_GIZMO_COLOR = LeapColor.cerulean;

    public Vector3 center;
    public Vector2 radii;
    public Transform transform;
    public Matrix4x4? overrideMatrix;

    public Rect(LocalRect localRect, Transform transform)
      : this(localRect.center, localRect.radii, transform) { }

    public Rect(Vector3 center, Vector2 radii, Transform transform = null) {
      this.center = center;
      this.radii = radii;
      this.transform = transform;
      overrideMatrix = null;
    }

    /// <summary>
    /// Local-to-world matrix for this Rect. (Read only.)
    /// </summary>
    public Matrix4x4 matrix {
      get {
        if (overrideMatrix.HasValue) {
          return overrideMatrix.Value * Matrix4x4.Translate(center);
        }
        if (transform == null) {
          return Matrix4x4.Translate(center);
        }
        return transform.localToWorldMatrix * Matrix4x4.Translate(center);
      }
    }

    /// <summary>
    /// Local-to-world pose for the center of this Rect. (Read only.)
    /// 
    /// This extracts a Pose from the attached matrix, eliminating scale information.
    /// </summary>
    public Pose pose {
      get {
        return matrix.GetPose();
      }
    }

    public Vector3 localCorner00 {
      get { return new Vector3(-radii.x, -radii.y); }
    }
    public Vector3 localCorner01 {
      get { return new Vector3(-radii.x, radii.y); }
    }
    public Vector3 localCorner11 {
      get { return new Vector3(radii.x, radii.y); }
    }
    public Vector3 localCorner10 {
      get { return new Vector3(radii.x, -radii.y); }
    }
    
    /// <summary>
    /// Returns whether the given world-space point projects inside this Rect. Optionally
    /// also outputs the calculated rect-space point to point_rect.
    /// </summary>
    public bool ContainsProjectedPoint(Vector3 point) {
      Vector3 unusedPoint_rect;
      return ContainsProjectedPoint(point, out unusedPoint_rect);
    }
    /// <summary>
    /// Returns whether the given world-space point projects inside this Rect. Optionally
    /// also outputs the calculated rect-space point to point_rect.
    /// </summary>
    public bool ContainsProjectedPoint(Vector3 point, out Vector3 point_rect) {
      point_rect = this.matrix.inverse.MultiplyPoint3x4(point);

      var absPoint_rect = point_rect.Abs();
      return absPoint_rect.x <= radii.x && absPoint_rect.y <= radii.y;
    }

    #region Side Enumerator

    public RectWorldSegmentEnumerator segments {
      get { return new RectWorldSegmentEnumerator(this); }
    }
    public struct RectWorldSegmentEnumerator {
      //Rect rect;
      private int _sideIdx;
      private Vector3 corner00;
      private Vector3 corner01;
      private Vector3 corner11;
      private Vector3 corner10;
      public RectWorldSegmentEnumerator(Rect rect) {
        //this.rect = rect;
        _sideIdx = -1;
        corner00 = rect.matrix.MultiplyPoint3x4(rect.localCorner00);
        corner01 = rect.matrix.MultiplyPoint3x4(rect.localCorner01);
        corner11 = rect.matrix.MultiplyPoint3x4(rect.localCorner11);
        corner10 = rect.matrix.MultiplyPoint3x4(rect.localCorner10);
      }
      public RectWorldSegmentEnumerator GetEnumerator() { return this; }
      public LocalSegment3 Current {
        get {
          switch (_sideIdx) {
            case 0: return new LocalSegment3(corner00, corner01);
            case 1: return new LocalSegment3(corner01, corner11);
            case 2: return new LocalSegment3(corner11, corner10);
            case 3: return new LocalSegment3(corner10, corner00);
            default: return new LocalSegment3();
          }
        }
      }
      public bool MoveNext() {
        _sideIdx += 1;
        return _sideIdx < 4;
      }
    }

    public RectLocalSegmentEnumerator localSegments {
      get { return new RectLocalSegmentEnumerator(this); }
    }
    public struct RectLocalSegmentEnumerator {
      //Rect rect;
      private int _sideIdx;
      private Vector3 corner00;
      private Vector3 corner01;
      private Vector3 corner11;
      private Vector3 corner10;
      public RectLocalSegmentEnumerator(Rect rect) {
        //this.rect = rect;
        _sideIdx = -1;
        corner00 = rect.localCorner00;
        corner01 = rect.localCorner01;
        corner11 = rect.localCorner11;
        corner10 = rect.localCorner10;
      }
      public RectLocalSegmentEnumerator GetEnumerator() { return this; }
      public LocalSegment3 Current {
        get {
          switch (_sideIdx) {
            case 0: return new LocalSegment3(corner00, corner01);
            case 1: return new LocalSegment3(corner01, corner11);
            case 2: return new LocalSegment3(corner11, corner10);
            case 3: return new LocalSegment3(corner10, corner00);
            default: return new LocalSegment3();
          }
        }
      }
      public bool MoveNext() {
        _sideIdx += 1;
        return _sideIdx < 4;
      }
    }

    #endregion

    #region Runtime Gizmos

    public void DrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.PushMatrix();
      drawer.matrix = this.matrix;

      Vector3 b = localCorner01, c = localCorner11,
              a = localCorner00, d = localCorner10;

      Vector2 shrink;
      var invRadii = Vector3.one.CompDiv(radii.Abs()).NaNOrInfTo(0f);
      if (radii == Vector2.zero) {
        shrink = Vector2.zero;
      }
      else {
        shrink = Vector2.one * (radii * 0.02f).Abs().CompMin();
      }
      int numRectLines = 8;
      for (int i = 0; i < numRectLines; i++) {
        var shrinkMul = (Vector2.one - shrink.CompMul(invRadii) * i);
        drawer.DrawLine(a * shrinkMul, b * shrinkMul);
        drawer.DrawLine(b * shrinkMul, c * shrinkMul);
        drawer.DrawLine(c * shrinkMul, d * shrinkMul);
        drawer.DrawLine(d * shrinkMul, a * shrinkMul);
      }

      drawer.DrawLine(a, c);
      drawer.DrawLine(b, d);

      //drawer.matrix = Matrix4x4.Scale(new Vector3(1f, 1f, 0f)) * this.matrix;
      //drawer.DrawCube(Vector3.zero, new Vector3(radii.x, radii.y, 1f));

      drawer.PopMatrix();
    }

    #endregion

  }

}


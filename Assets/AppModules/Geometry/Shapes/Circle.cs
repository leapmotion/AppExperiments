using System;
using Leap.Unity.Infix;
using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.Geometry {

  using UnityRect = UnityEngine.Rect;

  [System.Serializable]
  public struct Circle {

    public Transform transform;
    public Vector3 center;
    private Direction3 direction;
    public float radius;

    #region Constructors

    public Circle(LocalCircle localCircle, Transform withTransform)
      : this(localCircle.center, localCircle.direction, localCircle.radius,
             withTransform) { }

    public Circle(float radius = 0.5f, Component transformSource = null)
      : this(default(Vector3), default(Direction3), radius, transformSource) { }

    public Circle(Vector3 center = default(Vector3),
                  Direction3 direction = default(Direction3),
                  float radius = 0.5f,
                  Component transformSource = null) {
      this.transform = (transformSource == null ? null : transformSource.transform);
      this.center = center;
      this.direction = direction;
      this.radius = radius;
    }

    #endregion

    #region Accessors

    /// <summary>
    /// Local-to-world matrix for this Circle.
    /// </summary>
    public Matrix4x4 matrix {
      get {
        if (transform == null) {
          return Matrix4x4.Translate(center);
        }
        return transform.localToWorldMatrix * Matrix4x4.Translate(center);
      }
    }

    /// <summary>
    /// The world position of the center of this Circle (read only). This is dependent on
    /// the state of its Transform if it has one, as well as its defined local-space
    /// center position.
    /// </summary>
    public Vector3 position {
      get {
        return this.matrix.MultiplyPoint3x4(Vector3.zero);
      }
    }

    #endregion

    #region Debug Rendering

    public void DrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      Matrix4x4 m = Matrix4x4.identity;
      if (transform != null) {
        m = transform.localToWorldMatrix;
      }

      drawer.PushMatrix();
      drawer.matrix = m;

      drawer.DrawWireArc(
        center: center,
        normal: direction,
        radialStartDirection: direction.Vec().GetPerpendicular(),
        radius: radius,
        fractionOfCircleToDraw: 1f,
        numCircleSegments: 44
      );

      drawer.PopMatrix();
    }

    public void DrawLines(Action<Vector3, Vector3> lineDrawingFunc) {
      DrawWireArc(
        center: center,
        normal: direction,
        radialStartDirection: direction.Vec().GetPerpendicular(),
        radius: radius,
        fractionOfCircleToDraw: 1f,
        numCircleSegments: 44,
        matrix: transform == null ?
          Matrix4x4.identity : transform.localToWorldMatrix,
        lineDrawingFunc: lineDrawingFunc
      );
    }

    // Welp, RuntimeGizmos might need an overhaul to accept arbitrary drawing
    // functions, because this is super useful.
    public static void DrawWireArc(Vector3 center, Vector3 normal,
                                   float radius, int numCircleSegments,
                                   Action<Vector3, Vector3> lineDrawingFunc,
                                   float fractionOfCircleToDraw = 1.0f,
                                   Matrix4x4? matrix = null,
                                   Vector3? radialStartDirection = null) {
      if (!matrix.HasValue) {
        matrix = Matrix4x4.identity;
      }
      if (!radialStartDirection.HasValue) {
        radialStartDirection = normal.GetPerpendicular();
      }
      
      normal = normal.normalized;
      Vector3 radiusVector = radialStartDirection.Value.normalized * radius;
      Vector3 nextVector;
      int numSegmentsToDraw = (int)(numCircleSegments * fractionOfCircleToDraw);
      Quaternion rotator = Quaternion.AngleAxis(360f / numCircleSegments, normal);
      for (int i = 0; i < numSegmentsToDraw; i++) {
        nextVector = rotator * radiusVector;
        lineDrawingFunc(
          matrix.Value.MultiplyPoint3x4(center + radiusVector),
          matrix.Value.MultiplyPoint3x4(center + nextVector)
        );
        radiusVector = nextVector;
      }
    }

    #endregion

  }

  public static class CircleExtensions {
    
    public static Vector3 GetPerpendicular(this Vector3 v) {
      return Utils.Perpendicular(v);
    }

  }

}

using Leap.Unity;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public static class NewUtils {


    #region Transform Utils

    /// <summary>
    /// Returns a list of transforms including this transform and ALL of its children,
    /// including the children of its children, and the children of their children, and
    /// so on.
    /// 
    /// THIS ALLOCATES GARBAGE. Use it for editor code only.
    /// </summary>
    public static List<Transform> GetSelfAndAllChildren(this Transform t,
                                                       bool includeInactiveObjects = false) {
      var allChildren = new List<Transform>();

      Stack<Transform> toVisit = Pool<Stack<Transform>>.Spawn();

      try {
        // Traverse the hierarchy of this object's transform to find all of its Colliders.
        toVisit.Push(t.transform);
        Transform curTransform;
        while (toVisit.Count > 0) {
          curTransform = toVisit.Pop();

          // Recursively search children and children's children
          foreach (var child in curTransform.GetChildren()) {
            // Ignore children with Rigidbodies of their own; its own Rigidbody
            // owns its own colliders and the colliders of its children
            if (includeInactiveObjects || child.gameObject.activeSelf) {
              toVisit.Push(child);
            }
          }

          // Since we'll visit every valid child, all we need to do is add the colliders
          // of every transform we visit.
          allChildren.Add(curTransform);
        }
      }
      finally {
        toVisit.Clear();
        Pool<Stack<Transform>>.Recycle(toVisit);
      }

      return allChildren;
    }

    #endregion

    #region Geometry Utils

    #region Plane & Rect Clamping

    public static Vector3 GetLocalPlanePosition(this Vector3 worldPosition, Pose planePose) {
      var localSpacePoint = worldPosition.From(planePose).position;
      return localSpacePoint;
    }

    public static Vector3 ClampedToPlane(this Vector3 worldPosition, Pose planePose) {
      var localSpacePoint = worldPosition.From(planePose).position;

      var localSpaceOnPlane = new Vector3(localSpacePoint.x,
                                        localSpacePoint.y, 0f);

      return (planePose * localSpaceOnPlane).position;
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight) {
      bool unused;
      return worldPosition.ClampedToRect(rectCenterPose, rectWidth, rectHeight, out unused);
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight,
                                        out bool isProjectionWithinRect) {
      var localSpacePoint = worldPosition.From(rectCenterPose).position;

      var localSpaceOnPlane = new Vector3(Mathf.Clamp(localSpacePoint.x, -rectWidth,  rectWidth),
                                        Mathf.Clamp(localSpacePoint.y, -rectHeight, rectHeight), 0f);

      isProjectionWithinRect = Mathf.Abs(localSpacePoint.x) <= rectWidth;
      isProjectionWithinRect &= Mathf.Abs(localSpacePoint.y) <= rectHeight;

      return (rectCenterPose * localSpaceOnPlane).position;
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight,
                                        out float sqrDistToRect,
                                        out bool isProjectionInRect) {
      var localSpacePoint = worldPosition.From(rectCenterPose).position;

      isProjectionInRect = Mathf.Abs(localSpacePoint.x) <= rectWidth / 2f;
      isProjectionInRect &= Mathf.Abs(localSpacePoint.y) <= rectHeight / 2f;

      var localSpaceOnPlane = new Vector3(Mathf.Clamp(localSpacePoint.x, -rectWidth / 2f,  rectWidth / 2f),
                                        Mathf.Clamp(localSpacePoint.y, -rectHeight / 2f, rectHeight / 2f), 0f);

      var positionOnRect = (rectCenterPose * localSpaceOnPlane).position;

      sqrDistToRect = (positionOnRect - worldPosition).sqrMagnitude;

      return positionOnRect;
    }

    #endregion

    #endregion

    #region RuntimeGizmoDrawer Utils

    public static void DrawPose(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                                Pose pose, float radius = 0.10f,
                                bool drawCube = false) {
      drawer.PushMatrix();

      drawer.matrix = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);

      var origColor = drawer.color;

      //drawer.DrawWireSphere(Vector3.zero, radius);
      if (drawCube) {
        drawer.DrawCube(Vector3.zero, Vector3.one * radius * 0.3f);
      }
      drawer.DrawPosition(Vector3.zero, radius * 2);

      drawer.color = origColor;

      drawer.PopMatrix();
    }

    public static void DrawRay(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                               Vector3 position, Vector3 direction) {
      drawer.DrawLine(position, position + direction);
    }

    public static void DrawDashedLine(this RuntimeGizmos.RuntimeGizmoDrawer drawer, 
                                      Vector3 a, Vector3 b,
                                      float segmentsPerMeter = 32f,
                                      float normalizedPhaseOffset = 0f) {
      var distance = (b - a).magnitude;
      var numSegments = distance * segmentsPerMeter;
      var segmentLength = distance / numSegments;

      var dir = (b - a) / distance;

      for (float i = normalizedPhaseOffset; i < numSegments; i += 2) {
        var start = a + dir * segmentLength * i;
        var end   = a + dir * Mathf.Min(segmentLength * (i + 1), distance);

        drawer.DrawLine(start, end);
      }
    }

    #endregion

    #region Color Utils

    public static Color Lerp(this Color a, Color b, float t) {
      return Color.Lerp(a, b, t);
    }

    #endregion

    #region Nullable Utils

    public static T? ValueOr<T>(this T? foo, T? other) where T : struct {
      if (foo.HasValue) {
        return foo;
      }
      return other;
    }

    #endregion

  }

  #region Grids

  public struct GridPoint {
    public int x, y;
    public Vector3 rootPos;
    public float cellWidth, cellHeight;

    public Vector3 centerPos { get { return rootPos + new Vector3(cellWidth / 2f, -cellHeight / 2f); } }

    public int gridId;
  }

  public struct GridPointEnumerator {

    public Vector2 size;
    public int numRows, numCols;
    public Matrix4x4 transform;

    private int _index;
    private Vector2 _cellSize;

    public GridPointEnumerator(Vector2 size, int numRows, int numCols) {
      if (numRows < 1) numRows = 1;
      if (numCols < 1) numCols = 1;

      this.size = size;
      this.numRows = numRows;
      this.numCols = numCols;

      this.transform = Matrix4x4.identity;

      this._index = -1;
      _cellSize = new Vector2(size.x / numCols, size.y / numRows);
    }

    public GridPointEnumerator GetEnumerator() { return this; }

    private int maxIndex { get { return numRows * numCols - 1; } }

    public bool MoveNext() {
      _index += 1;
      return _index <= maxIndex;
    }

    public GridPoint Current {
      get {
        var x = _index % numCols;
        var y = _index / numCols;
        var pos = transform * (new Vector3(-(size.x / 2) + _cellSize.x * x,
                                           (size.y / 2) - _cellSize.y * y));
        return new GridPoint() {
          x = x,
          y = y,
          rootPos = pos,
          gridId = _index,
          cellWidth = _cellSize.x,
          cellHeight = _cellSize.y
        };
      }
    }

  }

  #endregion

}


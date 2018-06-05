using Leap.Unity;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pose = Leap.Unity.Pose;

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

      Queue<Transform> toVisit = Pool<Queue<Transform>>.Spawn();

      try {
        // Traverse the hierarchy of this object's transform to find all of its Colliders.
        toVisit.Enqueue(t.transform);
        Transform curTransform;
        while (toVisit.Count > 0) {
          curTransform = toVisit.Dequeue();

          // Recursively search children and children's children
          foreach (var child in curTransform.GetChildren()) {
            if (includeInactiveObjects || child.gameObject.activeSelf) {
              toVisit.Enqueue(child);
            }
          }
          
          allChildren.Add(curTransform);
        }
      }
      finally {
        toVisit.Clear();
        Pool<Queue<Transform>>.Recycle(toVisit);
      }

      return allChildren;
    }

    /// <summary>
    /// Scans all the children in order of the argument Transform, appending each transform
    /// it finds to toFill. Children are added depth-first by default.
    ///
    /// Pass breadthFirst: true to fill the list breadth-first instead.
    /// </summary>
    public static void GetAllChildren(this Transform t, List<Transform> toFill,
                                      bool breadthFirst = false) {
      if (breadthFirst) {
        var cursor = t; var cursorIdx = toFill.Count; var endIdx = cursorIdx;
        do {
          endIdx += addImmediateChildren(cursor, toFill);
          cursorIdx += 1;
          if (cursorIdx >= endIdx) break;
          cursor = toFill[cursorIdx];
        } while (true);
      }
      else {
        addChildrenRecursive(t, toFill);
      }
    }
    private static void addChildrenRecursive(Transform t, List<Transform> list) {
      foreach (var child in t.GetChildren()) {
        list.Add(child);
        addChildrenRecursive(child, list);
      }
    }
    private static int addImmediateChildren(Transform t, List<Transform> list) {
      int numChildren = 0;
      foreach (var child in t.GetChildren()) {
        list.Add(child); numChildren++;
      }
      return numChildren;
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

    #region Rigidbody Utils

    /// <summary>
    /// Sets body.position and body.rotation using the argument Pose.
    /// </summary>
    public static void SetPose(this Rigidbody body, Pose pose) {
      body.position = pose.position;
      body.rotation = pose.rotation;
    }

    /// <summary>
    /// Calls body.MovePosition() and body.MoveRotation() using the argument Pose.
    /// </summary>
    public static void MovePose(this Rigidbody body, Pose pose) {
      body.MovePosition(pose.position);
      body.MoveRotation(pose.rotation);
    }

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

    #region Gradient Utils

    public static Texture2D ToTexture(this Gradient gradient, int resolution = 256, TextureFormat format = TextureFormat.ARGB32) {
      Texture2D tex = new Texture2D(resolution, 1, format, mipmap: false, linear: true);
      tex.filterMode = FilterMode.Bilinear;
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.hideFlags = HideFlags.HideAndDontSave;

      for (int i = 0; i < resolution; i++) {
        float t = i / (resolution - 1.0f);
        tex.SetPixel(i, 0, gradient.Evaluate(t));
      }
      tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
      return tex;
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

    #region Pose Utils

    public static Pose Integrated(this Pose thisPose, Movement movement, float deltaTime) {
      thisPose.position = movement.velocity * deltaTime + thisPose.position;

      if (movement.angularVelocity.sqrMagnitude > 0.00001f) {
        var angVelMag = movement.angularVelocity.magnitude;
        thisPose.rotation = Quaternion.AngleAxis(angVelMag * deltaTime,
                                                 movement.angularVelocity / angVelMag)
                            * thisPose.rotation;
      }

      return thisPose;
    }

    #endregion

    #region Vector3 Utils

    /// <summary>
    /// Returns a copy of the input Vector3 with a different X component.
    /// </summary>
    public static Vector3 WithX(this Vector3 v, float x) {
      return new Vector3(x, v.y, v.z);
    }

    /// <summary>
    /// Returns a copy of the input Vector3 with a different Y component.
    /// </summary>
    public static Vector3 WithY(this Vector3 v, float y) {
      return new Vector3(v.x, y, v.z);
    }

    /// <summary>
    /// Returns a copy of the input Vector3 with a different Z component.
    /// </summary>
    public static Vector3 WithZ(this Vector3 v, float z) {
      return new Vector3(v.x, v.y, z);
    }

    /// <summary>
    /// Returns the values of this vector clamped component-wise with minimums from minV
    /// and maximums from maxV.
    /// </summary>
    public static Vector3 Clamped(this Vector3 v, Vector3 minV, Vector3 maxV) {
      return new Vector3(Mathf.Clamp(v.x, minV.x, maxV.x),
                         Mathf.Clamp(v.y, minV.y, maxV.y),
                         Mathf.Clamp(v.z, minV.z, maxV.z));
    }

    /// <summary>
    /// Infix method to convert a Vector3 to a Vector2.
    /// </summary>
    public static Vector2 ToVector2(this Vector3 v) {
      return v;
    }

    public static Vector3 RotatedAround(this Vector3 v, 
                                        Vector3 aroundPoint, float angle, Vector3 axis) {
      var vFromPoint = v - aroundPoint;
      vFromPoint = Quaternion.AngleAxis(angle, axis) * vFromPoint;
      return aroundPoint + vFromPoint;
    }

    public static Vector3 Abs(this Vector3 v) {
      return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 NaNOrInfTo(this Vector3 v, float valueIfNaNOrInf) {
      return new Vector3(v.x.NaNOrInfTo(valueIfNaNOrInf), v.y.NaNOrInfTo(valueIfNaNOrInf),
        v.z.NaNOrInfTo(valueIfNaNOrInf));
    }

    #endregion

    #region Vector2 Utils

    /// <summary>
    /// Returns the values of this vector clamped component-wise with minimums from minV
    /// and maximums from maxV.
    /// </summary>
    public static Vector2 Clamped(this Vector2 v, Vector2 minV, Vector2 maxV) {
      return new Vector2(Mathf.Clamp(v.x, minV.x, maxV.x),
                         Mathf.Clamp(v.y, minV.y, maxV.y));
    }

    public static Vector2 Abs(this Vector2 v) {
      return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    #endregion

    #region Float Utils

    public static float Squared(this float f) {
      return f * f;
    }

    public static float Abs(this float f) {
      return Mathf.Abs(f);
    }

    public static float NaNOrInfTo(this float f, float valueIfNaNOrInf) {
      if (float.IsNaN(f) || float.IsNegativeInfinity(f) || float.IsPositiveInfinity(f)) {
        return valueIfNaNOrInf;
      }
      return f;
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


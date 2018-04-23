using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class SlideArmCylinder : MonoBehaviour {

    private float _height = 1f;
    public float height {
      get {
        if (cylinderMeshFilter != null) {
          _height = cylinderMeshFilter.transform.localScale.y * 2;
        }
        return _height;
      }
      set {
        _height = value;
        if (cylinderMeshFilter != null) {
          cylinderMeshFilter.transform.localScale =
            cylinderMeshFilter.transform.localScale.WithY(value) / 2;
        }
      }
    }

    private float _width;
    public float width {
      get {
        if (cylinderMeshFilter != null) {
          _width = cylinderMeshFilter.transform.localScale.x;
        }
        return _width;
      }
      set {
        _width = value;
        if (cylinderMeshFilter != null) {
          cylinderMeshFilter.transform.localScale =
            cylinderMeshFilter.transform.localScale.WithXZ(_width);
        }
      }
    }

    public MeshFilter cylinderMeshFilter;

    private void OnValidate() {
      _height = height;
      _width = width;
    }

  }

  public static class SlideArmCylinderVector3Extensions {

    public static Vector3 WithY(this Vector3 v, float y) {
      return new Vector3(v.x, y, v.z);
    }

    public static Vector3 WithXZ(this Vector3 v, float xz) {
      return new Vector3(xz, v.y, xz);
    }

    public static Vector3 WithXZ(this Vector3 v, float x, float z) {
      return new Vector3(x, v.y, z);
    }

  }

}
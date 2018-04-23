using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  [System.Serializable]
  public struct Sphere {
    public Transform transform;

    [SerializeField]
    private Vector3 _center;
    public Vector3 center {
      get {
        if (transform == null) return _center;
        else return transform.TransformPoint(_center);
      }
      set {
        if (transform == null) _center = value;
        else _center = transform.InverseTransformPoint(value);
      }
    }

    [SerializeField]
    private float _radius;
    public float radius {
      get {
        if (transform == null) return _radius;
        else return transform.lossyScale.x * _radius;
      }
      set {
        if (transform == null) _radius = value;
        else if (transform.lossyScale.x == 0) _radius = 0f;
        else _radius = value / transform.lossyScale.x;
      }
    }

    public Sphere(float radius = 0.10f, Component transformSource = null)
      : this(default(Vector3), radius, transformSource) { }

    public Sphere(Vector3 center = default(Vector3), float radius = 0.10f,
                  Component transformSource = null) {
      this.transform = (transformSource == null ? null : transformSource.transform);
      _center = center;
      _radius = radius;
    }

  }

  public static class SphereExtensions {

    /// <summary>
    /// Constructs a world-space Sphere from a SphereCollider component on a Transform.
    /// The Transform is assumed to have uniform scale.
    /// </summary>
    public static Sphere ToWorldSphere(this SphereCollider sphereCollider) {
      return new Sphere(
        sphereCollider.transform.TransformPoint(sphereCollider.center),
        sphereCollider.transform.lossyScale.x * sphereCollider.radius
      );
    }

  }

}

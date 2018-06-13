using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  public struct Box {

    public Transform transform;
    public Vector3 center;
    public Vector3 radii;

    private Box(LocalBox localBox) {
      this.center = localBox.center;
      this.radii = localBox.radii;

      this.transform = null;
    }

    public Box(LocalBox localBox, Transform withTransform) 
           : this(localBox) {
      this.transform = withTransform;
    }

    /// <summary>
    /// Local-to-world matrix for this Box.
    /// </summary>
    public Matrix4x4 matrix {
      get {
        if (transform == null) {
          return Matrix4x4.Translate(center);
        }
        return transform.localToWorldMatrix * Matrix4x4.Translate(center);
      }
    }

    public void DrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (transform != null) {
        drawer.PushMatrix();

        drawer.matrix = Matrix4x4.TRS(transform.TransformPoint(center),
                                      transform.rotation,
                                      Quaternion.Inverse(transform.rotation)
                                        * transform.TransformVector(radii));

        drawBoxGizmos(drawer, Vector3.zero, Vector3.one);

        drawer.PopMatrix();
      }
      else {
        drawBoxGizmos(drawer, center, radii);
      }
    }

    private void drawBoxGizmos(RuntimeGizmoDrawer drawer, Vector3 center, Vector3 radii) {
      drawer.DrawWireCube(center, radii * 2f);

      drawer.color = drawer.color.WithAlpha(0.05f);
      int div = 3;
      float invDiv = 1f / div;
      for (int i = 0; i < div; i++) {
        for (int j = 0; j < div; j++) {
          for (int k = 0; k < div; k++) {
            drawer.DrawWireCube((center + (new Vector3(i, j, k) - radii) * invDiv * 2),
                                radii * invDiv * 2f);
          }
        }
      }

      drawer.color = drawer.color.WithAlpha(0.04f);
      drawer.DrawCube(center, radii * 2f);
    }

  }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Utilities {

  [AddComponentMenu("Leap Motion/Physics/Ignore Collisions With Bodies")]
  public class IgnoreCollisionsWithBodies : MonoBehaviour {

    public Collider thisCollider = null;

    public List<Rigidbody> ignoreWhichBodies;

    private void Reset() {
      if (thisCollider == null) thisCollider = GetComponent<Collider>();
    }

    private void OnValidate() {
      if (thisCollider == null) thisCollider = GetComponent<Collider>();
    }

    private void Start() {
      if (thisCollider != null) {
        var collidersBuffer = Pool<List<Collider>>.Spawn(); collidersBuffer.Clear();
        try {
          foreach (var body in ignoreWhichBodies) {
            Utils.FindOwnedChildComponents(body, collidersBuffer,
                                           includeInactiveObjects: true);
            foreach (var colliderToIgnore in collidersBuffer) {
              Physics.IgnoreCollision(thisCollider, colliderToIgnore);
            }
            collidersBuffer.Clear();
          }
        }
        finally {
          collidersBuffer.Clear(); Pool<List<Collider>>.Recycle(collidersBuffer);
        }
      }
    }
  }

}

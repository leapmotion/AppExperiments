using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Launcher {

  [AddComponentMenu("")]
  public class FollowTransform : MonoBehaviour {

    public Transform target;

    public enum FollowMode { Update, FixedUpdate }
    public FollowMode mode;

    private void Update() {
      if (mode != FollowMode.Update) return;
      if (target != null && target.gameObject.activeInHierarchy) {
        this.transform.position = target.transform.position;
        this.transform.rotation = target.transform.rotation;
      }
    }

    private void FixedUpdate() {
      if (mode != FollowMode.FixedUpdate) return;
      if (target != null && target.gameObject.activeInHierarchy) {
        this.transform.position = target.transform.position;
        this.transform.rotation = target.transform.rotation;
      }
    }

  }

}

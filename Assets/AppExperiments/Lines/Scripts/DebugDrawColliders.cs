using Leap.Unity.RuntimeGizmos;
using UnityEngine;

public class DebugDrawColliders : MonoBehaviour, IRuntimeGizmoComponent {

  public Color color = Color.green;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    drawer.color = color;
    drawer.DrawColliders(this.gameObject, true, true);
  }
}

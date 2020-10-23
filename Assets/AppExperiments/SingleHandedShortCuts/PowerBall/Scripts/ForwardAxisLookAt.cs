
using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates Forward basis vector towards target
/// with rotation limited to be around the right axis only.
/// </summary>
public class ForwardAxisLookAt : MonoBehaviour {
  public Transform Target;
  public bool UseCameraAsTarget = true;
  void Awake() {
    if (UseCameraAsTarget) Target = Camera.main.transform;
  }

  void Update() {
    if (transform.parent != null) {
      transform.localEulerAngles = new Vector3(90f, 0f, 0f);
      align();
    }
  }

  private void align() {
    Vector3 lookVector = Target.position - transform.position;
    //lookVector.y = transform.position.y;

    float forwardProject = Vector3.Dot(lookVector, transform.forward);
    lookVector += transform.forward * forwardProject;

    Quaternion lookRotate = Quaternion.FromToRotation(transform.forward, lookVector);
    float lookAngle;
    Vector3 lookAxis;
    lookRotate.ToAngleAxis(out lookAngle, out lookAxis);

    transform.Rotate(lookAxis, lookAngle, Space.World);
  }
}
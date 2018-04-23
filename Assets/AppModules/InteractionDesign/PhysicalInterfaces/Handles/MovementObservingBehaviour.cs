using Leap.Unity.Layout;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public abstract class MovementObservingBehaviour : MonoBehaviour, IPoseProvider {

    private DeltaBuffer           _deltaPosBuffer = new DeltaBuffer(5);
    private DeltaQuaternionBuffer _deltaRotBuffer = new DeltaQuaternionBuffer(5);

    private Pose _curPose = Pose.identity;

    protected virtual void Awake() {
      _curPose = this.transform.ToPose();
    }

    protected virtual void OnEnable() {
      _deltaPosBuffer.Clear();
      _deltaRotBuffer.Clear();

      _curPose = this.transform.ToPose();
    }

    protected virtual void LateUpdate() {
      var time = Time.time;
      _curPose = GetPose();
      _deltaPosBuffer.Add(_curPose.position, time);
      _deltaRotBuffer.Add(_curPose.rotation, time);

      _movement = new Movement(_deltaPosBuffer.Delta(), _deltaRotBuffer.Delta());
    }

    public abstract Pose GetPose();

    private Movement _movement = Movement.identity;
    public Movement movement {
      get { return _movement; }
    }

    public virtual bool isMoving {
      get {
        return movement.velocity.sqrMagnitude
          > PhysicalInterfaceUtils.MIN_MOVING_SPEED_SQR;
      }
    }

    public Pose prevPose {
      get { return _curPose + (movement.inverse * Time.deltaTime).inverse.ToPose(); }
    }

  }
  
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Leap.Unity.PhysicalInterfaces {

  public class HandledObject : MonoBehaviour, IHandledObject {

    public Pose curPose {
      get { return this.transform.ToPose(); }
    }

    [SerializeField]
    private IHandledObjectConstraint[] _constraints;
    public IHandledObjectConstraint[] constraints {
      get { return _constraints; }
    }

    private Dictionary<zz2Old_IHandle, Pose> _objToHandleDeltas = null;

    public void MoveByHandle(zz2Old_IHandle attachedHandle,
                             Pose toPose,
                             Vector3 aroundPivot,
                             out Pose newHandleTargetPose) {

      // Move this panel to the handle.


      // Apply handle constraints in order.


      // Output the target pose the handled would need to be to stay attached to this
      // panel rigidly.


      var finalPose = curPose;
      foreach (var constraint in constraints) {
        finalPose = constraint.Apply(toPose, aroundPivot);
      }

      newHandleTargetPose = finalPose.Then(_objToHandleDeltas[attachedHandle]);
    }

  }

  public interface IAttachable<ToType> {

    void Attach(ToType toObj);

    void Detach(ToType fromObj);

  }

  //[System.Serializable]
  //public class AttachedBehaviourList<FromObjType, ToObjType>
  //  : IIndexable<FromObjType> {

  //  [SerializeField]
  //  private List<MonoBehaviour> _attachedBehaviours;

  //  public void OnAfterDeserialize() {

  //  }

  //  public void OnBeforeSerialize() {

  //  }

  //}

}

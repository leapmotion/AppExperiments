using System;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class zz2Old_InteractionObjectHandle : MonoBehaviour, zz2Old_IHandle {

    public InteractionBehaviour intObj;

    void Start() {
      intObj = GetComponent<InteractionBehaviour>();
      intObj.OnGraspedMovement += onGraspedMovement;
    }

    public bool isHeld {
      get { return intObj != null && intObj.isGrasped; }
    }

    private Pose _targetPose = Pose.identity;
    public Pose targetPose {
      get { return _targetPose; }
      protected set {
        _targetPose = value;
      }
    }

    public ReadonlyList<IHandledObject> attachedObjects {
      get {
        throw new System.NotImplementedException();
      }
    }

    private void onGraspedMovement(Vector3 origPos, Quaternion origRot,
                                   Vector3 newPos, Quaternion newRot,
                                   List<InteractionController> controllers) {
      var newPose = new Pose(newPos, newRot);

      var graspingCentroid = controllers.Query()
                                        .Select(c => c.GetGraspPoint())
                                        .Fold((acc, p) => p + acc)
                             / controllers.Count;

      Movement sum = Movement.identity;
      foreach (var handledObj in attachedObjects) {
        Pose newHandleTargetPose;
        handledObj.MoveByHandle(this, newPose, graspingCentroid,
                                 out newHandleTargetPose);

        sum += new Movement(targetPose, newHandleTargetPose, 1f);
      }
      sum /= attachedObjects.Count;
    }

  }

  

  public interface IHandledObjectConstraint {

    Pose Apply(Pose curPose, Vector3 pivotPoint);

  }

  public class HandledObject_LookConstraint : MonoBehaviour, IHandledObjectConstraint {

    public Vector3 lookTarget {
      get {
        // This should be Camera.main.transform.position
        throw new System.NotImplementedException();
      }
    }

    public Vector3 horizonNormal {
      get {
        // This should be Camera.main.transform.parent.up
        throw new System.NotImplementedException();
      }
    }

    public bool flip180;

    public Pose Apply(Pose panelPose, Vector3 pivotPoint) {
      var constrainedPose = PivotLook.Solve(panelPose,
                                            pivotPoint,
                                            lookTarget,
                                            horizonNormal);

      return constrainedPose;
    }

  }

}

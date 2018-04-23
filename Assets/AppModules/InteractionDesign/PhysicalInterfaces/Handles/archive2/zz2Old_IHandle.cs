namespace Leap.Unity.PhysicalInterfaces {

  public interface zz2Old_IHandle {

    bool isHeld { get; }

    Pose targetPose { get; }

    ReadonlyList<IHandledObject> attachedObjects { get; }

  }

}
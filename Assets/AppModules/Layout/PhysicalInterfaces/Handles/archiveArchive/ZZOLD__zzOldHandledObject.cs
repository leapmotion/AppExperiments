using Leap.Unity;
using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity.PhysicalInterfaces {

  public class ZZOLD__zzOldHandledObject : MonoBehaviour {

    #region Inspector

    [Header("Handles")]
    
    /// <summary>
    /// Only put IHandles in here!! TODO: Terrible requirement, enforce via attribute,
    /// whatever, this is a total mess
    /// </summary>
    public List<Transform> includeTheseHandles;
    
    // Rendered via Custom Editor.
    /// <summary>
    /// All handles owned by this HandledObject. Manipulating these handles will
    /// manipulate the HandlesObject in some way -- by default, by moving it.
    /// </summary>
    private List<zzOld_IHandle> _attachedHandles = new List<zzOld_IHandle>();

    /// <summary>
    /// All handles owned by this HandledObject. Read-only.
    /// </summary>
    public ReadonlyList<zzOld_IHandle> attachedHandles {
      get { return _attachedHandles; }
    }

    #endregion

    //#region Unity Events

    //protected virtual void Reset() {
    //  RefreshHandles();
    //}

    //protected virtual void OnValidate() {
    //  RefreshHandles();
    //}

    //protected virtual void OnEnable() {
    //  if (!_handlesInitialized) {
    //    RefreshHandles();
    //  }

    //  initializeHandledObject();

    //  registerHandleCallbacks();
    //}

    //protected virtual void OnDisable() {
    //  unregisterHandleCallbacks();
    //}

    //protected virtual void Update() {
    //  updateHandledObject();
    //}

    //void LateUpdate() {
    //  foreach (var idleHandle in _idleHandles) {
    //    var deltaPose = _handleDeltaPoses[idleHandle];

    //    idleHandle.Move(this.pose.Then(deltaPose));
    //  }
    //}

    //#endregion

    //#region Attached Handles

    //private bool _handlesInitialized = false;

    //public void RefreshHandles() {
    //  //NewUtils.FindOwnedChildComponents(this, _attachedHandles,
    //  //                                  includeInactiveObjects: true);

    //  foreach (var transform in includeTheseHandles) {
    //    if (transform != null) {
    //      var handle = transform.GetComponent<IHandle>();
    //      if (handle != null) {
    //        _attachedHandles.Add(handle);
    //      }
    //    }
    //  }

    //  _handlesInitialized = true;
    //}

    //#endregion

    //#region Handle Events

    //private void registerHandleCallbacks() {
    //  foreach (var handle in _attachedHandles) {
    //    _handleDeltaPoses[handle] = (handle.pose.From(this.pose));
    //  }
    //}

    //private void unregisterHandleCallbacks() {
    //  foreach (var handle in _attachedHandles) {
    //    _handleDeltaPoses.Remove(handle);
    //  }
    //}

    //private struct OldNewPosePair { public Pose oldPose; public Pose newPose; }

    //private Dictionary<IHandle, OldNewPosePair> _handleMotions
    //  = new Dictionary<IHandle, OldNewPosePair>();

    //private void onHandleMoved(IHandle handle, Pose oldPose, Pose newPose) {

    //  OldNewPosePair posePair;
    //  if (_handleMotions.TryGetValue(handle, out posePair)) {
    //    // Exists, just update.
    //    // Old pose stays the same, new pose changes.
    //    posePair.newPose = newPose;
    //    _handleMotions[handle] = posePair;
    //  }
    //  else {
    //    // Add the pose pair.
    //    _handleMotions[handle] = new OldNewPosePair() {
    //      oldPose = oldPose,
    //      newPose = newPose
    //    };
    //  }
    //}

    //private void onHandlePickedUp(IHandle handle) {
    //  _idleHandles.Remove(handle);

    //  _heldHandles.Add(handle);

    //  if (_heldHandles.Count == 1) {
    //    OnPickedUp();
    //    OnPickedUpHandle(this);
    //  }
    //}

    //private void onHandlePlaced(IHandle handle) {
    //  _heldHandles.Remove(handle);

    //  _idleHandles.Add(handle);

    //  if (_heldHandles.Count == 0) {
    //    OnPlaced();
    //    OnPlacedHandle(this);
    //  }
    //}

    //private void onHandlePlacedInContainer(IHandle handle) {
    //  _heldHandles.Remove(handle);

    //  _idleHandles.Add(handle);
      
    //  if (_heldHandles.Count == 0) {
    //    OnPlaced();
    //    OnPlacedHandle(this);
    //  }
    //}

    //private void onHandleThrown(IHandle handle, Vector3 throwVector) {
    //  _heldHandles.Remove(handle);

    //  _idleHandles.Add(handle);

    //  if (_heldHandles.Count == 0) {
    //    OnThrown(throwVector);
    //    OnThrownHandle(this, throwVector);
    //  }
    //}

    //#endregion

    //#region Handled Object

    //public Pose pose {
    //  get {
    //    return this.transform.ToPose();
    //  }
    //}

    ///// <summary> Handles that are currently not held. </summary>
    //private HashSet<IHandle> _idleHandles = new HashSet<IHandle>();
    ///// <summary> Handles that are currently not held. (Read only.) </summary>
    //protected ReadonlyHashSet<IHandle> idleHandles {
    //  get { return _idleHandles; }
    //}

    ///// <summary> Handles that are currently held. </summary>
    //private HashSet<IHandle> _heldHandles = new HashSet<IHandle>();
    ///// <summary> Handles that are currently held. (Read only.) </summary>
    //protected ReadonlyHashSet<IHandle> heldHandles {
    //  get { return _heldHandles; }
    //}

    //public bool isHeld {
    //  get { return _heldHandles.Count > 0; }
    //}

    //private void initializeHandledObject() {
    //  _idleHandles.Clear();
    //  _heldHandles.Clear();

    //  foreach (var handle in _attachedHandles) {
    //    if (handle.isHeld) {
    //      _heldHandles.Add(handle);
    //    }
    //    else {
    //      _idleHandles.Add(handle);
    //    }
    //  }
    //}

    //private void updateHandledObject() {
    //  //updatePreKabschState();

    //  Matrix4x4 kabschResult;
    //  solveHandleKabsch(out kabschResult);

    //  //updatePostKabschState();

    //  updateHandledObjectPose(kabschResult);

    //  _handleMotions.Clear();
    //}

    //#endregion

    //#region Virtual Functions

    //private Dictionary<IHandle, Pose> _handleDeltaPoses
    //  = new Dictionary<IHandle, Pose>();

    //protected virtual void updateHandledObjectPose(Matrix4x4 kabschResult) {
    //  // Move this object based on the deltaPose, but preserve the handles' poses if
    //  // they happen to be child transforms of this object.

    //  // Strategy 1: Preserve original poses.
    //  var idleHandleDeltaPoses = Pool<List<Pose>>.Spawn();
    //  idleHandleDeltaPoses.Clear();
    //  try {
    //    //foreach (var handle in _attachedHandles) {
    //    //  origPoses.Add(handle.pose);
    //    //}

    //    var origPose = this.pose;

    //    //Debug.Log("Delta pose from handles: " + deltaPoseFromHandles);
    //    //Debug.Log("OK, my target pose will be " + this.pose.Then(deltaPoseFromHandles));
    //    //this.transform.SetWorldPose(this.pose.Then(deltaPoseFromHandles));
    //    this.transform.position = kabschResult.GetVector3() + this.transform.position;
    //    this.transform.rotation = kabschResult.GetQuaternion() * this.transform.rotation;

    //    OnMoved();
    //    OnMovedHandle(this, origPose, this.pose);

    //    //int origPosesIdx = 0;
    //    //foreach (var handle in _attachedHandles) {
    //    //  handle.SetPose(origPoses[origPosesIdx++]);
    //    //}
    //  }
    //  finally {
    //    idleHandleDeltaPoses.Clear();
    //    Pool<List<Pose>>.Recycle(idleHandleDeltaPoses);
    //  }

    //  // Strategy 2: Remove child transforms as children, then re-attach after shifting
    //  // the handled object.
    //  //var origParents = Pool<List<Transform>>.Spawn();
    //  //origParents.Clear();
    //  //var handleTransforms = Pool<List<Transform>>.Spawn();
    //  //handleTransforms.Clear();
    //  //try {
    //  //  foreach (var handle in _attachedHandles) {
    //  //    var handleBehaviour = handle as MonoBehaviour;
    //  //    if (handleBehaviour != null) {
    //  //      handleTransforms.Add(handleBehaviour.transform);
    //  //    }
    //  //  }

    //  //  foreach (var handleTransform in handleTransforms) {
    //  //    origParents.Add(handleTransform.parent);
    //  //    handleTransform.parent = null;
    //  //  }

    //  //  this.transform.position = kabschResult.GetVector3() + this.transform.position;
    //  //  this.transform.rotation = kabschResult.GetQuaternion() * this.transform.rotation;

    //  //  int origParentIdx = 0;
    //  //  foreach (var handleTransform in handleTransforms) {
    //  //    handleTransform.parent = origParents[origParentIdx++];
    //  //  }
    //  //}
    //  //finally {
    //  //  origParents.Clear();
    //  //  Pool<List<Transform>>.Recycle(origParents);

    //  //  handleTransforms.Clear();
    //  //  Pool<List<Transform>>.Recycle(handleTransforms);
    //  //}
    //}

    //#endregion

    //#region Kabsch Movement

    //private Interaction.KabschSolver _kabsch = new Interaction.KabschSolver();

    ////private Dictionary<IHandle, Pose> _origHandlePoses = new Dictionary<IHandle, Pose>();

    ////private void updatePreKabschState() {
    ////  // Ensure there's a reference pose for all handle poses we received movement from.
    ////  //foreach (var handle in _heldHandles) {
    ////  //  if (!_origHandlePoses.ContainsKey(handle)) {
    ////  //    _origHandlePoses[handle] = handle.pose;
    ////  //  }
    ////  //}

    ////  // Ensure there's NO reference pose for non-held handles.
    ////  //var removeHandlesFromKabsch = Pool<List<IHandle>>.Spawn();
    ////  //removeHandlesFromKabsch.Clear();
    ////  //try {
    ////  //  foreach (var handlePosePair in _origHandlePoses) {
    ////  //    if (!_heldHandles.Contains(handlePosePair.Key)) {
    ////  //      removeHandlesFromKabsch.Add(handlePosePair.Key);
    ////  //    }
    ////  //  }
    ////  //  foreach (var handle in removeHandlesFromKabsch) {
    ////  //    _origHandlePoses.Remove(handle);
    ////  //  }
    ////  //}
    ////  //finally {
    ////  //  removeHandlesFromKabsch.Clear();
    ////  //  Pool<List<IHandle>>.Recycle(removeHandlesFromKabsch);
    ////  //}
    ////}

    //private void solveHandleKabsch(out Matrix4x4 kabschResult) {
    //  //if (_origHandlePoses.Count == 0) {
    //  //  kabschResult = Matrix4x4.identity;
    //  //  return;
    //  //}
    //  if (_handleMotions.Count == 0) {
    //    kabschResult = Matrix4x4.identity;
    //    return;
    //  }

    //  List<Vector3> origPoints = Pool<List<Vector3>>.Spawn();
    //  origPoints.Clear();
    //  List<Vector3> curPoints = Pool<List<Vector3>>.Spawn();
    //  curPoints.Clear();

    //  try {
    //    Vector3 objectPos = this.pose.position;

    //    foreach (var handlePosesPair in _handleMotions) {
    //      Pose origPose = handlePosesPair.Value.oldPose;
    //      origPoints.Add(origPose.position - objectPos);
    //      origPoints.Add(origPose.position + origPose.rotation * Vector3.up * 0.01f - objectPos);
    //      origPoints.Add(origPose.position + origPose.rotation * Vector3.right * 0.01f - objectPos);

    //      Pose curPose = handlePosesPair.Value.newPose;
    //      curPoints.Add(curPose.position - objectPos);
    //      curPoints.Add(curPose.position + curPose.rotation * Vector3.up * 0.01f - objectPos);
    //      curPoints.Add(curPose.position + curPose.rotation * Vector3.right * 0.01f - objectPos);
    //    }

    //    if (origPoints.Count == 0) {
    //      kabschResult = Matrix4x4.identity;
    //      return;
    //    }

    //    kabschResult = _kabsch.SolveKabsch(origPoints, curPoints);

    //    _handleMotions.Clear();

    //    //solvedPoseFromCurrentPose = solvedMatrix.GetPose();
    //  }
    //  finally {
    //    origPoints.Clear();
    //    Pool<List<Vector3>>.Recycle(origPoints);

    //    curPoints.Clear();
    //    Pool<List<Vector3>>.Recycle(curPoints);
    //  }
    //}

    ////private void updatePostKabschState() {
    ////  foreach (var handle in _attachedHandles) {
    ////    if (_origHandlePoses.ContainsKey(handle)) {
    ////      _origHandlePoses[handle] = handle.pose;
    ////    }
    ////  }
    ////}

    //#endregion

    //#region Access Helpers

    ///// <summary>
    ///// isHeld property support for PlayMaker.
    ///// </summary>
    //public bool GetIsHeld() { return isHeld; }

    //#endregion

    //#region IHandle

    //public Movement movement {
    //  get {
    //    throw new System.NotImplementedException();
    //  }
    //}

    //public Pose deltaPose {
    //  get {
    //    throw new System.NotImplementedException();
    //  }
    //}

    //public Vector3 heldPosition {
    //  get {
    //    return this.pose.position;
    //  }
    //}

    //public void SetPose(Pose pose) {
    //  this.transform.SetWorldPose(pose);

    //  foreach (var handleDeltaPosePair in _handleDeltaPoses) {
    //    handleDeltaPosePair.Key.SetPose(
    //      handleDeltaPosePair.Value.Then(this.pose));
    //  }
    //}

    //public event Action OnPickedUp                          = () => { };
    //public event Action<IHandle> OnPickedUpHandle           = (h) => { };
    //public event Action OnMoved                             = () => { };
    //public event Action<IHandle, Pose, Pose> OnMovedHandle  = (h, p0, p1) => { };
    //public event Action OnPlaced                            = () => { };
    //public event Action<IHandle> OnPlacedHandle             = (h) => { };
    //public event Action OnPlacedInContainer                 = () => { };
    //public event Action<IHandle> OnPlacedHandleInContainer  = (h) => { };
    //public event Action<Vector3> OnThrown                   = (v) => { };
    //public event Action<IHandle, Vector3> OnThrownHandle    = (h, v) => { };

    //#endregion

  }

}

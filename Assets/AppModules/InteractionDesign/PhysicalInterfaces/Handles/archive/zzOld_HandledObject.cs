//using Leap.Unity.Attributes;
//using Leap.Unity.Query;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Leap.Unity.PhysicalInterfaces {

//  public class zzOld_HandledObject : MovementObservingBehaviour, zzOld_IHandle {

//    #region Inspector

//    [Header("Handles (each must be IHandle)")]
//    //[ElementsImplementInterface(typeof(IHandle))]
//    // TODO: Write a custom property drawer that renders ImplementsInterface fields
//    // instead of plain Transform fields.
//    [SerializeField, EditTimeOnly]
//    private Transform[] _handles;
//    public IIndexable<zzOld_IHandle> handles {
//      get {
//        return new zzOld_TransformArrayComponentWrapper<zzOld_IHandle>(_handles);
//      }
//    }

//    #endregion

//    #region Unity Events

//    private Dictionary<zzOld_IHandle, Pose> _objToHandleDeltaPoses
//      = new Dictionary<zzOld_IHandle, Pose>();

//    private zzOld_IHandle _heldHandle = null;
//    public zzOld_IHandle heldHandle { get { return _heldHandle; } }

//    protected override void Awake() {
//      base.Awake();

//      _targetPose = this.pose;
//    }

//    protected virtual void Update() {
//      var objPose = this.pose;

//      if (_heldHandle != null && _heldHandle.wasReleased) {
//        _heldHandle = null;
//      }

//      // Enforces only one handle is held at a time.
//      // This isn't great, but needs to be true for now.
//      {
//        foreach (var handle in handles.GetEnumerator()) {
//          if (handle.wasHeld && handle != _heldHandle) {
//            if (_heldHandle != null) {
//              _heldHandle.Release();
//            }

//            _heldHandle = handle;
//          }
//        }
//      }

//      // Make sure there's a delta pose entry for all currently attached handles.
//      foreach (var handle in handles.GetEnumerator()) {
//        if (!_objToHandleDeltaPoses.ContainsKey(handle)) {
//          _objToHandleDeltaPoses[handle] = handle.pose.From(objPose);
//        }
//      }

//      // Handle movement (easier when only one handle is held at any one time).
//      if (_heldHandle != null) {
//        // Move this object based on the movement of the held handle.
//        var handleToObjPose = _objToHandleDeltaPoses[_heldHandle].inverse;
//        var newObjPose = _heldHandle.targetPose.Then(handleToObjPose);

//        this.targetPose = newObjPose;

//        OnUpdateTarget();
//      }

//      updateMoveToTarget();

//      // Move all handles to match the new pose of this object.
//      foreach (var handle in handles.GetEnumerator()) {
//        var objToHandlePose = _objToHandleDeltaPoses[handle];
//        handle.targetPose = objPose.Then(objToHandlePose);
//      }
//    }

//    public Action OnUpdateTarget = () => { };

//    private void updateMoveToTarget() {
//      pose = PhysicalInterfaceUtils.SmoothMove(prevPose, pose, this.targetPose, 1f);
//    }

//    #endregion

//    #region IHandle

//    public override Pose GetPose() {
//      return pose;
//    }

//    public virtual Pose pose {
//      get { return this.transform.ToPose(); }
//      protected set {
//        this.transform.SetPose(value);
//      }
//    }

//    public Vector3 localPivot {
//      get { return Vector3.zero; }
//    }

//    private Pose _targetPose;
//    public Pose targetPose {
//      get { return _targetPose; }
//      set { _targetPose = value; }
//    }

//    public float rigidness {
//      get { return _heldHandle == null ? 0f : _heldHandle.rigidness; }
//    }

//    public bool isHeld {
//      get {
//        return _heldHandle != null;
//      }
//    }

//    public bool wasHeld {
//      get {
//        return handles.Query().Any(h => h.wasHeld);
//      }
//    }

//    public bool wasMoved {
//      get {
//        return handles.Query().Any(h => h.wasMoved);
//      }
//    }

//    public bool wasReleased {
//      get {
//        return handles.Query().Any(h => h.wasReleased);
//      }
//    }

//    public bool wasThrown {
//      get {
//        return handles.Query().Any(h => h.wasReleased);
//      }
//    }

//    public void Hold() {
//      Debug.LogError("Can't hold a HandledObject directy; instead, call Hold() on one "
//                     + "of one of its Handles.");
//    }

//    public void Release() {
//      if (_heldHandle != null) {
//        _heldHandle.Release();

//        _heldHandle = null;
//      }
//    }

//    #endregion

//  }

//  public struct zzOld_TransformArrayComponentWrapper<GetComponentType>
//                : IIndexable<GetComponentType>
//  {
//    Transform[] _arr;

//    public zzOld_TransformArrayComponentWrapper(Transform[] arr) {
//      _arr = arr;
//    }

//    public GetComponentType this[int idx] {
//      get { return _arr[idx].GetComponent<GetComponentType>(); }
//    }

//    public int Count { get { return _arr.Length; } }
//  }

//}

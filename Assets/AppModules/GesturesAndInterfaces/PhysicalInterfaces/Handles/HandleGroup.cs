using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class HandleGroup : MonoBehaviour, IHandle {

    [Header("Handles (Must Implement IHandle!)")]
    [SerializeField]
    private GameObject[] _handles;
    public IIndexable<IHandle> handles {
      // TODO: Create something like an "ElementsImplementInterface" attribute
      get { return new GameObjectArrayComponentWrapper<IHandle>(_handles); }
    }

    #region hopes and dreams

    // Something like this might also work, but it's not very graceful, and not generic
    // either :(
    //[System.Serializable]
    //public struct ImplementsIHandleWrapper {
    //  [SerializeField]
    //  [ImplementsInterface(typeof(IHandle))]
    //  private MonoBehaviour _handleObj;
    //  public IHandle handle { get { return _handleObj as IHandle; } }
    //}
    //public ImplementsIHandleWrapper[] serializedHandleArray;

    // THIS would work, but if we're going to go THAT far, might as well just make a 
    // collection struct that implements all sorts of useful interfaces and has events
    // for listening to membership changes, and is drawn more nicely...
    //[System.Serializable]
    //public struct ImplementsInterfaceWrapper<T> where T : class {
    //  [SerializeField]
    //  [ImplementsInterfaceViaTypeProperty("getTypeProperty")]
    //  private MonoBehaviour _handleObj;
    //  public T handle { get { return _handleObj as T; } }

    //  public Type getTypeProperty() {
    //    return typeof(T);
    //  }
    //}
    //public ImplementsInterfaceWrapper<IHandle>[] GENERIC_serializedHandleArray;

    #endregion

    protected HashSet<IHandle> _heldHandles = new HashSet<IHandle>();

    public virtual bool isHeld {
      get {
        return _heldHandles.Count > 0;
      }
    }

    public virtual bool isMoving {
      get {
        return handles.Query().Any(handle => handle.isMoving);
      }
    }

    /// <summary>
    /// This property is ignored currently when set, and always returns false.
    /// 
    /// TODO: It would be interesting to allow a HandleGroup to have its OWN
    /// InteractionObject associated with it that can be directly grabbed, to manipulate
    /// a more complicated graph of handle-handle connections.
    /// </summary>
    public virtual bool isKinematic {
      get {
        return false;
      }
      set {
        return;
      }
    }

    protected virtual void Start() {
      foreach (var handle in handles.GetEnumerator()) {
        handle.OnHandleHoldBegin += onHandleGraspBegin;
        handle.OnHandleHoldEnd   += onHandleGraspEnd;
      }
    }

    protected virtual void onHandleGraspBegin(IHandle handle) {
      _heldHandles.Add(handle);

      foreach (var otherHandle in handles.Query()
                                         .Where(h => h != handle && !h.isHeld)) {
        otherHandle.isKinematic = false;
      }

      if (_heldHandles.Count == 1) {
        OnHoldBegin();
        OnHandleHoldBegin(this);
      }
    }

    protected virtual void onHandleGraspEnd(IHandle handle) {
      // The last-held-handle should remain kinematic, allowing the final pose of the
      // system to relax around that last-placed handle.
      if (_heldHandles.Count == 1) {
        handle.isKinematic = true;
      }

      _heldHandles.Remove(handle);

      if (_heldHandles.Count == 0) {
        OnHoldEnd();
        OnHandleHoldEnd(this);
      }
    }

    public event Action OnHoldBegin = () => { };
    public event Action<IHandle> OnHandleHoldBegin = (thisHandle) => { };

    public event Action OnHoldEnd = () => { };
    public event Action<IHandle> OnHandleHoldEnd = (thisHandle) => { };

  }

  public struct GameObjectArrayComponentWrapper<GetComponentType>
                : IIndexable<GetComponentType> {
    GameObject[] _arr;

    public GameObjectArrayComponentWrapper(GameObject[] arr) {
      _arr = arr;
    }

    public GetComponentType this[int idx] {
      get { return _arr[idx].GetComponent<GetComponentType>(); }
    }

    public int Count { get { return _arr.Length; } }
  }

}

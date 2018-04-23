using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  public class DrivenGameObjectPool {

    /// <summary>
    /// Pools dictionary, keying GameObjectComponentDescription.GetHash() results to
    /// GameObjects
    /// </summary>
    private static Dictionary<GameObjectComponentDescription,
                              DrivenGameObjectPool> _pools
      = new Dictionary<GameObjectComponentDescription, DrivenGameObjectPool>();

    private static DrivenGameObjectPool getOrCreatePool(
                     GameObjectComponentDescription forComponents) {
      DrivenGameObjectPool pool;
      if (!_pools.TryGetValue(forComponents, out pool)) {
        _pools[forComponents] = pool = new DrivenGameObjectPool();
        pool._components = forComponents;
      }
      return pool;
    }

    public static DrivenGameObject Spawn(GameObjectComponentDescription withComponents) {
      return getOrCreatePool(withComponents).Spawn();
    }

    private Stack<DrivenGameObject> _objStack = new Stack<DrivenGameObject>();

    /// <summary>
    /// The DrivenGameObjectPool is itself associated with a GameObject, the parent of
    /// all GameObjects in its pool.
    /// </summary>
    private GameObject _backingPoolGameObject = null;
    public GameObject poolGameObject {
      get {
        if (_backingPoolGameObject == null) {
          _backingPoolGameObject = new GameObject("__Driven Game Object Pool__");
          _backingPoolGameObject.SetActive(false);
        }
        return _backingPoolGameObject;
      }
    }

    private GameObjectComponentDescription _components;
    public GameObjectComponentDescription componentTypes {
      get { return _components; }
    }

    public DrivenGameObject Spawn() {
      if (_objStack.Count > 0) {
        return _objStack.Pop();
      }
      return new DrivenGameObject(this);
    }

    public void Recycle(DrivenGameObject drivenGameObject) {
      if (drivenGameObject.pool != this) {
        throw new InvalidOperationException(
          "[DrivenGameObjectPool] Tried to recycle a DrivenGameObject into this pool, but "
        + "it did not originate from this pool.");
      }

      drivenGameObject.gameObject.transform.parent = poolGameObject.transform;

      _objStack.Push(drivenGameObject);
    }

  }

}
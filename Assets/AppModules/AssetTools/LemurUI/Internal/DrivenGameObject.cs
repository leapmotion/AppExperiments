using System;
using UnityEngine;

namespace Leap.Unity.LemurUI {
  
  public class DrivenGameObject {

    private DrivenGameObjectPool _pool = null;
    public DrivenGameObjectPool pool { get { return _pool; } }

    private GameObject _gameObject = null;
    public GameObject gameObject {
      get {
        if (_gameObject == null) {
          _gameObject = new GameObject("__Driven Game Object__");
        }
        return _gameObject;
      }
    }

    public DrivenGameObject(DrivenGameObjectPool pool) {
      if (pool == null) {
        throw new ArgumentNullException(
          "DrivenGameObject instances must belong to a pool.");
      }

      _pool = pool;

      foreach (var componentType in pool.componentTypes) {
        gameObject.AddComponent(componentType);
      }
    }

    public void Recycle() {
      pool.Recycle(this);
    }

  }

}

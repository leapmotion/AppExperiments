using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PrefabPool : MonoBehaviour {

    /// <summary>
    /// The prefab to be pooled. Spawn() to get a prefab, and
    /// be sure to Recycle() it when you're done! Prefab pools
    /// work best for transient GameObjects, where the cost of
    /// instantiating the prefab is larger than the cost of
    /// maintaining copies of the GameObject in the scene.
    /// </summary>
    [SerializeField, EditTimeOnly]
    [OnEditorChange("prefab")]
    private GameObject _prefab;
    public GameObject prefab {
      get {
        return _prefab;
      }
      set {
        this.gameObject.name = value.name + " Pool";
        _prefab = value;
      }
    }

    private Stack<GameObject> _pool = new Stack<GameObject>();

    public GameObject Spawn() {
      GameObject obj;

      if (_pool.Count == 0) {
        obj = Instantiate(prefab);
        obj.transform.parent = this.transform;
      }
      else {
        obj = _pool.Pop();
      }

      return obj;
    }

    public T Spawn<T>() where T : MonoBehaviour {
      GameObject obj = Spawn();
      return obj == null ? null : obj.GetComponent<T>();
    }

    public void Recycle(GameObject pooledObj) {
      pooledObj.transform.parent = this.transform;
      _pool.Push(pooledObj);
    }

    public void Recycle<T>(T pooledObj) where T : MonoBehaviour {
      Recycle(pooledObj.gameObject);
    }

  }

}

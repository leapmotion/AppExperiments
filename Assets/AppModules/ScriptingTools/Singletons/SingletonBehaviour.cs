using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

    protected static T s_instance = null;
    public static T instance {
      get {
        if (s_instance == null) {
          string objName = "SingletonBehaviour";

          #if UNITY_EDITOR
          objName = typeof(T).Name;
          #endif

          GameObject instanceObj = new GameObject("__" + objName + "__");

          // Set the object to false so Awake() isn't called immediately.
          instanceObj.SetActive(false);

          s_instance = instanceObj.AddComponent<T>();
          s_instance.hideFlags = HideFlags.DontSave;
        }

        return s_instance;
      }
    }

  }

}
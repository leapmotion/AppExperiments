using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class ImplementingBehaviours<T> : IIndexable<T> where T : class {

    public MonoBehaviour[] monoBehaviours;

    public T this[int idx] {
      get { return monoBehaviours[idx] as T; }
    }

    public int Count { get { return monoBehaviours.Length; } }

    public static ImplementingBehaviours<T> FromMonoBehaviours(MonoBehaviour[] monoBehaviours) {
      return new ImplementingBehaviours<T>() { monoBehaviours = monoBehaviours };
    }

  }

}
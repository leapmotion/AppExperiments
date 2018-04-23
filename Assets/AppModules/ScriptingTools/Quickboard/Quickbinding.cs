

//namespace Leap.Unity {
//  using UnityEngine;
//  using UnityObject = UnityEngine.Object;

//  /// <summary>
//  /// A Quickbound variable provides static access to memory that is triply-keyed:
//  /// (1) By type; (2) By a context string; (3) by a key string.
//  /// </summary>
//  public struct Quickbound<T> {
//    public string context;
//    public string key;

//    public Quickbound(string context, string key, T initialValue) {
//      this.context = context;
//      this.key = key;

//      Set(initialValue);
//    }

//    public Quickbound(string context, string key) {
//      this.context = context;
//      this.key = key;
//    }

//    public void Set(T value) {
//      Quickboard.C(context).Set<T>(key, value);
//    }

//    public T Get() {
//      return Quickboard.C(context).Get<T>(key);
//    }

//    public static implicit operator T(Quickbound<T> quickboundT) {
//      return quickboundT.Get();
//    }
//  }

//}
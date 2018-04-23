using System;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  public struct GameObjectComponentDescription : IEquatable<GameObjectComponentDescription> {
    private readonly Type _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7;
    private readonly Hash _typesHash;

    #region Constructors

    public GameObjectComponentDescription(Type t0) {
      _t0 = t0; _t1 = null; _t2 = null; _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1) {
      _t0 = t0; _t1 = t1; _t2 = null; _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = null;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = null; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = null; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = null; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5, Type t6) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = t6; _t7 = null;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }
    public GameObjectComponentDescription(Type t0, Type t1, Type t2, Type t3,
                                          Type t4, Type t5, Type t6, Type t7) {
      _t0 = t0; _t1 = t1; _t2 = t2; _t3 = t3;
      _t4 = t4; _t5 = t5; _t6 = t6; _t7 = t7;

      _typesHash = new Hash() { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };
    }

    public bool Equals(GameObjectComponentDescription other) {
      throw new NotImplementedException();
    }

    #endregion

    public override int GetHashCode() {
      return _typesHash.GetHashCode();
    }

    public ComponentTypeEnumerator GetEnumerator() {
      return new ComponentTypeEnumerator(this);
    }

    public struct ComponentTypeEnumerator {
      GameObjectComponentDescription compDesc;
      private int idx;

      public ComponentTypeEnumerator(GameObjectComponentDescription compDesc) {
        this.compDesc = compDesc;
        idx = -1;
      }

      public Type Current {
        get {
          switch (idx) {
            case 0: return compDesc._t0;
            case 1: return compDesc._t1;
            case 2: return compDesc._t2;
            case 3: return compDesc._t3;
            case 4: return compDesc._t4;
            case 5: return compDesc._t5;
            case 6: return compDesc._t6;
            default: return compDesc._t7;
          }
        }
      }
      public bool MoveNext() {
        idx++; return idx < 8 && Current != null;
      }
    }
  }

  /// <summary>
  /// Lemur UI types declare precisely what combination of components they require on
  /// their GameObjects, which are then generically pooled and managed.
  /// </summary>
  public interface IGameObjectDriver {
    /// <summary>
    /// The components that are required for the IGameObjectDriver to drive a GameObject.
    /// </summary>
    GameObjectComponentDescription requiredComponents { get; }

    // TODO: Make extension method
    //
    // Use DrivenGameObject to handle auto-creation of game objects.....
    // need to figure out the actual life-cycle here
    //
    ///// <summary>
    ///// Gets the GameObject that the IGameObjectDriver is driving; this is a
    ///// pooled resource that can be returned to the pool by calling Recycle(). If there
    ///// is no GameObject being driven yet, this getter will spawn one.
    ///// </summary>
    //GameObject gameObject {
    //  get {
    //    return driven.gameObject;
    //  }
    //}

    DrivenGameObject driven { get; }

    /// <summary>
    /// Recycles any allocated resources into an appropriate pool.
    /// </summary>
    void Recycle();
  }

}
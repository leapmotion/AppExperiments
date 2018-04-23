using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LemurUI {
  
  /// <summary>
  /// Implementing this interface marks a type for construction from Lemur.Default calls.
  /// You'll still have to ensure that there's an instantiatable type mapped for the
  /// abstract type you specified as defaultable, using Lemur.SetDefault.
  /// </summary>
  public interface IDefaultableLemurType { }

  public abstract class LemurUIElement {

    /// <summary>
    /// The Component driving the GameObject associated with this LemurUIElement.
    /// </summary>
    protected abstract IGameObjectDriver gameObjectDriver { get; }

    /// <summary>
    /// Retrieves the current GameObject associated with this LemurUIElement, creating
    /// one if necessary. These GameObjects are pooled; call Recycle() when you are
    /// don't need it anymore.
    /// </summary>
    public GameObject gameObject {
      get { return gameObjectDriver.driven.gameObject; }
    }

    /// <summary>
    /// Call this when you're done with the UI element and it will have its allocated
    /// resources diverted into a pool for future spawns.
    /// </summary>
    public void Recycle() {
      gameObjectDriver.Recycle();
    }

    //public abstract LemurUIElement Duplicate();

  }

  public abstract class GameObjectDriver<LemurUIElement>
                          : IGameObjectDriver
                          where LemurUIElement : LemurUI.LemurUIElement {

    /// <summary>
    /// For simple GameObjectDrivers that only need a GameObject and Transform, the
    /// default (empty) GameObjectComponentDescription suffices.
    /// </summary>
    public virtual GameObjectComponentDescription requiredComponents {
      get {
        return new GameObjectComponentDescription();
      }
    }

    private DrivenGameObject _backingDrivenObjectWrapper;
    /// <summary>
    /// The DrivenGameObject, a wrapper class around the GameObject driven by this
    /// class that handles pooling.
    /// </summary>
    public DrivenGameObject driven {
      get {
        if (_backingDrivenObjectWrapper == null) {
          _backingDrivenObjectWrapper = DrivenGameObjectPool.Spawn(
                                          withComponents: requiredComponents);
        }
        return _backingDrivenObjectWrapper;
      }
    }

    public void Recycle() {
      driven.Recycle();
    }

    /// <summary>
    /// This is where the magic happens! When a LemurUI element needs to do
    /// something in a Unity scene, it binds a Driver to do it. When a Driver is bound
    /// to a valid (non-null) LemurUI element, that Driver is responsible for propagating
    /// the LemurUI element's data to the Unity representation as faithfully as possible.
    /// </summary>
    public abstract void Bind(LemurUIElement element);
  }

}

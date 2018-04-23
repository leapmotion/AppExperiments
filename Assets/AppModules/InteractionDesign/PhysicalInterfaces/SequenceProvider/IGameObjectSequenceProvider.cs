using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Interface for a component that can provide an indexable sequence of GameObjects.
  /// </summary>
  public interface IGameObjectSequenceProvider {

    int Count { get; }

    GameObject this[int idx] { get; }

  }

  /// <summary>
  /// Enumerator for an IGameObjectSequence.
  /// </summary>
  public struct GameObjectSequenceEnumerator {

    IGameObjectSequenceProvider sequence;
    int index;

    public GameObjectSequenceEnumerator(IGameObjectSequenceProvider sequence) {
      this.sequence = sequence;
      index = -1;
    }
    
    public GameObjectSequenceEnumerator GetEnumerator() { return this; }

    public bool MoveNext() { index++;  return index < sequence.Count - 1; }

    public GameObject Current { get { return sequence[index]; } }

    public bool TryGetNext(out GameObject t) {
      var isValid = MoveNext();
      t = null;
      if (isValid) t = Current;
      return isValid;
    }

    public void Reset() {
      index = -1;
    }

  }

  public static class IGameObjectSequenceProviderExtensions {

    public static GameObjectSequenceEnumerator GetEnumerator(this IGameObjectSequenceProvider sequence) {
      return new GameObjectSequenceEnumerator(sequence);
    }

    // deleteme old query system
    //public static QueryWrapper<GameObject, GameObjectSequenceEnumerator> Query(this IGameObjectSequenceProvider sequence) {
    //  return new QueryWrapper<GameObject, GameObjectSequenceEnumerator>(GetEnumerator(sequence));
    //}

  }

}
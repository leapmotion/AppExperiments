using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Interface for a component that can provide an indexable sequence of MonoBehaviours.
  /// </summary>
  public interface IComponentSequenceProvider<T> where T : MonoBehaviour {

    int Count { get; }

    T this[int idx] { get; }

  }

  /// <summary>
  /// Enumerator for an IComponentSequenceProvider<T>.
  /// </summary>
  public struct IComponentSequenceProviderEnumerator<T> where T : MonoBehaviour {

    IComponentSequenceProvider<T> sequence;
    int index;

    public IComponentSequenceProviderEnumerator(IComponentSequenceProvider<T> sequence) {
      this.sequence = sequence;
      index = 0;
    }

    public bool MoveNext() { index++;  return index < sequence.Count - 1; }
    public T Current { get { return sequence[index]; } }

  }

  public static class IComponentSequenceProviderExtensions {

    public static IComponentSequenceProviderEnumerator<T> GetEnumerator<T>(this IComponentSequenceProvider<T> sequence) where T : MonoBehaviour {
      return new IComponentSequenceProviderEnumerator<T>(sequence);
    }

  }

}
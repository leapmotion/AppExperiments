using System.Collections.Generic;

public struct FlowRoutine {
  private IEnumerator<Flow> _baseEnumerator;
  private Stack<IEnumerator<Flow>> _enumeratorStack;

  public FlowRoutine(IEnumerator<Flow> enumerator) {
    _enumeratorStack = null;
    _baseEnumerator = enumerator;

    State = FlowRoutineState.Update;
    LastYieldTime = 0;
    HeapValue = -1;
    HeapIndex = -1;
  }

  public bool MoveNext() {
    var hasNext = topEnumerator().MoveNext();

    if (hasNext) {
      if (topEnumerator().Current.Type == Flow.CoValueType.ForCoroutine) {
        pushEnumerator(topEnumerator().Current.Coroutine as IEnumerator<Flow>);
        return MoveNext();
      } else {
        return true;
      }
    } else {
      if (_enumeratorStack == null || _enumeratorStack.Count == 0) {
        return false;
      } else {
        _enumeratorStack.Pop();
        return MoveNext();
      }
    }
  }

  public bool IsValid {
    get {
      return _baseEnumerator != null;
    }
  }

  public Flow Current {
    get {
      return topEnumerator().Current;
    }
  }

  public FlowRoutineState State { get; private set; }

  public float HeapValue { get; set; }

  public long LastYieldTime { get; set; }

  public int HeapIndex { get; set; }

  private IEnumerator<Flow> topEnumerator() {
    if (_enumeratorStack == null) {
      return _baseEnumerator;
    } else {
      return _enumeratorStack.Peek();
    }
  }

  private void pushEnumerator(IEnumerator<Flow> value) {
    if (_enumeratorStack == null) {
      _enumeratorStack = new Stack<IEnumerator<Flow>>();
    }
    _enumeratorStack.Push(value);
  }

  public enum FlowRoutineState {
    Update,
    FixedUpdate,
    LateUpdate,
    EndOfFrame,
    Thread
  }
}

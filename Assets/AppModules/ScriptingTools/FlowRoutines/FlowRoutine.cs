using System.Collections.Generic;

public struct FlowRoutine {

  private FlowRoutineState _coroutineState;
  private long _lastYieldTime;

  private IEnumerator<Flow> _baseEnumerator;
  private Stack<IEnumerator<Flow>> _enumeratorStack;

  private float _heapValue;
  private int _heapIndex;

  public FlowRoutine(IEnumerator<Flow> enumerator) {
    _baseEnumerator = enumerator;
    _coroutineState = FlowRoutineState.Update;
    _lastYieldTime = 0;
    _enumeratorStack = null;
    _heapValue = -1;
    _heapIndex = -1;
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

  public FlowRoutineState State {
    get {
      return _coroutineState;
    }
  }

  public float HeapValue {
    get {
      return _heapValue;
    }
    set {
      _heapValue = value;
    }
  }

  public long LastYieldTime {
    get {
      return _lastYieldTime;
    }
    set {
      _lastYieldTime = value;
    }
  }

  public int HeapIndex {
    get {
      return _heapIndex;
    }
    set {
      _heapIndex = value;
    }
  }

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

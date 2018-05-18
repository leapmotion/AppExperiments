using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class FlowRunner : MonoBehaviour {

  //Queues of flow routines that will execute on the next frame
  private Queue<FlowRoutine> _intoUpdateQueue = new Queue<FlowRoutine>();
  private Queue<FlowRoutine> _intoFixedUpdateQueue = new Queue<FlowRoutine>();
  private Queue<FlowRoutine> _intoLateUpdateQueue = new Queue<FlowRoutine>();
  private Queue<FlowRoutine> _intoEndOfFrameQueue = new Queue<FlowRoutine>();

  //Heaps of flow routines that will execute N frames in the future
  private FlowHeap _frameUpdateHeap = new FlowHeap();
  private FlowHeap _frameFixedUpdateHeap = new FlowHeap();
  private FlowHeap _frameLateUpdateHeap = new FlowHeap();
  private FlowHeap _frameEndOfFrameHeap = new FlowHeap();

  //Heaps of flow routine that will execute N seconds in the future
  private FlowHeap _timeUpdateHeap = new FlowHeap();
  private FlowHeap _timeFixedUpdateHeap = new FlowHeap();
  private FlowHeap _timeLateUpdateHeap = new FlowHeap();
  private FlowHeap _timeEndOfFrameHeap = new FlowHeap();

  private int _fixedFrameCount = 0;
  private Stopwatch _stopwatch = new Stopwatch();

  private static FlowRunner _cachedInstance = null;
  private static void ensureInstanceExists() {
    if (_cachedInstance == null) {
      _cachedInstance = FindObjectOfType<FlowRunner>();
      if (_cachedInstance == null) {
        _cachedInstance = new GameObject("__FlowRunner").AddComponent<FlowRunner>();
      }
    }
  }

  /// <summary>
  /// Creates and starts a new Flow Routine that runs the given enumerator.
  /// </summary>
  public static void StartNew(IEnumerator<Flow> enumerator) {
    ensureInstanceExists();
    FlowRoutine instance = new FlowRoutine(enumerator);
    _cachedInstance.stepFlowRoutine(instance);
  }

  /// <summary>
  /// Runs the given enumerator to completion without any delay.  Does no yielding or
  /// thread hopping at all, and ignores all yield values.
  /// </summary>
  public static void RunToCompletion(IEnumerator<Flow> enumerator) {
    while (enumerator.MoveNext()) { }
  }

  protected void Awake() {
    _stopwatch.Start();
    StartCoroutine(eventCoroutine());
    StartCoroutine(fixedUpdateCoroutine());
  }

  protected IEnumerator eventCoroutine() {
    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    while (true) {
      yield return null;
      stepQueue(_intoUpdateQueue);
      stepHeap(_frameUpdateHeap, Time.frameCount);
      stepHeap(_timeUpdateHeap, Time.time);

      yield return waitForEndOfFrame;
      stepQueue(_intoLateUpdateQueue);
      stepHeap(_frameLateUpdateHeap, Time.frameCount);
      stepHeap(_timeLateUpdateHeap, Time.time);
    }
  }

  private IEnumerator fixedUpdateCoroutine() {
    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    while (true) {
      yield return waitForFixedUpdate;
      _fixedFrameCount++;
      stepQueue(_intoFixedUpdateQueue);
      stepHeap(_frameFixedUpdateHeap, Time.fixedTime);
      stepHeap(_timeFixedUpdateHeap, _fixedFrameCount);
    }
  }

  private void stepQueue(Queue<FlowRoutine> queue) {
    int elementCount;
    lock (queue) {
      elementCount = queue.Count;
    }

    while (elementCount-- != 0) {
      FlowRoutine element;
      lock (queue) {
        element = queue.Dequeue();
      }
      stepFlowRoutine(element);
    }
  }

  private void stepHeap(FlowHeap heap, float toValue) {
    while (true) {
      FlowRoutine element;
      lock (heap) {
        if (heap.Count == 0 || heap.PeekMin().HeapValue > toValue) {
          break;
        }

        element = heap.RemoveMin();
      }

      stepFlowRoutine(element);
    }
  }

  private void stepFlowRoutine(FlowRoutine routine) {
    if (!routine.IsValid) {
      throw new Exception("Cannot step an invalid flow routine instance!");
    }

    routine.LastYieldTime = _stopwatch.ElapsedTicks;

    while (routine.MoveNext()) {
      Flow current = routine.Current;

      if (current.Type == Flow.CoValueType.WhenElapsed) {
        if (_stopwatch.ElapsedTicks < routine.LastYieldTime + current.Arg) {
          continue;
        } else {
          addIntoQueue(routine);
          return;
        }
      }

      switch (current.Type) {
        case Flow.CoValueType.ForFrames:
          routine.HeapValue = getFrame(routine) + current.Arg;
          insertIntoFrameHeap(routine);
          return;
        case Flow.CoValueType.ForSeconds:
          routine.HeapValue = getTime(routine) + current.Arg;
          insertIntoTimeHeap(routine);
          return;
        case Flow.CoValueType.IntoUpdate:
          lock (_intoUpdateQueue) {
            _intoUpdateQueue.Enqueue(routine);
          }
          return;
        case Flow.CoValueType.IntoFixedUpdate:
          lock (_intoFixedUpdateQueue) {
            _intoFixedUpdateQueue.Enqueue(routine);
          }
          return;
        case Flow.CoValueType.IntoLateUpdate:
          lock (_intoLateUpdateQueue) {
            _intoLateUpdateQueue.Enqueue(routine);
          }
          return;
        case Flow.CoValueType.IntoEndOfFrame:
          lock (_intoLateUpdateQueue) {
            _intoLateUpdateQueue.Enqueue(routine);
          }
          return;
        case Flow.CoValueType.IntoNewThread:
          ThreadPool.QueueUserWorkItem(threadCallback, routine);
          return;
        case Flow.CoValueType.UntilFrame:
          routine.HeapValue = current.Arg;
          insertIntoFrameHeap(routine);
          return;
        case Flow.CoValueType.UntilTime:
          routine.HeapValue = current.Arg;
          insertIntoTimeHeap(routine);
          return;
        default:
          throw new Exception("Unexpected CoValueType " + current.Type);
      }
    }
  }

  private void threadCallback(object context) {
    FlowRoutine instance = (FlowRoutine)context;
    stepFlowRoutine(instance);
  }

  private float getTime(FlowRoutine instance) {
    switch (instance.State) {
      case FlowRoutine.FlowRoutineState.Update:
      case FlowRoutine.FlowRoutineState.LateUpdate:
      case FlowRoutine.FlowRoutineState.EndOfFrame:
        return Time.time;
      case FlowRoutine.FlowRoutineState.FixedUpdate:
        return Time.fixedTime;
      default:
        throw new Exception();
    }
  }

  private int getFrame(FlowRoutine instance) {
    switch (instance.State) {
      case FlowRoutine.FlowRoutineState.Update:
      case FlowRoutine.FlowRoutineState.LateUpdate:
      case FlowRoutine.FlowRoutineState.EndOfFrame:
        return Time.frameCount;
      case FlowRoutine.FlowRoutineState.FixedUpdate:
        return _fixedFrameCount;
      default:
        throw new Exception();
    }
  }

  private void addIntoQueue(FlowRoutine instance) {
    if (!checkNonThreaded(instance)) return;

    Queue<FlowRoutine> queue;

    switch (instance.State) {
      case FlowRoutine.FlowRoutineState.Update:
        queue = _intoUpdateQueue;
        break;
      case FlowRoutine.FlowRoutineState.FixedUpdate:
        queue = _intoFixedUpdateQueue;
        break;
      case FlowRoutine.FlowRoutineState.LateUpdate:
        queue = _intoLateUpdateQueue;
        break;
      case FlowRoutine.FlowRoutineState.EndOfFrame:
        queue = _intoEndOfFrameQueue;
        break;
      default:
        throw new Exception("Unexpected flow state.");
    }

    lock (queue) {
      queue.Enqueue(instance);
    }
  }

  private void insertIntoFrameHeap(FlowRoutine instance) {
    if (!checkNonThreaded(instance)) return;

    FlowHeap heap;

    switch (instance.State) {
      case FlowRoutine.FlowRoutineState.Update:
        heap = _frameUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.FixedUpdate:
        heap = _frameFixedUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.LateUpdate:
        heap = _frameLateUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.EndOfFrame:
        heap = _frameEndOfFrameHeap;
        break;
      default:
        throw new Exception("Unexpected flow state.");
    }

    lock (heap) {
      heap.Insert(instance);
    }
  }

  private void insertIntoTimeHeap(FlowRoutine instance) {
    if (!checkNonThreaded(instance)) return;

    FlowHeap heap;

    switch (instance.State) {
      case FlowRoutine.FlowRoutineState.Update:
        heap = _timeUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.FixedUpdate:
        heap = _timeFixedUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.LateUpdate:
        heap = _timeLateUpdateHeap;
        break;
      case FlowRoutine.FlowRoutineState.EndOfFrame:
        heap = _timeEndOfFrameHeap;
        break;
      default:
        throw new Exception("Unexpected flow state.");
    }

    lock (heap) {
      heap.Insert(instance);
    }
  }

  private bool checkNonThreaded(FlowRoutine routine) {
    if (routine.State == FlowRoutine.FlowRoutineState.Thread) {
      UnityEngine.Debug.LogError("Cannot use temporal Flow types from within a non-Unity thread!  Use one of the Flow.Into values first!");
      return false;
    } else {
      return true;
    }
  }
}

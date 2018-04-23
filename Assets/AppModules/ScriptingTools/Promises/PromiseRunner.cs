//using System;
//using UnityEngine;

//namespace Leap.Unity.Promises {

//  public class PromiseRunner : MonoBehaviour {

//    #region Constants

//    public const int UNSCHEDULED_PROMISE_ID = -1;

//    public const string PROMISE_RUNNER_OBJECT_NAME = "__Promise Runner__";

//    #endregion

//    #region Internal Classes

//    private class PromiseWrapper {
//      public object promiseObj;
//      public Type promiseType;
//      public int promiseId;

//      public void Clear() {
//        promiseObj = null;
//        promiseType = null;
//        promiseId = 0;
//      }
//    }

//    private class ResultPlaceholder { }
//    private static ResultPlaceholder s_resultPlaceholder = new ResultPlaceholder();

//    #endregion

//    #region Memory & Events

//    private static ProduceConsumeBuffer<PromiseWrapper> _pendingPromises
//       = new ProduceConsumeBuffer<PromiseWrapper>(256);

//    private static object[] _resultsArr;

//    public static PromiseRunner instance = null;

//    [RuntimeInitializeOnLoadMethod]
//    private static void RuntimeInitializeOnLoad() {
//      var promiseRunnerObj = new GameObject(PROMISE_RUNNER_OBJECT_NAME);
//      instance = promiseRunnerObj.AddComponent<PromiseRunner>();
//    }

//    private void Update() {
//      staticUpdate();
//    }

//    private static Type[] s_promiseTypeArgsBuffer = new Type[1];
//    private static void staticUpdate() {
//      // TODO: Actually have promises resolved on a separate thread.
//      while (_pendingPromises.Count > 0) {
//        PromiseWrapper promiseWrapper;
//        if (!_pendingPromises.TryDequeue(out promiseWrapper)) {
//          throw new System.InvalidOperationException(
//            "Couldn't dequeue a promise from the pending promises buffer.");
//        }
//        else {
//          s_promiseTypeArgsBuffer[0] = promiseWrapper.promiseType;

//          var genericPromiseType = typeof(Promise<>);
//          var instantiatedPromiseType = genericPromiseType
//                                          .MakeGenericType(s_promiseTypeArgsBuffer);

//          System.Reflection.MethodInfo fulfillMethod = instantiatedPromiseType
//                                                         .GetMethod("Fulfill");

//          object fulfillResult = fulfillMethod.Invoke(promiseWrapper.promiseObj,
//                                                      null);

//          _resultsArr[promiseWrapper.promiseId] = fulfillResult;
//        }
//      }
//    }

//    #endregion

//    #region Static API

//    /// <summary>
//    /// Schedules the execution of a new promise. It is safe to schedule a promise from
//    /// any thread.
//    /// </summary>
//    public static void SchedulePromise<T>(Promise<T> promise) {
//      var promiseWrapper = Pool<PromiseWrapper>.Spawn();
//      promiseWrapper.Clear();

//      promiseWrapper.promiseId = takeFirstAvailableId();
//      promiseWrapper.promiseObj = promise as object;
//      promiseWrapper.promiseType = promise.resultType;

//      promise.NotifyPromiseId(promiseWrapper.promiseId);

//      if (!_pendingPromises.TryEnqueue(ref promiseWrapper)) {
//        throw new System.InvalidOperationException(
//          "Couldn't schedule new promise for a " + typeof(T).ToString()
//        + "; ProduceConsumeBuffer failed to enqueue the promise wrapper.");
//      }
//    }

//    /// <summary>
//    /// Returns a valid promiseId and flags it as taken. To return IDs to the pool,
//    /// call returnPromiseId.
//    /// </summary>
//    /// <returns></returns>
//    private static int takeFirstAvailableId() {
//      for (int i = 0; i < _resultsArr.Length; i++) {
//        if (_resultsArr[i] == null) {
//          _resultsArr[i] = s_resultPlaceholder;
//          return i;
//        }
//      }
//      return -1;
//    }

//    /// <summary>
//    /// Adds the promiseId back to the ID pool for re-use.
//    /// </summary>
//    private static void returnPromiseId(int promiseId) {
//      _resultsArr[promiseId] = null;
//    }

//    /// <summary>
//    /// Gets the result of the promise and returns it, freeing up the promise ID for
//    /// future use.
//    /// </summary>
//    public static T GetFulfilled<T>(Promise<T> promise) {
//      if (_resultsArr[promise.promiseId] == s_resultPlaceholder) {
//        throw new System.InvalidOperationException(
//          "Couldn't fulfill promise for " + promise.resultType.ToString() + " "
//        + "because it wasn't ready yet.");
//      }
//      T toReturn = (T)(_resultsArr[promise.promiseId]);
//      returnPromiseId(promise.promiseId);
//      return toReturn;
//    }

//    public static bool IsResultReady(int promiseId) {
//      return _resultsArr[promiseId] != null
//             && _resultsArr[promiseId] != s_resultPlaceholder;
//    }

//    #endregion

//  }

//}

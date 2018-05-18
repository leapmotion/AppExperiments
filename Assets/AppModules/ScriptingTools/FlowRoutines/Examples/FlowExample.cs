using UnityEngine;
using System.Collections.Generic;

public class FlowExample : MonoBehaviour {
  public const int ARRAY_SIZE = 10000;

  void Start() {
    FlowRunner.StartNew(flowRoutine());
  }

  IEnumerator<Flow> flowRoutine() {
    Debug.Log("Starting flow routine!");

    int[,] array = new int[ARRAY_SIZE, ARRAY_SIZE];
    float startTime;
    int startFrame;

    Debug.Log("Starting expensive operation on main thread...");
    startTime = Time.realtimeSinceStartup;
    startFrame = Time.frameCount;
    for (int i = 0; i < ARRAY_SIZE; i++) {
      for (int j = 0; j < ARRAY_SIZE; j++) {
        array[i, j] = i * j;
      }
    }
    Debug.Log("Ended operation.  Took " + (Time.realtimeSinceStartup - startTime) + " seconds at " + ((Time.frameCount - startFrame) / (Time.realtimeSinceStartup - startTime)) + " FPS");

    Debug.Log("Starting expensive operation on main thread...");
    startTime = Time.realtimeSinceStartup;
    startFrame = Time.frameCount;
    for (int i = 0; i < ARRAY_SIZE; i++) {
      yield return Flow.IfElapsed(4);
      for (int j = 0; j < ARRAY_SIZE; j++) {
        array[i, j] = i * j;
      }
    }
    Debug.Log("Ended operation.  Took " + (Time.realtimeSinceStartup - startTime) + " seconds at " + ((Time.frameCount - startFrame) / (Time.realtimeSinceStartup - startTime)) + " FPS");

    Debug.Log("Starting expensive operation on other thread...");
    startTime = Time.realtimeSinceStartup;
    startFrame = Time.frameCount;
    yield return Flow.IntoNewThread();
    for (int i = 0; i < ARRAY_SIZE; i++) {
      for (int j = 0; j < ARRAY_SIZE; j++) {
        array[i, j] = i * j;
      }
    }
    yield return Flow.IntoUpdate();
    Debug.Log("Ended operation.  Took " + (Time.realtimeSinceStartup - startTime) + " seconds at " + ((Time.frameCount - startFrame) / (Time.realtimeSinceStartup - startTime)) + " FPS");
  }
}

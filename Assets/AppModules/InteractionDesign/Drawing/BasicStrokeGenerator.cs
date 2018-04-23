using Leap.Unity.Meshing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class BasicStrokeGenerator : MonoBehaviour, IStreamReceiver<StrokePoint> {

    public const int MAX_NUM_STROKE_POINTS = 256;

    [Header("Stroke Object Output")]
    public StrokeObject outputStrokeObjectPrefab;
    public Transform  outputParentObject;

    private StrokeObject _curStrokeObject;

    private bool _strokeInProgress = false;

    private void initStroke() {
      _curStrokeObject = Instantiate(outputStrokeObjectPrefab);
      _curStrokeObject.transform.parent = outputParentObject;
    }

    private void addToStroke(StrokePoint strokePoint) {

      using (new ProfilerSample("addToStroke: Restart Stroke")) {
        if (_curStrokeObject.Count > MAX_NUM_STROKE_POINTS) {
          finalizeStroke();
          initStroke();
        }
      }

      using (new ProfilerSample("addToStroke: Modify Stroke")) {
        _curStrokeObject.Add(strokePoint);
      }
    }

    private void finalizeStroke() {
      _curStrokeObject = null;
    }

    #region IStreamReceiver<StrokePoint>

    public void Open() {
      if (!Application.isPlaying) return;

      if (_strokeInProgress) {
        finalizeStroke();
      }

      _strokeInProgress = false;
    }

    public void Receive(StrokePoint strokePoint) {
      if (!Application.isPlaying) return;

      if (!_strokeInProgress) {
        _strokeInProgress = true;

        initStroke();
      }

      using (new ProfilerSample("BasicStrokeGenerator addToStroke")) {
        addToStroke(strokePoint);
      }
    }

    public void Close() {
      if (!Application.isPlaying) return;

      if (_strokeInProgress) {
        finalizeStroke();
      }

      _strokeInProgress = false;
    }

    #endregion

  }

}

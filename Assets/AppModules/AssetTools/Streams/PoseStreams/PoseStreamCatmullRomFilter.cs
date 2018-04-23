using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamCatmullRomFilter : MonoBehaviour,
                                            IStreamReceiver<Pose>,
                                            IStream<Pose> {

    public float samplesPerMeter = 256f;
    public float samplesPer90Degrees = 12f;

    public event Action OnOpen  = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };
    
    private RingBuffer<Pose> _poseBuffer = new RingBuffer<Pose>(4);

    public void Open() {
      _poseBuffer.Clear();

      OnOpen();
    }

    private Pose[] _posesBuffer = new Pose[4];
    private Pose[] _smoothedPosesBuffer = new Pose[512];

    public void Receive(Pose data) {
      bool wasNotFull = false;
      if (!_poseBuffer.IsFull) wasNotFull = true;

      _poseBuffer.Add(data);

      if (_poseBuffer.IsFull) {
        if (wasNotFull) {
          send(_poseBuffer.Get(0), _poseBuffer.Get(0),
               _poseBuffer.Get(1), _poseBuffer.Get(2));
        }
        send(_poseBuffer.Get(0), _poseBuffer.Get(1),
             _poseBuffer.Get(2), _poseBuffer.Get(3));
      }
    }

    private void send(Pose a, Pose b, Pose c, Pose d, bool reverseOutput = false) {

      var length = Vector3.Distance(_posesBuffer[1].position,
                                    _posesBuffer[2].position);
      var numSamplesByPosition = getNumSamplesByPosition(length);

      var angle = Quaternion.Angle(_posesBuffer[1].rotation,
                                   _posesBuffer[2].rotation);
      var numSamplesByRotation = getNumSamplesByRotation(angle);

      var numSamples = Mathf.Max(numSamplesByPosition, numSamplesByRotation);

      var spline = Splines.CatmullRom.ToPoseCHS(a, b, c, d);

      var t = 0f;
      var incr = 1f / numSamples;
      var pose = Pose.identity;
      // Note: We do record the position at t = 1, but it's only _used_ when
      // "reverseOutput" is true, which occurs once at the end of the stream.
      for (int i = 0; i <= numSamples; i++) {
        pose = spline.PoseAt(t);

        _smoothedPosesBuffer[i] = pose;

        t += incr;
      }

      if (!reverseOutput) {
        for (int i = 0; i < numSamples; i++) {
          OnSend(_smoothedPosesBuffer[i]);
        }
      }
      else {
        // Starting _at_ numSamples is intentional: This is the very last pose in the
        // stream.
        for (int i = numSamples; i >= 0; i--) {
          OnSend(_smoothedPosesBuffer[i]);
        }
      }
    }

    private int getNumSamplesByPosition(float length) {
      var numSamples = Mathf.FloorToInt(length * samplesPerMeter);
      numSamples = Mathf.Max(2, numSamples);
      return numSamples;
    }

    private int getNumSamplesByRotation(float angle) {
      var numSamples = Mathf.FloorToInt((angle / 90f) * samplesPer90Degrees);
      numSamples = Mathf.Max(2, numSamples);
      return numSamples;
    }

    public void Close() {
      if (_poseBuffer.Count < 2) {
        return;
      }
      if (_poseBuffer.Count == 2) {
        OnSend(_poseBuffer.Get(0));
        OnSend(_poseBuffer.Get(1));
      }
      else {
        send(_poseBuffer.Get(3), _poseBuffer.Get(3),
             _poseBuffer.Get(2), _poseBuffer.Get(1),
             reverseOutput: true);
      }

      OnClose();
    }

  }

}

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Leap;
using Leap.Unity.Graphing;

namespace Leap.Unity.Networking {
  public class LeapStreamingProvider : LeapEncodingProvider {
    //[NonSerialized]
    public FrameEncoding prevState;
    //[NonSerialized]
    public FrameEncoding lerpState;
    Queue<byte[]> stateBuffer;

    //Interpolation Properties
    float interpolationTime = 0f;
    float sampleInterval = 0.035f;
    float lastTimeFrameAdded = 0f;
    byte[] temp;

    public override void Start() {
      base.Start();
      stateBuffer = new Queue<byte[]>();
    }

    public override void AddFrameState(FrameEncoding state) {
      sampleInterval = Mathf.Clamp(Mathf.Lerp(sampleInterval, Time.realtimeSinceStartup - lastTimeFrameAdded, 0.1f), 0f, 0.5f);
      lastTimeFrameAdded = Time.realtimeSinceStartup;
      stateBuffer.Enqueue(state.data);
    }

    public override void Update() {
      //Constantly adjust the interpolation play speed,
      //so there is always one sample in the buffer
      float playSpeed = stateBuffer.Count * 0.5f;
      //Increment the interpolation timeline
      interpolationTime += Time.unscaledDeltaTime * playSpeed;
      interpolationTime = Mathf.Clamp(interpolationTime, 0f, sampleInterval * 3f);

      //Read from the state buffer until the interpolation timeline is caught up with real time
      while ((stateBuffer.Count > 0) && (interpolationTime > sampleInterval)) {
        prevState.fillEncoding(currentState.data);
        currentState.fillEncoding(stateBuffer.Dequeue());
        interpolationTime -= sampleInterval;
      }

      //Interpolate the real samples according to the interpolation timeline
      lerpState.lerp(prevState, currentState, interpolationTime / sampleInterval);

      //if (RealtimeGraph.Instance != null) { RealtimeGraph.Instance.AddSample("InterpolationAlpha", RealtimeGraph.GraphUnits.Miliseconds, (prevState.RPos == currentState.RPos) ? 10f : 0f); }

      //Decode the interpolated state and send it off
      fillCurrentFrame(lerpState);
      DispatchUpdateFrameEvent(CurrentFrame);
    }
  }
}
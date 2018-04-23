using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Leap.Unity.Networking {
  public class LeapUNETStreamer : NetworkBehaviour {
    public FrameEncodingEnum FrameEncodingType;
    LeapServiceProvider LeapDataProvider;
    LeapStreamingProvider NetworkDataProvider;
    float lastUpdate = 0f;
    float interval = 0.016f;
    byte[] handData;
    [HideInInspector]
    public FrameEncoding playerState;

    // Use this for initialization
    void Start() {
      //Application.targetFrameRate = 60;
      NetworkDataProvider = Resources.FindObjectsOfTypeAll<LeapStreamingProvider>()[0];
      LeapDataProvider = Resources.FindObjectsOfTypeAll<LeapServiceProvider>()[0];

      switch (FrameEncodingType) {
        case FrameEncodingEnum.VectorHand:
          playerState = new VectorFrameEncoding();
          NetworkDataProvider.lerpState = new VectorFrameEncoding();
          NetworkDataProvider.prevState = new VectorFrameEncoding();
          NetworkDataProvider.currentState = new VectorFrameEncoding();
          break;
        case FrameEncodingEnum.CurlHand:
          playerState = new CurlFrameEncoding();
          NetworkDataProvider.lerpState = new CurlFrameEncoding();
          NetworkDataProvider.prevState = new CurlFrameEncoding();
          NetworkDataProvider.currentState = new CurlFrameEncoding();
          break;
        default:
          playerState = new VectorFrameEncoding();
          NetworkDataProvider.lerpState = new VectorFrameEncoding();
          NetworkDataProvider.prevState = new VectorFrameEncoding();
          NetworkDataProvider.currentState = new VectorFrameEncoding();
          break;
      }

      playerState.fillEncoding(handData);
    }

    [ClientRpc]
    void RpcsetState(byte[] data) {
      if (isLocalPlayer && !NetworkServer.active) {
        handData = data;
        playerState.fillEncoding(handData);
        if (NetworkDataProvider) {
          NetworkDataProvider.AddFrameState(playerState); //Enqueue new tracking data on an interval for everyone else
        }
      }
      return;
    }

    // Update is called once per frame
    void Update() {
      if (NetworkServer.active && (Time.time>lastUpdate+interval)) {
        lastUpdate = Time.time;
        playerState.fillEncoding(LeapDataProvider.CurrentFrame, transform);
        NetworkDataProvider.AddFrameState(playerState);
        RpcsetState(playerState.data);
      }
    }
  }
}
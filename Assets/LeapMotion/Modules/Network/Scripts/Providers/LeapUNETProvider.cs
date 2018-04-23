using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Networking;
using UnityEngine.Networking;

namespace Leap.Unity.Networking {
  public class LeapUNETProvider : NetworkBehaviour {
    public const string F_N = "LeapUNETProvider";

    public FrameEncodingEnum frameEncodingType;
    public Transform handController;
    LeapServiceProvider leapDataProvider;
    LeapStreamingProvider networkDataProvider;
    float lastUpdate = 0f;
    float interval = 0.035f;
    byte[] handData;
    [HideInInspector]
    public FrameEncoding playerState;

    // Use this for initialization
    void Start() {
      //Application.targetFrameRate = 60;
      if (isLocalPlayer) {
        switch (frameEncodingType) {
          case FrameEncodingEnum.VectorHand:
            playerState = new VectorFrameEncoding();
            break;
          case FrameEncodingEnum.CurlHand:
            playerState = new CurlFrameEncoding();
            break;
          default:
            playerState = new VectorFrameEncoding();
            break;
        }
        //LeapDataProvider = HandController.gameObject.AddComponent<LeapServiceProvider>();
        leapDataProvider = handController.gameObject.AddComponent<LeapXRServiceProvider>();
        //ENABLE THESE AGAIN ONCE THE SERVICE HAS THESE EXPOSED SOMEHOW
        //LeapDataProvider._temporalWarping = HandController.parent.GetComponent<LeapVRTemporalWarping>();
        //LeapDataProvider._temporalWarping.provider = LeapDataProvider;
        //LeapDataProvider._isHeadMounted = true;
        //LeapDataProvider.UpdateHandInPrecull = true;
      } else {
        networkDataProvider = handController.gameObject.AddComponent<LeapStreamingProvider>();
        //Destroy(handController.parent.GetComponent<LeapXRTemporalWarping>());
        switch (frameEncodingType) {
          case FrameEncodingEnum.VectorHand:
            playerState = new VectorFrameEncoding();
            networkDataProvider.lerpState = new VectorFrameEncoding();
            networkDataProvider.prevState = new VectorFrameEncoding();
            networkDataProvider.currentState = new VectorFrameEncoding();
            break;
          case FrameEncodingEnum.CurlHand:
            playerState = new CurlFrameEncoding();
            networkDataProvider.lerpState = new CurlFrameEncoding();
            networkDataProvider.prevState = new CurlFrameEncoding();
            networkDataProvider.currentState = new CurlFrameEncoding();
            break;
          default:
            playerState = new VectorFrameEncoding();
            networkDataProvider.lerpState = new VectorFrameEncoding();
            networkDataProvider.prevState = new VectorFrameEncoding();
            networkDataProvider.currentState = new VectorFrameEncoding();
            break;
        }
      }
      //handController.gameObject.AddComponent<LeapHandController>();
      playerState.fillEncoding(null);
    }

    [ClientRpc(channel = 1)]
    void RpcsetState(byte[] data) {
      if (!isLocalPlayer) {
        handData = data;
        if (playerState != null && handData != null) {
          playerState.fillEncoding(handData);
          if (networkDataProvider) {
            networkDataProvider.AddFrameState(playerState); //Enqueue new tracking data on an interval for everyone else
          }
        }
      }
      return;
    }

    [Command(channel = 1)]
    void CmdsetState(byte[] data) {
      handData = data;
      playerState.fillEncoding(handData);
      RpcsetState(playerState.data);
    }

    void Update() {
      using (new ProfilerSample("LeapUNET Update", this)) {
        if (isLocalPlayer && (!isServer || (isServer && NetworkManager.singleton.numPlayers > 1))) {
          if (Time.realtimeSinceStartup > lastUpdate + interval) {
            lastUpdate = Time.realtimeSinceStartup;
            playerState.fillEncoding(leapDataProvider.CurrentFrame, handController);
            CmdsetState(playerState.data);
          }
        }
      }
    }
  }
}
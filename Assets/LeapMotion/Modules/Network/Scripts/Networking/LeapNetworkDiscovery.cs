using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

namespace Leap.Unity.Networking {
  public class LeapNetworkDiscovery : NetworkDiscovery {
    bool Server = false;
    NetworkManager NetManager;

    // Use this for initialization
    void Start() {
      Server = ServerState.isServer;
      NetManager = NetworkManager.singleton;
      Initialize();
      if (!Server) {
        StartAsClient();
        //NetManager.StartClient();
      } else {
        StartAsServer();
        NetManager.StartHost();
      }
      if (isServer) {
        enabled = false;
      }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data) {
      base.OnReceivedBroadcast(fromAddress, data);

      if (NetworkManager.singleton != null && NetworkManager.singleton.client == null) {
        Debug.Log(fromAddress + "/" + data);
        NetworkManager.singleton.networkAddress = fromAddress.Remove(0, 7);
        NetworkManager.singleton.networkPort = 7777;// Convert.ToInt32(data);
        NetworkManager.singleton.StartClient();
        //StopBroadcast();
        if (!isServer) {
          enabled = false;
        }
      }
    }
  }
}
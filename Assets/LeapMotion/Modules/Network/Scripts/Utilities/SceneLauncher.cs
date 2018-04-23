using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLauncher : MonoBehaviour {
  public string SceneName = "MultiPlayerNetworking";
  public void StartServer() {
    ServerState.isServer = true;
    SceneManager.LoadScene(SceneName);
  }

  public void StartClient() {
    ServerState.isServer = false;
    SceneManager.LoadScene(SceneName);
  }
}

public static class ServerState {
  public static bool isServer = true;
}
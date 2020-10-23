using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

namespace LeapSingleHandedShortcuts {
  public class PaperAirplaneSpawner : MonoBehaviour {

    public InteractionHand interactionHand;
    public Transform AirplanePrefab;
    public Transform Palm;
    public AudioSource Audio;
    public List<AudioClip> SpawnClips;
    public List<AudioClip> LaunchClips;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
      if (Input.GetKeyUp(KeyCode.P)) SpawnAirplane();
    }

    public List<PaperAirPlane> unLaunchedPlanes = new List<PaperAirPlane>();

    public void SpawnAirplane() {
      if (unLaunchedPlanes.Count > 0) {
        return;
      }
      PaperAirPlane paperAirPlane = Instantiate(AirplanePrefab).GetComponent<PaperAirPlane>();
      paperAirPlane.interactionHand = interactionHand;
      paperAirPlane.Palm = Palm;
      paperAirPlane.OnSpawn();
      unLaunchedPlanes.Add(paperAirPlane);
      paperAirPlane.OnLaunch += onLaunch;
      int spawnSound = Random.Range(0, SpawnClips.Count);
      Audio.PlayOneShot(SpawnClips[spawnSound]);
    }

    private void onLaunch(PaperAirPlane airPlane) {
      airPlane.OnLaunch -= onLaunch;
      unLaunchedPlanes.Remove(airPlane);
      int launchSound = Random.Range(0, LaunchClips.Count);
      Audio.PlayOneShot(LaunchClips[launchSound]);
    }
  }
}

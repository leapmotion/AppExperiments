using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsatorRunner : MonoBehaviour {

  private static bool s_appClosing = false;

  private static PulsatorRunner s_instance;
  public static PulsatorRunner instance {
    get {
      if (s_instance == null) {
        s_instance = new GameObject("__Pulsator Runner__").AddComponent<PulsatorRunner>();
      }
      return s_instance;
    }
  }

  public static void NotifyEnabled(Pulsator pulsator) {
    if (s_appClosing || !Application.isPlaying) return;

    instance.notifyEnabled(pulsator);
  }

  public static void NotifyDisabled(Pulsator pulsator) {
    if (s_appClosing || !Application.isPlaying) return;

    instance.notifyDisabled(pulsator);
  }

  private HashSet<Pulsator> _pulsators = new HashSet<Pulsator>();

  void Update() {
    foreach (var pulsator in _pulsators) {
      pulsator.UpdatePulsator(Time.deltaTime);
    }
  }

  void OnDestroy() {
    s_appClosing = true;
  }

  private void notifyEnabled(Pulsator pulsator) {
    _pulsators.Add(pulsator);
  }

  private void notifyDisabled(Pulsator pulsator) {
    _pulsators.Remove(pulsator);
  }

}

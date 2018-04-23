using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class LODItemController : MonoBehaviour {

    [Range(5f, 180f)]
    public float maxCameraLookAngle = 30f;

    [Range(0.10f, 1f)]
    public float maxDetailDistance = 0.40f;

    public enum ViewMode { Full, Fade, Single }
    public ViewMode viewMode;

    [Header("Fade Mode Options")]
    [Range(0f, 0.80f)]
    public float fadeRadius = 0.20f;

    [Range(0f, 0.90f)]
    public float cutoffActivationPercent = 0.50f;

    public enum FadeDistanceMode { FastFalloff, Linear, SlowFalloff }

    public FadeDistanceMode fadeDistanceMode;

    //public bool smoothDetailFade = false;
    //public bool oneAtATime = true;

    private List<LODItem> items = new List<LODItem>();

    [Header("Debug")]
    public bool drawDebug = false;

    private int counter = 0;

    private void Update() {

      GetComponentsInChildren<LODItem>(items);

      var camera = Camera.main;

      counter += 1;
      var pingThisFrame = false;
      if (counter % 10 == 0) {
        counter = 0;
        if (drawDebug) {
          pingThisFrame = true;
        }
      }

      var selector = GetComponent<PullTabSelector>();

      var closestAngle = float.PositiveInfinity;
      LODItem closestItem = null;
      foreach (var item in items) {
        var testAngle = Vector3.Angle(camera.transform.forward,
                                      item.transform.position - camera.transform.position);
        var testDist = Vector3.Distance(item.transform.position, camera.transform.position);


        if (pingThisFrame) {
          DebugPing.Ping(item.transform.position, LeapColor.blue, 0.08f);

          Debug.Log(testAngle);
        }

        if (testAngle < closestAngle
            && testAngle <= maxCameraLookAngle
            && testDist <= maxDetailDistance) {
          
          closestAngle = testAngle;

          if (selector != null && selector.listOpenCloseAmount < 0.10f) {
            var activeMarbleItem = selector.activeMarbleParent.GetComponentInChildren<LODItem>();
            if (item == activeMarbleItem) {
              closestItem = item;
            }
          }
          else {
            closestItem = item;
          }

          if (pingThisFrame) {
            DebugPing.Ping(item.transform.position, LeapColor.red, 0.09f);
          }
        }
      }


      if (viewMode == ViewMode.Full) {
        foreach (var item in items) {
          if (closestItem != null) {
            if (item.propertySwitch.GetIsOffOrTurningOff()) {
              item.propertySwitch.On();
            }
          }
          else {
            if (item.propertySwitch.GetIsOnOrTurningOn()) {
              item.propertySwitch.Off();
            }
          }
        }
      }
      else if (viewMode == ViewMode.Fade) {
        foreach (var item in items) {
          var detailActivation = 0f;
          if (closestItem != null) {
            float dist;
            switch (fadeDistanceMode) {
              case FadeDistanceMode.FastFalloff:
                dist = Mathf.Sqrt((item.transform.position - closestItem.transform.position).magnitude);
                detailActivation = dist.Map(0f, Mathf.Sqrt(fadeRadius), 1f, 0f);
                break;
              case FadeDistanceMode.SlowFalloff:
                dist = (item.transform.position - closestItem.transform.position).sqrMagnitude;
                detailActivation = dist.Map(0f, fadeRadius * fadeRadius, 1f, 0f);
                break;
              case FadeDistanceMode.Linear:
              default:
                dist = (item.transform.position - closestItem.transform.position).magnitude;
                detailActivation = dist.Map(0f, fadeRadius, 1f, 0f);
                break;
            }

            if (detailActivation < cutoffActivationPercent) detailActivation = 0f;
          }

          if (item.tweenSwitch != null) {
            item.tweenSwitch.SetTweenTarget(detailActivation);
          }
          else {
            Debug.LogError("Can't use Fade view mode for this item; it doesn't use a "
                           + "TweenSwitch.", item);
          }
        }
      }
      else if (viewMode == ViewMode.Single) {
        foreach (var item in items) {
          if (item != closestItem && item.tweenSwitch != null
            && item.propertySwitch.GetIsOnOrTurningOn()) {
            item.propertySwitch.Off();
          }
        }
        // Older: one at a time;
        if (closestItem != null && closestItem.tweenSwitch != null
            && closestItem.propertySwitch.GetIsOffOrTurningOff()) {
          closestItem.propertySwitch.On();
        }
      }

      if (pingThisFrame && closestItem != null) {
        DebugPing.Ping(closestItem.transform.position, LeapColor.purple, 0.10f);
      }
    }

  }

}

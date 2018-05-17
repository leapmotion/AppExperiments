using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Portals;
using Leap.Unity.Interaction;

public class PortalCard : MonoBehaviour {

  public Transform head;
  public Portal portal;

  public float wallSize = 0.01f;
  public Transform topWall;
  public Transform bottomWall;
  public Transform leftWall;
  public Transform rightWall;

  [Header("Portals")]
  public float width;
  public float height;

  [Header("Handles")]
  public Transform portalAnchor;
  public PortalHandle[] handles;
  public float scaleFactor = 1;
  public float transitionPercent = 1;
  public float transitionPastHead = 1.5f;
  public float transitionSpeed = 0.05f;
  public float distanceSmoothing = 0.05f;

  private SmoothedFloat _smoothedDistance;

  public bool reset = false;

  public bool isExpandedOrGrasped {
    get {
      return _smoothedDistance.value > transitionPercent ||
             handles.Query().All(h => h.isGrasped);
    }
  }

  private void OnValidate() {
    portal.width = width;
    portal.height = height;

    updateWalls();
  }

  private void Start() {
    _smoothedDistance = new SmoothedFloat();
    _smoothedDistance.delay = distanceSmoothing;
    _smoothedDistance.reset = true;
  }

  private void Update() {

    float averageDistance = 0;
    int graspedHandles = 0;

    foreach (var handle in handles) {
      if (handle.isGrasped) {
        float distance = Mathf.Max(0, -transform.InverseTransformPoint(handle.position).z);

        averageDistance += distance;
        graspedHandles++;
      } else {
        handle.isGrasped = false;
      }
    }

    if (graspedHandles == 2) {
      _smoothedDistance.Update(averageDistance / graspedHandles, Time.deltaTime);
    } else {
      float distToHead = Vector3.Distance(head.position, transform.position);
      float percentMoved = _smoothedDistance.value / distToHead;

      float goalDist;
      if (percentMoved > transitionPercent) {
        goalDist = Mathf.Min(distToHead * transitionPastHead, _smoothedDistance.value + transitionSpeed * Time.deltaTime);
      } else {
        goalDist = 0;
      }

      if (reset) {
        goalDist = 0;
      }

      _smoothedDistance.Update(goalDist, Time.deltaTime);
    }

    portalAnchor.transform.localPosition = new Vector3(0, 0, -_smoothedDistance.value);
    portal.width = _smoothedDistance.value * scaleFactor + width;
    portal.height = _smoothedDistance.value * scaleFactor + height;

    updateWalls();
    reset = false;
  }

  private void updateWalls() {
    float heightOff = (portal.height + wallSize) / 2;
    float widthOff = (portal.width + wallSize) / 2;

    topWall.transform.localPosition = new Vector3(0, heightOff, 0);
    topWall.transform.localScale = new Vector3(portal.width, wallSize, wallSize);

    bottomWall.transform.localPosition = new Vector3(0, -heightOff, 0);
    bottomWall.transform.localScale = new Vector3(portal.width, wallSize, wallSize);

    leftWall.transform.localPosition = new Vector3(-widthOff, 0, 0);
    leftWall.transform.localScale = new Vector3(wallSize, portal.height + wallSize * 2, wallSize);

    rightWall.transform.localPosition = new Vector3(widthOff, 0, 0);
    rightWall.transform.localScale = new Vector3(wallSize, portal.height + wallSize * 2, wallSize);
  }


}

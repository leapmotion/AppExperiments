using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.Attachments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace LeapSingleHandedShortcuts {
  public class PaperAirPlane : MonoBehaviour {
    public InteractionHand interactionHand;
    public Transform Palm;
    public Rigidbody AirplaneRigidbody;

    public float CurrentVelocity;

    public bool IsSpawning = true;
    public bool IsFlying = false;
    public bool IsAgentControlled = false;

    private Vector3 previousPalmPosition;
    private Vector3 Velocity;
    //public Transform VelocityMarker;
    private Vector3 lastTrackedPosition;
    private Vector3 VelocitySnapShot;
    private Queue<Vector3> velocityList = new Queue<Vector3>();
    private Vector3 averageVelocity;

    public float SpawnDuration = 0.5f;
    public AnimationCurve ScaleCurveX;
    public AnimationCurve ScaleCurveY;
    public AnimationCurve ScaleCurveZ;

    public float AimAngleSpeed = 3f;
    public float AddTorqueMultiplier = 3f;
    public float AddForceMultiplier = .005f;

    public delegate void LaunchAction(PaperAirPlane airplance);
    public event LaunchAction OnLaunch;

    void Awake() {
      AirplaneRigidbody.isKinematic = false;
      StartCoroutine(waitToKinematic());
    }

    private IEnumerator waitToKinematic()
    {
        yield return new WaitForEndOfFrame();
        AirplaneRigidbody.isKinematic = true;
    }

    void Update() {
      if (!IsAgentControlled) {
        CalculateAvergeHandVelocity();
        Vector3 pinchPosition = interactionHand.leapHand.GetPinchPosition();
        float distanceToPinch = (transform.position - pinchPosition).magnitude;
        float pinchStrength = interactionHand.leapHand.PinchStrength;
        if (IsSpawning && pinchStrength > .7f) {
          IsFlying = false;
          transform.position = pinchPosition;
          AirplaneRigidbody.velocity = new Vector3(.0001f, .0001f, .0001f);
          AirplaneRigidbody.angularVelocity = new Vector3(.0001f, .0001f, .0001f);
          transform.parent = Palm;
        }
        else if (!IsFlying && pinchStrength < .2f) {
          OnThrow();
          IsFlying = true;
        }
        if (IsFlying) OnFly();
      }
    }

    public void OnThrow() {
      Debug.Log("OnThrow()");
      transform.parent = null;
      AirplaneRigidbody.isKinematic = false;
      //snapshot and constrain velocity derived
      VelocitySnapShot = averageVelocity;
      VelocitySnapShot = VelocitySnapShot * .3f;// scale the velocity so arm doesn't reach as far;
      //VelocityMarker.position = VelocitySnapShot;
      lastTrackedPosition = interactionHand.leapHand.PalmPosition.ToVector3();
      CalculateAvergeHandVelocity();
      if (IsSpawning) {
        StopAllCoroutines();
        StartCoroutine(UnspawnBehaviour());
        IsFlying = false;
      }
      else if (averageVelocity.magnitude > .1f) {
        AirplaneRigidbody.velocity = averageVelocity;
        if(OnLaunch != null) OnLaunch(this);
      }
      else if (OnLaunch != null) OnLaunch(this);
    }

    public void OnSpawn() {
      Vector3 airPlaneUp = interactionHand.leapHand.GetPinchPosition() - interactionHand.leapHand.WristPosition.ToVector3();
      transform.position = interactionHand.leapHand.GetPinchPosition();
      transform.LookAwayFrom(Camera.main.transform);
      transform.localEulerAngles += new Vector3(-20f, 0f, 0f);
      transform.rotation = Quaternion.LookRotation(transform.forward, airPlaneUp);
      StartCoroutine(SpawnBehavior(SpawnDuration));
    }

    public void OnFly() {
      if (AirplaneRigidbody.velocity.magnitude > 0f) {
        Quaternion targetRotation = Quaternion.LookRotation(AirplaneRigidbody.velocity, transform.up) * Quaternion.Euler(new Vector3(-1f, 0f, 0f) * AirplaneRigidbody.velocity.magnitude);
        AirplaneRigidbody.rotation = Quaternion.Slerp(AirplaneRigidbody.rotation, targetRotation, AimAngleSpeed * AirplaneRigidbody.velocity.magnitude );
        AirplaneRigidbody.AddTorque(transform.right * AirplaneRigidbody.velocity.magnitude * AddTorqueMultiplier);
        AirplaneRigidbody.AddRelativeForce(Vector3.up * AirplaneRigidbody.velocity.magnitude * AddForceMultiplier);
      }
    }


    public void CalculateAvergeHandVelocity() {
      Vector3 palmPosition = interactionHand.leapHand.PalmPosition.ToVector3();
      Velocity = (palmPosition - previousPalmPosition) / Time.deltaTime;
      if (velocityList.Count >= 3) {
        velocityList.Dequeue();
      }
      if (velocityList.Count < 3) {
        velocityList.Enqueue(Velocity);
      }
      averageVelocity = new Vector3(0, 0, 0);
      foreach (Vector3 v in velocityList) {
        averageVelocity += v;
      }
      averageVelocity = (averageVelocity / 3);
      previousPalmPosition = palmPosition;
    }

    //Grow behavior
    private IEnumerator SpawnBehavior(float duration) {
      Debug.Log("SpawnBehavior");
      IsSpawning = true;
      Vector3 startScale = Vector3.zero;
      Vector3 targetScale = Vector3.one;
      float elapsedTime = 0f;
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        float scaleX = Mathf.Lerp(startScale.x, targetScale.x, ScaleCurveX.Evaluate(elapsedTime / duration));
        float scaleY = Mathf.Lerp(startScale.y, targetScale.y, ScaleCurveY.Evaluate(elapsedTime / duration));
        float scaleZ = Mathf.Lerp(startScale.z, targetScale.z, ScaleCurveZ.Evaluate(elapsedTime / duration));
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        yield return null;
      }
      IsSpawning = false;
    }
    // launch 
    private IEnumerator UnspawnBehaviour() {

      AirplaneRigidbody.isKinematic = true;
      float duration = .3f;
      Vector3 startScale = transform.localScale;
      Vector3 targetScale = new Vector3(.001f, .001f, .001f);
      Vector3 startPosition = transform.position;
      float elapsedTime = 0f;
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, interactionHand.leapHand.GetPinchPosition(), elapsedTime / duration);
        transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
        yield return null;

      }
      OnLaunch(this);
      Destroy(gameObject);
    }
  }
}

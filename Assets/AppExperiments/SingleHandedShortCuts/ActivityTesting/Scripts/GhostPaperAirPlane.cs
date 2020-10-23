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

namespace LeapSingleHandedShortcuts
{
    public class GhostPaperAirPlane : MonoBehaviour
    {

        public Rigidbody AirplaneRigidbody;

        public float CurrentVelocity;

        private Vector3 previousPalmPosition;
        private Vector3 Velocity;
        //public Transform VelocityMarker;
        private Vector3 lastTrackedPosition;
        private Vector3 VelocitySnapShot;
        private Queue<Vector3> velocityList = new Queue<Vector3>();
        private Vector3 averageVelocity;

        public float AimAngleSpeed = 3f;
        public float AddTorqueMultiplier = 3f;
        public float AddForceMultiplier = .005f;

        public delegate void LaunchAction(PaperAirPlane airplance);
        public event LaunchAction OnLaunch;

        void Update()
        {
            Debug.Log("Ghost Update");
            Quaternion targetRotation = Quaternion.LookRotation(AirplaneRigidbody.velocity, transform.up) * Quaternion.Euler(new Vector3(-1f, 0f, 0f) * AirplaneRigidbody.velocity.magnitude);
            AirplaneRigidbody.rotation = Quaternion.Slerp(AirplaneRigidbody.rotation, targetRotation, AimAngleSpeed * AirplaneRigidbody.velocity.magnitude);
            AirplaneRigidbody.AddTorque(transform.right * AirplaneRigidbody.velocity.magnitude * AddTorqueMultiplier);
            AirplaneRigidbody.AddRelativeForce(Vector3.up * AirplaneRigidbody.velocity.magnitude * AddForceMultiplier);
        }
    }
}
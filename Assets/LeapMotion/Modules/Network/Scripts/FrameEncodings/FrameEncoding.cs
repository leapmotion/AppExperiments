using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace Leap.Unity.Networking {
  public enum FrameEncodingEnum {
    VectorHand,
    CurlHand
  };

  public abstract class FrameEncoding {
    public byte[] data;
    public uint frameSize;
    [NonSerialized]
    public Vector3 RPos;
    [NonSerialized]
    public Vector3 LPos;
    [NonSerialized]
    public Quaternion RRot;
    [NonSerialized]
    public Quaternion LRot;
    public abstract void fillEncoding(byte[] handData);
    public abstract void fillEncoding(Frame frame, Transform transform);
    public abstract void DecodeFrame(long frameID, Transform transform, Bone[] leftbones, Bone[] rightbones, List<Finger>[] fingers, Hand LeftHand, Hand RightHand);
    public abstract void lerp(FrameEncoding a, FrameEncoding b, float alpha);

    public byte floatToByte(float inFloat, float movementRange = 0.3f) {
      float clamped = Mathf.Clamp(inFloat, -movementRange / 2f, movementRange / 2f);
      clamped += movementRange / 2f;
      clamped /= movementRange;
      clamped *= 255f;
      clamped = Mathf.Floor(clamped);
      return (byte)clamped;
    }

    public float byteToFloat(byte inByte, float movementRange = 0.3f) {
      float clamped = (float)inByte;
      clamped /= 255f;
      clamped *= movementRange;
      clamped -= movementRange / 2f;
      return clamped;
    }

    public void fillHand(Hand toFill, long frameID, int id, float confidence, float grabStrength, float grabAngle, float pinchStrength, float pinchDistance, float palmWidth, bool isLeft, float timeVisible,/* Arm arm,*/ List<Finger> fingers, Vector palmPosition, Vector stabilizedPalmPosition, Vector palmVelocity, Vector palmNormal, LeapQuaternion rotation, Vector direction, Vector wristPosition) {
      toFill.FrameId = frameID;
      toFill.Id = id;
      toFill.Confidence = confidence;
      toFill.GrabStrength = grabStrength;
      toFill.GrabAngle = grabAngle;
      toFill.PinchStrength = pinchStrength;
      toFill.PinchDistance = pinchDistance;
      toFill.PalmWidth = palmWidth;
      toFill.IsLeft = isLeft;
      toFill.TimeVisible = timeVisible;
      //toFill.Arm = arm;
      toFill.Fingers = fingers;
      toFill.PalmPosition = palmPosition;
      toFill.StabilizedPalmPosition = stabilizedPalmPosition;
      toFill.PalmVelocity = palmVelocity;
      toFill.PalmNormal = palmNormal;
      toFill.Rotation = rotation;
      toFill.Direction = direction;
      toFill.WristPosition = wristPosition;
    }

    public void fillBone(Bone toFill, Vector prevJoint, Vector nextJoint, Vector center, Vector direction, float length, float width, Bone.BoneType type, LeapQuaternion rotation) {
      toFill.PrevJoint = prevJoint;
      toFill.NextJoint = nextJoint;
      toFill.Center = center;
      toFill.Direction = direction;
      toFill.Length = length;
      toFill.Width = width;
      toFill.Type = type;
      toFill.Rotation = rotation;
    }

    public void fillFinger(Finger toFill, long frameId, int handId, int fingerId, float timeVisible, Vector tipPosition, Vector tipVelocity, Vector direction, Vector stabilizedTipPosition, float width, float length, bool isExtended, Finger.FingerType type, Bone metacarpal, Bone proximal, Bone intermediate, Bone distal) {
      toFill.Id = handId;
      toFill.HandId = handId;
      toFill.TimeVisible = timeVisible;
      toFill.TipPosition = tipPosition;
      //toFill.TipVelocity = tipVelocity;                     // deprecated as of
      //toFill.StabilizedTipPosition = stabilizedTipPosition; // Core 4.4
      toFill.Direction = direction;
      toFill.Width = width;
      toFill.Length = length;
      toFill.IsExtended = isExtended;
      toFill.Type = type;
      toFill.bones[0] = metacarpal;
      toFill.bones[1] = proximal;
      toFill.bones[2] = intermediate;
      toFill.bones[3] = distal;
    }

    public void fillArm(Arm toFill, Vector elbow, Vector wrist, Vector center, Vector direction, float length, float width, LeapQuaternion rotation) {
      toFill.PrevJoint = elbow;
      toFill.NextJoint = wrist;
      toFill.Center = center;
      toFill.Direction = direction;
      toFill.Length = length;
      toFill.Width = width;
      toFill.Rotation = rotation;
    }
  }
}
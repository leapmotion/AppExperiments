using System;
using UnityEngine;
using System.Collections.Generic;
//using BinaryRecordings;

namespace Leap.Unity.Networking {
  [Serializable]
  public class VectorFrameEncoding : FrameEncoding {//, ISerializableObject {
    private byte[] LBones;
    private byte[] RBones;

    #region Constructors
    public VectorFrameEncoding() {
      data = new byte[170];
      frameSize = (uint)data.Length;
      LPos = Vector3.zero;
      RPos = Vector3.zero;
      LRot = Quaternion.identity;
      RRot = Quaternion.identity;
      LBones = new byte[75];
      RBones = new byte[75];
      for (int i = 0; i < LBones.Length; i++) {
        LBones[i] = 0;
        RBones[i] = 0;
      }
    }
    #endregion

    #region Encoding Functions
    //Encodes the values for a single hand into an intermediate interpolable representation
    void EncodeHandValues(Hand inHand, out bool isLeft, out Vector3 pos, out Quaternion rot, out byte[] bones) {
      isLeft = inHand.IsLeft;
      pos = inHand.PalmPosition.ToVector3();
      rot = inHand.Rotation.ToQuaternion();
      bones = isLeft ? LBones : RBones;
      int index = 0;
      for (int i = 0; i < 5; i++) {
        Vector3 baseMetacarpal = ToLocal(inHand.Fingers[i].bones[0].PrevJoint.ToVector3(), pos, rot);
        bones[index] = floatToByte(baseMetacarpal.x);
        bones[index + 1] = floatToByte(baseMetacarpal.y);
        bones[index + 2] = floatToByte(baseMetacarpal.z);
        for (int j = 0; j < 4; j++) {
          Vector3 joint = ToLocal(inHand.Fingers[i].bones[j].NextJoint.ToVector3(), pos, rot);
          bones[index + 3 + (j * 3)] = floatToByte(joint.x);
          bones[index + 4 + (j * 3)] = floatToByte(joint.y);
          bones[index + 5 + (j * 3)] = floatToByte(joint.z);
        }
        index += 15;
      }
    }

    //Encodes a Raw Tracking Frame into an (interpolable) intermediate representation and a byte array
    public override void fillEncoding(Frame frame, Transform transform) {
      LPos = Vector3.zero; RPos = Vector3.zero; LRot = Quaternion.identity; RRot = Quaternion.identity;
      bool isLeft = false; Vector3 pos; Quaternion rot; byte[] Bones;
      for (int i = 0; i < 2; i++) {
        if (i < frame.Hands.Count) {
          EncodeHandValues(frame.Hands[i], out isLeft, out pos, out rot, out Bones);
          if (isLeft) {
            LPos = Quaternion.Inverse(transform.rotation) * (pos - transform.position);
            LRot = Quaternion.Inverse(transform.rotation) * rot;
            LBones = Bones;
          } else {
            RPos = Quaternion.Inverse(transform.rotation) * (pos - transform.position);
            RRot = Quaternion.Inverse(transform.rotation) * rot;
            RBones = Bones;
          }
        }
      }
      if (LBones == null) {
        for (int i = 0; i < LBones.Length; i++) {
          LBones[i] = 0;
        }
        LPos = Vector3.zero;
        LRot = Quaternion.identity;
      }
      if (RBones == null) {
        for (int i = 0; i < RBones.Length; i++) {
          RBones[i] = 0;
        }
        RPos = Vector3.zero;
        RRot = Quaternion.identity;
      }

      fillByteArray();
    }

    //Constructs an interpolable representation from a byte array
    public override void fillEncoding(byte[] handData) {
      if (handData == null || handData.Length != data.Length) { handData = new byte[data.Length]; }
      data = handData;
      int index = 0;

      //Left Hand
      LPos = new Vector3((BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f);

      LRot = Utils.DecompressBytesToQuat(data, ref index);

      for (int i = 0; i < 75;) {
        LBones[i++] = data[index++];
      }

      //Right Hand
      RPos = new Vector3((BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f);

      RRot = Utils.DecompressBytesToQuat(data, ref index);

      for (int i = 0; i < 75;) {
        RBones[i++] = data[index++];
      }
    }

    //Fills a 170-byte array, representing the pose and position of both hands
    void fillByteArray(byte[] handData) {
      if (handData == null || handData.Length != data.Length) { handData = new byte[data.Length]; }
      int index = 0;

      //Left Hand
      for (int i = 0; i < 3; i++) {
        BitConverterNonAlloc.GetBytes(Convert.ToInt16(LPos[i] * 4096f), handData, ref index);
      }

      Utils.CompressQuatToBytes(LRot, handData, ref index);

      for (int i = 0; i < 75;) {
        handData[index++] = LBones[i++];
      }

      //Right Hand
      for (int i = 0; i < 3; i++) {
        BitConverterNonAlloc.GetBytes(Convert.ToInt16(RPos[i] * 4096f), handData, ref index);
      }

      Utils.CompressQuatToBytes(RRot, handData, ref index);

      for (int i = 0; i < 75;) {
        handData[index++] = RBones[i++];
      }
    }

    //Fills data with a 170-byte array, representing the pose and position of both hands
    public void fillByteArray() {
      fillByteArray(data);
    }
    #endregion

    #region Decoding Functions
    //Uses the interpolable data to reconstruct and fill a hand object
    void DecodeHand(long frameID, List<Finger>[] fingers, bool c, Vector3 p, Quaternion r, Bone[] bones, byte[] fingerBones, Hand outHand) {
      for (int i = 0; i < 5; i++) {
        Vector3 PrevJoint = Vector3.zero;
        Vector3 NextJoint = Vector3.zero;
        Quaternion boneRot = Quaternion.identity;
        for (int j = 0; j < 4; j++) {
          PrevJoint = new Vector3(byteToFloat(fingerBones[(((i * 5) + j) * 3)]), byteToFloat(fingerBones[(((i * 5) + j) * 3) + 1]), byteToFloat(fingerBones[(((i * 5) + j) * 3) + 2]));
          NextJoint = new Vector3(byteToFloat(fingerBones[(((i * 5) + j + 1) * 3)]), byteToFloat(fingerBones[(((i * 5) + j + 1) * 3) + 1]), byteToFloat(fingerBones[(((i * 5) + j + 1) * 3) + 2]));
          //Calculate Bone Rotations from offsets
          if (PrevJoint != NextJoint) {
            if (i != 0) {
              boneRot = Quaternion.LookRotation((NextJoint - PrevJoint).normalized, Vector3.Cross((NextJoint - PrevJoint).normalized, Vector3.right).normalized);
            } else {
              Vector3 downward = Vector3.Cross((NextJoint - PrevJoint).normalized, Vector3.right).normalized;
              boneRot = Quaternion.LookRotation((NextJoint - PrevJoint).normalized, Vector3.Cross((NextJoint - PrevJoint).normalized, (c ? -downward : downward)).normalized);
            }
          }
          fillBone(bones[(i * 4) + j], ToWorld(PrevJoint, p, r).ToVector(), ToWorld(NextJoint, p, r).ToVector(), (ToWorld(NextJoint + PrevJoint, p, r) / 2f).ToVector(), p.ToVector(), 1f, 1f, (Bone.BoneType)j, (r * boneRot).ToLeapQuaternion());
        }
        fillFinger(fingers[c ? 0 : 1][i], frameID, (c ? 0 : 1), i, Time.time, ToWorld(NextJoint, p, r).ToVector(), Vector.Zero, (boneRot * Vector3.forward).ToVector(), ToWorld(NextJoint, p, r).ToVector(), 1f, 1f, true, (Finger.FingerType)i, bones[(i * 4) + 0], bones[(i * 4) + 1], bones[(i * 4) + 2], bones[(i * 4) + 3]);
      }

      fillArm(outHand.Arm, ToWorld(new Vector3(0f, 0f, -0.3f), p, r).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), p, r).ToVector(), ToWorld(new Vector3(0f, 0f, -0.125f), p, r).ToVector(), Vector.Zero, 0.3f, 0.05f, Quaternion.identity.ToLeapQuaternion());
      fillHand(outHand, frameID, (c ? 0 : 1), 1f, 0.5f, 100f, 0.5f, 50f, 100f, c, 1f, fingers[c ? 0 : 1], p.ToVector(), p.ToVector(), Vector3.zero.ToVector(), (r * Vector3.up).ToVector(), r.ToLeapQuaternion(), (r * Vector3.forward).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), p, r).ToVector());
    }

    //Uses the interpolable data to reconstruct and fill a frame object
    public override void DecodeFrame(long frameID, Transform transform, Bone[] leftbones, Bone[] rightbones, List<Finger>[] fingers, Hand LeftHand, Hand RightHand) {
      int index = 0;
      if (LPos != Vector3.zero) {
        DecodeHand(frameID, fingers, true, (transform.rotation * LPos) + transform.position, transform.rotation * LRot, leftbones, LBones, LeftHand);
        index++;
      } else {
        LeftHand.PalmPosition = Vector.Zero;
      }
      if (RPos != Vector3.zero) {
        DecodeHand(frameID, fingers, false, (transform.rotation * RPos) + transform.position, transform.rotation * RRot, rightbones, RBones, RightHand);
      } else {
        RightHand.PalmPosition = Vector.Zero;
      }
    }
    #endregion

    #region Utility Functions
    public override void lerp(FrameEncoding a, FrameEncoding b, float alpha) {
      VectorFrameEncoding start = a as VectorFrameEncoding;
      VectorFrameEncoding finish = b as VectorFrameEncoding;
      if (a != null && b != null) {
        if (!(start.LPos == Vector3.zero || finish.LPos == Vector3.zero)) { LPos = Vector3.LerpUnclamped(start.LPos, finish.LPos, alpha); } else { LPos = Vector3.zero; }
        if (!(start.RPos == Vector3.zero || finish.RPos == Vector3.zero)) { RPos = Vector3.LerpUnclamped(start.RPos, finish.RPos, alpha); } else { RPos = Vector3.zero; }
        LRot = Quaternion.SlerpUnclamped(start.LRot, finish.LRot, alpha);
        RRot = Quaternion.SlerpUnclamped(start.RRot, finish.RRot, alpha);
        for (int i = 0; i < LBones.Length; i++) {
          LBones[i] = (byte)((float)start.LBones[i] + alpha * ((float)finish.LBones[i] - (float)start.LBones[i]));
          RBones[i] = (byte)((float)start.RBones[i] + alpha * ((float)finish.RBones[i] - (float)start.RBones[i]));
        }
      }
    }

    public static Vector3 ToWorld(Vector3 point, Vector3 Pos, Quaternion Rot) {
      return (Rot * point) + Pos;
    }

    public static Vector3 ToLocal(Vector3 point, Vector3 Pos, Quaternion Rot) {
      return Quaternion.Inverse(Rot) * (point - Pos);
    }
    /*
    public void OnSerialize(BinaryWriter writer) {
      writer.Write(LPos);
      writer.Write(LRot);
      writer.Write(LBones);
      writer.Write(RPos);
      writer.Write(RRot);
      writer.Write(RBones);
    }

    public void OnDeserialize(BinaryReader reader) {
      LPos = reader.ReadVector3();
      LRot = reader.ReadQuaternion();
      reader.ReadBytes(LBones);
      RPos = reader.ReadVector3();
      RRot = reader.ReadQuaternion();
      reader.ReadBytes(RBones);
      //fillByteArray(); //Call before sending deserialized frame over the network
    }*/
    #endregion
  }
}
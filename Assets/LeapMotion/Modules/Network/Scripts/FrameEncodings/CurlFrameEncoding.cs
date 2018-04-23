using System;
using UnityEngine;
using System.Collections.Generic;

namespace Leap.Unity.Networking {
  [System.Serializable]
  public class CurlFrameEncoding : FrameEncoding {
    private byte[] LCurl;
    private byte[] RCurl;

    #region Constructors
    public CurlFrameEncoding() {
      data = new byte[32];
      frameSize = (uint)data.Length;
      LPos = Vector3.zero;
      RPos = Vector3.zero;
      LRot = Quaternion.identity;
      RRot = Quaternion.identity;
      LCurl = new byte[6];
      RCurl = new byte[6];
      for (int i = 0; i < LCurl.Length; i++) {
        LCurl[i] = 0;
        RCurl[i] = 0;
      }
    }
    #endregion

    #region Encoding Functions
    //Encodes the values for a single hand into an intermediate interpolable representation
    void EncodeHandValues(Hand inHand, out bool isLeft, out Vector3 pos, out Quaternion rot, out byte[] curl) {
      isLeft = inHand.IsLeft;
      pos = inHand.PalmPosition.ToVector3();
      rot = inHand.Rotation.ToQuaternion();
      curl = isLeft ? LCurl : RCurl;//new byte[6];
      curl[0] = (byte)(Mathf.Clamp01(((Vector3.Dot(inHand.Basis.xBasis.ToVector3() * (isLeft ? -1f : 1f), inHand.Fingers[0].Direction.ToVector3()) + 1f) / 2f) - 0.1f) * 255f);
      float spread = -0.5f;
      for (int i = 1; i < 5; i++) {
        curl[i] = (byte)(Mathf.Clamp01(1f - ((Vector3.Dot(inHand.Direction.ToVector3(), inHand.Fingers[i].Direction.ToVector3()) + 1f) / 2f)) * 255f);
        spread += Mathf.Abs((Quaternion.Inverse(inHand.Rotation.ToQuaternion()) * inHand.Fingers[i].Direction.ToVector3()).x);
      }
      curl[5] = (byte)(Mathf.Clamp01(spread) * 255f);
    }

    //Encodes a Raw Tracking Frame into an (interpolable) intermediate representation and a byte array
    public override void fillEncoding(Frame frame, Transform transform) {
      LPos = Vector3.zero; RPos = Vector3.zero; LRot = Quaternion.identity; RRot = Quaternion.identity; LCurl = new byte[6]; RCurl = new byte[6];
      for (int i = 0; i < 6; i++) {
        LCurl[i] = (byte)128;
        RCurl[i] = (byte)128;
      }
      bool isLeft = false; Vector3 pos; Quaternion rot; byte[] curl = new byte[6];
      for (int i = 0; i < 2; i++) {
        if (i < frame.Hands.Count) {
          EncodeHandValues(frame.Hands[i], out isLeft, out pos, out rot, out curl);
        } else {
          isLeft = !isLeft;
          pos = Vector3.zero;
          rot = Quaternion.identity;
          curl = new byte[6];
          for (int j = 0; j < 6; j++) {
            curl[j] = (byte)128;
          }
        }

        if (isLeft) {
          LPos = Quaternion.Inverse(transform.rotation) * (pos - transform.position);
          LRot = Quaternion.Inverse(transform.rotation) * rot;
          LCurl = curl;
        } else {
          RPos = Quaternion.Inverse(transform.rotation) * (pos - transform.position);
          RRot = Quaternion.Inverse(transform.rotation) * rot;
          RCurl = curl;
        }
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

      for (int i = 0; i < 6;) {
        LCurl[i++] = data[index++];
      }

      //Right Hand
      RPos = new Vector3((BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f,
                         (BitConverterNonAlloc.ToInt16(data, ref index)) / 4096f);

      RRot = Utils.DecompressBytesToQuat(data, ref index);

      for (int i = 0; i < 6;) {
        RCurl[i++] = data[index++];
      }
    }

    //Fills a 32-byte array, representing the pose and position of both hands
    void fillByteArray(byte[] handData) {
      if (handData == null || handData.Length != data.Length) { handData = new byte[data.Length]; }
      int index = 0;

      //Left Hand
      for (int i = 0; i < 3; i++) {
        BitConverterNonAlloc.GetBytes(Convert.ToInt16(LPos[i] * 4096f), handData, ref index);
      }

      Utils.CompressQuatToBytes(LRot, handData, ref index);

      for (int i = 0; i < 6;) {
        handData[index++] = LCurl[i++];
      }

      //Right Hand
      for (int i = 0; i < 3; i++) {
        BitConverterNonAlloc.GetBytes(Convert.ToInt16(RPos[i] * 4096f), handData, ref index);
      }

      Utils.CompressQuatToBytes(RRot, handData, ref index);

      for (int i = 0; i < 6;) {
        handData[index++] = RCurl[i++];
      }
    }

    //Fills data with a 32-byte array, representing the pose and position of both hands
    public void fillByteArray() {
      fillByteArray(data);
    }
    #endregion

    #region Decoding Functions
    //Uses the interpolable data to reconstruct and fill a hand object
    void DecodeHand(long frameID, List<Finger>[] fingers, bool c, Vector3 p, Quaternion r, Bone[] bones, byte[] fingerCurls, Hand outHand) {
      for (int i = 0; i < 5; i++) {
        Vector3 PrevJoint;
        Vector3 NextJoint = Vector3.zero;
        //Finger Spread
        Quaternion boneRot = Quaternion.identity;
        for (int j = 0; j < 4; j++) {
          if (j == 0 && i > 0) {
            //4 Top Knuckles
            NextJoint = new Vector3(-i * 0.021f + 0.043f + (i == 4 ? 0.005f : 0f), (i > 1 ? 0.01f : 0f), 0.02f - (i > 2 ? 0.007f : 0f));
            PrevJoint = new Vector3(-i * 0.015f + 0.04f, -0.015f, -0.05f);
            boneRot = Quaternion.Euler(0f, (i == 0 ? 75 : ((i * -7f + 15f))), 0f);
          } else if (i == 0 && j == 0) {
            //Thumb "Knuckle"
            NextJoint = new Vector3(0.02f, -0.015f, -0.05f);
            PrevJoint = new Vector3(0.01f, -0.015f, -0.055f);
            boneRot = Quaternion.Euler(30f, 50, -90f);
          } else {
            //Main Fingers
            //Finger Curl
            if (j == 1 && i > 0) {
              boneRot = Quaternion.Euler((i == 0 ? 60f : 70f) * ((float)fingerCurls[i]) / 256f, j == 1 ? (i == 0 ? 75 : ((i * -7f + 15f) * (((float)fingerCurls[5]) / 256f) * 3f)) : 0f, 0f);
            } else {
              boneRot *= Quaternion.Euler((i == 0 ? 60f : 70f) * ((float)fingerCurls[i]) / 256f, 0f, 0f);
            }
            PrevJoint = NextJoint;
            NextJoint = NextJoint + (boneRot * new Vector3(0.0f, 0f, (i == 0 ? 0.055f : 0.045f) / (j)));
          }
          //Fix for Rigged Hands
          Quaternion meshRot = boneRot;
          meshRot = Quaternion.Euler(boneRot.eulerAngles.x, (c ? 1f : -1f) * boneRot.eulerAngles.y, (!c && (i == 0) ? -1f : 1f) * boneRot.eulerAngles.z);
          fillBone(bones[(i * 4) + j], ToWorld(PrevJoint, p, r, c).ToVector(), ToWorld(NextJoint, p, r, c).ToVector(), (ToWorld(NextJoint + PrevJoint, p, r, c) / 2f).ToVector(), p.ToVector(), 1f, 1f, (Bone.BoneType)j, (r * meshRot).ToLeapQuaternion());
        }
        fillFinger(fingers[c ? 0 : 1][i], frameID, (c ? 0 : 1), i, Time.time, ToWorld(NextJoint, p, r, c).ToVector(), Vector.Zero, (boneRot * Vector3.forward).ToVector(), ToWorld(NextJoint, p, r, c).ToVector(), 1f, 1f, true, (Finger.FingerType)i, bones[(i * 4) + 0], bones[(i * 4) + 1], bones[(i * 4) + 2], bones[(i * 4) + 3]);
      }

      Vector3 palm = p;

      fillArm(outHand.Arm, ToWorld(new Vector3(0f, 0f, -0.3f), p, r, c).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), p, r, c).ToVector(), ToWorld(new Vector3(0f, 0f, -0.125f), p, r, c).ToVector(), Vector.Zero, 0.3f, 0.05f, Quaternion.identity.ToLeapQuaternion());
      fillHand(outHand, frameID, (c ? 0 : 1), 1f, 0.5f, 100f, 0.5f, 50f, 100f, c, 1f, fingers[c ? 0 : 1], palm.ToVector(), palm.ToVector(), Vector3.zero.ToVector(), (r * Vector3.up).ToVector(), r.ToLeapQuaternion(), (r * Vector3.forward).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), p, r, c).ToVector());
    }

    //Uses the interpolable data to reconstruct and fill a frame object
    public override void DecodeFrame(long frameID, Transform transform, Bone[] leftbones, Bone[] rightbones, List<Finger>[] fingers, Hand LeftHand, Hand RightHand) {
      int index = 0;
      if (LPos != Vector3.zero) {
        DecodeHand(frameID, fingers, true, (transform.rotation * LPos) + transform.position, transform.rotation * LRot, leftbones, LCurl, LeftHand);
        index++;
      } else {
        LeftHand.PalmPosition = Vector.Zero;
      }
      if (RPos != Vector3.zero) {
        DecodeHand(frameID, fingers, false, (transform.rotation * RPos) + transform.position, transform.rotation * RRot, rightbones, RCurl, RightHand);
      } else {
        RightHand.PalmPosition = Vector.Zero;
      }
    }
    #endregion

    #region Utility Functions
    public CurlFrameEncoding Copy() {
      CurlFrameEncoding state = new CurlFrameEncoding();
      state.LPos.Set(LPos.x, LPos.y, LPos.z);
      state.RPos.Set(RPos.x, RPos.y, RPos.z);
      state.LRot.Set(LRot.x, LRot.y, LRot.z, LRot.w);
      state.RRot.Set(RRot.x, RRot.y, RRot.z, RRot.w);
      state.LCurl = new byte[6];
      state.RCurl = new byte[6];
      for (int i = 0; i < 6; i++) {
        state.LCurl[i] = LCurl[i];
        state.RCurl[i] = RCurl[i];
      }
      return state;
    }

    public override void lerp(FrameEncoding a, FrameEncoding b, float alpha) {
      CurlFrameEncoding start = a as CurlFrameEncoding;
      CurlFrameEncoding finish = b as CurlFrameEncoding;
      if (a != null && b != null) {
        if (!(start.LPos == Vector3.zero || finish.LPos == Vector3.zero)) { LPos = Vector3.LerpUnclamped(start.LPos, finish.LPos, alpha); } else { LPos = Vector3.zero; }
        if (!(start.RPos == Vector3.zero || finish.RPos == Vector3.zero)) { RPos = Vector3.LerpUnclamped(start.RPos, finish.RPos, alpha); } else { RPos = Vector3.zero; }
        LRot = Quaternion.SlerpUnclamped(start.LRot, finish.LRot, alpha);
        RRot = Quaternion.SlerpUnclamped(start.RRot, finish.RRot, alpha);
        for (int i = 0; i < LCurl.Length; i++) {
          LCurl[i] = (byte)((float)start.LCurl[i] + alpha * ((float)finish.LCurl[i] - (float)start.LCurl[i]));
          RCurl[i] = (byte)((float)start.RCurl[i] + alpha * ((float)finish.RCurl[i] - (float)start.RCurl[i]));
        }
      }
    }

    private static Vector3 ToWorld(Vector3 point, Vector3 TempPos, Quaternion TempRot, bool TempChirality) {
      return (TempRot * new Vector3(point.x * (TempChirality ? 1f : -1f), point.y, point.z)) + TempPos;
    }
    #endregion
  }
}
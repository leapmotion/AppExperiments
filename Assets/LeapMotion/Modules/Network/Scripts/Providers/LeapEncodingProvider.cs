using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace Leap.Unity.Networking {
  public class LeapEncodingProvider : LeapProvider {
    public FrameEncoding currentState;

    protected Frame currentFrame;
    protected long frameID = 100;

    protected int VisibleHands = 0;
    protected int prevVisibleHands = 0;

    protected List<Finger>[] fingers = new List<Finger>[5];

    protected Hand LeftHand = new Hand();
    protected Hand RightHand = new Hand();

    protected List<Hand> twohandlist = new List<Hand>(2);
    protected List<Hand> onehandlist = new List<Hand>(1);
    protected List<Hand> nohandlist = new List<Hand>(0);
    protected Bone[] leftbones = new Bone[20];
    protected Bone[] rightbones = new Bone[20];

    public virtual void Start() {
      currentFrame = new Frame();
      long Timestamp = (long)(Time.time * 1e6);
      //InteractionBox box = new InteractionBox(new Vector(0f, 0f, 0f), new Vector(100f, 100f, 100f));
      currentFrame.Id = frameID;
      currentFrame.CurrentFramesPerSecond = 110f;
      //currentFrame.InteractionBox = box; // removed as of Core 4.4
      currentFrame.Timestamp = Timestamp;

      for (int i = 0; i < fingers.Length; i++) {
        if (fingers[i] != null) {
          fingers[i].Clear();
        } else {
          fingers[i] = new List<Finger>();
        }
        for (int j = 0; j < 5; j++) {
          fingers[i].Add(new Finger());
        }
      }

      for (int i = 0; i < leftbones.Length; i++) {
        leftbones[i] = new Bone();
        rightbones[i] = new Bone();
      }

      twohandlist.Add(new Hand());
      twohandlist.Add(new Hand());
      onehandlist.Add(new Hand());
    }

    public virtual void AddFrameState(FrameEncoding state) {
      currentState = state;
    }

    public void fillCurrentFrame(FrameEncoding state) {
      frameID++;

      //Fill the Bone Lists, Finger Lists, and Hand Objects
      state.DecodeFrame(frameID, transform, leftbones, rightbones, fingers, LeftHand, RightHand);

      VisibleHands = 0;
      if (state.LPos != Vector3.zero) {
        VisibleHands++;
      }
      if (state.RPos != Vector3.zero) {
        VisibleHands++;
      }

      //Use different lists depending on the number of hands (zero-allocation trick)
      if (VisibleHands == 1) {
        onehandlist[0] = (LeftHand.PalmPosition == Vector.Zero) ? RightHand : LeftHand;
        currentFrame.Hands = onehandlist;
      } else if (VisibleHands == 2) {
        twohandlist[0] = LeftHand;
        twohandlist[1] = RightHand;
        currentFrame.Hands = twohandlist;
      } else {
        currentFrame.Hands = nohandlist;
      }
      prevVisibleHands = VisibleHands;
    }

    public virtual void Update() {
      if (currentState != null) {
        fillCurrentFrame(currentState);
      }

      DispatchUpdateFrameEvent(CurrentFrame);
    }

    public override Frame CurrentFrame {
      get {
        return currentFrame;
      }
    }

    //public override Image CurrentImage {
    //  get {
    //    return new Image();
    //  }
    //}

    public override Frame CurrentFixedFrame {
      get {
        return CurrentFrame;
      }
    }
  }
}
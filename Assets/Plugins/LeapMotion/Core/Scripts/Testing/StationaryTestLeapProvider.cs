/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

namespace Leap.Unity {

  public class StationaryTestLeapProvider : LeapProvider {

    private Frame _curFrame;

    private Hand _leftHand;
    private Hand _rightHand;

    public override Frame CurrentFrame {
      get {
        return _curFrame;
      }
    }

    public override Frame CurrentFixedFrame {
      get {
        return _curFrame;
      }
    }

    void Awake() {
      refreshFrame();
    }

    private void refreshFrame() {
      _curFrame = new Frame();

      _leftHand = this.MakeTestHand(true);
      _rightHand = this.MakeTestHand(false);

      _curFrame.Hands.Add(_leftHand);
      _curFrame.Hands.Add(_rightHand);
    }

    void Update() {
      refreshFrame();

      DispatchUpdateFrameEvent(_curFrame);
    }

    void FixedUpdate() {
      DispatchFixedFrameEvent(_curFrame);
    }

  }

}

using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6.Drawing {

  public class PoseToStrokePointStreamConverter : MonoBehaviour,
                            IStreamReceiver<Pose>,
                            IStream<StrokePoint> {

    #region Inspector / Settings

    [Header("Brush Settings")]

    public int maxStrokePointsPerObject = 256;

    [SerializeField]
    [Range(0.01f, 0.05f)]
    public float size = 0.025f;

    [SerializeField]
    public Color color = Color.white;

    #endregion

    #region IStreamReceiver<Pose>

    public void Open() {
      OnOpen();
    }

    public void Receive(Pose data) {
      OnSend(new StrokePoint() {
        pose = data,
        color = color,
        radius = size,
        temp_refFrame = Matrix4x4.identity
      });
    }

    public void Close() {
      OnClose();
    }

    #endregion

    #region IStream<StrokePoint>

    public event Action OnOpen = () => { };
    public event Action<StrokePoint> OnSend = (data) => { };
    public event Action OnClose = () => { };

    #endregion

  }

}
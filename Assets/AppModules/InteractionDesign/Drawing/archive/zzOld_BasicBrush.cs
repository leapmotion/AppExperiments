using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class zzOld_BasicBrush : MonoBehaviour, zzOld_IBrush {

    public const float MIN_BRUSH_DISTANCE = 0.01f;
    //public const float MAX_ANGLE_PER_CM = 5f;

    #region Inspector

    [Header("Brush Settings")]

    [SerializeField]
    [Range(0.01f, 0.05f)]
    private float _size = 0.025f;

    [SerializeField]
    private Color _color = Color.white;

    [Header("Stroke Generator")]

    [SerializeField]
    [ImplementsInterface(typeof(IStreamReceiver<StrokePoint>))]
    private MonoBehaviour _strokeGenerator;
    public IStreamReceiver<StrokePoint> strokeGenerator {
      get { return _strokeGenerator as IStreamReceiver<StrokePoint>; }
      set { _strokeGenerator = value as MonoBehaviour; }
    }

    #endregion

    #region MonoBehaviour Events

    private bool _shouldUpdateBrush = false;
    private Maybe<Pose> _maybeLastPose = Maybe.None;

    private void Update() {
      if (_didBrushingBegin) {
        _shouldUpdateBrush = true;
        _maybeLastPose = Maybe.None;

        strokeGenerator.Open();

        _didBrushingBegin = false;
      }

      if (_shouldUpdateBrush) {
        var targetPose = pose;

        var effPosition = targetPose.position;
        //var effRotation = targetPose.rotation;

        if (_maybeLastPose.hasValue) {
          if (Vector3.Distance(_maybeLastPose.valueOrDefault.position,
                               effPosition) > MIN_BRUSH_DISTANCE) {
            strokeGenerator.Receive(new StrokePoint() {
              pose = pose,
              color = color,
              radius = size,
              temp_refFrame = Matrix4x4.identity
            });
            _maybeLastPose = pose;
          }
        }
        else {
          strokeGenerator.Receive(new StrokePoint() {
            pose = pose,
            color = color,
            radius = size,
            temp_refFrame = Matrix4x4.identity
          });
          _maybeLastPose = pose;
        }

      }

      if (_didBrushingEnd) {
        _shouldUpdateBrush = false;
        _maybeLastPose = Maybe.None;

        strokeGenerator.Close();

        _didBrushingEnd = false;
      }

    }

    #endregion

    #region IBrush

    private bool _isBrushing = false;
    private bool _didBrushingBegin = false;
    private bool _didBrushingEnd = false;

    public bool isBrushing {
      get { return _isBrushing; }
    }

    public Pose pose { get { return this.transform.ToPose(); } }

    public Color color { get { return _color; } set { _color = value; } }

    public float size { get { return _size; } set { _size = value; } }

    public void Begin() {
      _isBrushing = true;

      _didBrushingBegin = true;
    }

    public void End() {
      _isBrushing = false;

      _didBrushingEnd = true;
    }

    public void Move(Pose newPose) {
      this.transform.SetWorldPose(newPose);
    }

    #endregion

  }

}
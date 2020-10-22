using Leap.Unity.Attributes;
using Leap.Unity.Apps.Paint6.Drawing;
using Leap.Unity.Infix;
using Leap.Unity.Meshing;
using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class InkwellMeshGenerator : MonoBehaviour, IRuntimeGizmoComponent {

    #region Inspector

    public bool autoDetectPinchAmount = true;

    [DisableIf("autoDetectPinchAmount", isEqualTo: false)]
    public LeapProvider provider;
    [DisableIf("autoDetectPinchAmount", isEqualTo: false)]
    public Chirality whichHand;

    [DisableIf("autoDetectPinchAmount", isEqualTo: true)]
    public float pinchAmount = 0f;
    public void SetPinchAmount(float amount) { pinchAmount = amount; }

    List<Vector3> thumbPoints = new List<Vector3>(64);
    List<Vector3> indexPoints = new List<Vector3>(64);

    [Header("Ucon Channel Input")]
    public ColorChannel colorChannelIn = new ColorChannel("brush/color");

    [Header("Mesh Generation")]

    [SerializeField]
    [ImplementsInterface(typeof(IPolyMesher<StrokeObject>))]
    private MonoBehaviour _strokePolyMesher = default;
    public IPolyMesher<StrokeObject> strokePolyMesher {
      get { return _strokePolyMesher as IPolyMesher<StrokeObject>; }
    }

    public PolyMeshObject fillPolyMeshObject;

    [Header("Debug")]
    public bool drawDebug = false;

    #endregion

    #region Unity Events

    private void Reset() {
      if (provider == null) provider = Hands.Provider;
    }

    private void OnEnable() {
      provider.OnUpdateFrame -= onUpdateFrame;
      provider.OnUpdateFrame += onUpdateFrame;
    }

    private void OnDisable() {
      provider.OnUpdateFrame -= onUpdateFrame;
    }

    private void Update() {
      updateMeshRepresentation();
    }

    #endregion

    #region Leap Frame Callback

    private void onUpdateFrame(Frame frame) {
      var hand = frame.Get(whichHand);

      thumbPoints.Clear();
      indexPoints.Clear();

      if (hand != null) {
        var index = hand.GetIndex(); var thumb = hand.GetThumb();

        //var indexLen = index.Length;
        // Exclude metacarpal for thumb length.
        //var thumbLen = thumb.bones[2].Length + thumb.bones[3].Length;
        //var indexLenOverThumbLen = indexLen / thumbLen;

        if (autoDetectPinchAmount) {
          var pinchStrength = Gestures.PinchGesture.Static_GetCustomPinchStrength(hand)
                                                   .Clamped01();
          pinchAmount = pinchStrength;
        }

        var progress = pinchAmount.Map(0f, 1f, -0.2f, 1f);
        var positiveDir = -hand.PalmarAxis();
        for (float p = -0.2f; p <= progress; p += 0.015f) {
          Vector3 thumbPos, indexPos;

          // Thumb position evaluation
          var progressAlongThumb = p;
          if (progressAlongThumb < 0f) {
            var thumbMC = thumb.bones[1];
            thumbPos = Vector3.Lerp(thumbMC.PrevJoint.ToVector3(),
                                    thumbMC.NextJoint.ToVector3(),
                                    progressAlongThumb.Map(-0.5f, 0f, 0f, 1f));
          }
          else {
            Bone thumbBone;
            if (progressAlongThumb < 0.5f) {
              thumbBone = thumb.bones[2];
            }
            else {
              progressAlongThumb -= 0.5f;
              thumbBone = thumb.bones[3];
            }
            thumbPos = Vector3.Lerp(thumbBone.PrevJoint.ToVector3(),
                                    thumbBone.NextJoint.ToVector3(),
                                    progressAlongThumb.Map(0f, 0.5f, 0f, 1f));
          }

          // Non-thumb position evaluation
          float progressAlongFinger = p;
          Vector3 fingerPos;
          var finger = index;
          if (progressAlongFinger < 0f) {
            var fingerMC = finger.bones[0];
            fingerPos = Vector3.Lerp(fingerMC.PrevJoint.ToVector3(),
                                     fingerMC.NextJoint.ToVector3(),
                                     progressAlongFinger.Map(-0.5f, 0f, 0f, 1f));
          }
          else {
            Bone fingerBone;
            if (progressAlongFinger < 0.33f) {
              fingerBone = finger.bones[1];
            }
            else if (progressAlongFinger < 0.66f) {
              progressAlongFinger -= 0.33f;
              fingerBone = finger.bones[2];
            }
            else {
              progressAlongFinger -= 0.66f;
              fingerBone = finger.bones[3];
            }
            fingerPos = Vector3.Lerp(fingerBone.PrevJoint.ToVector3(),
                                      fingerBone.NextJoint.ToVector3(),
                                      progressAlongFinger.Map(0f, 0.33f, 0f, 1f));
          }

          indexPos = fingerPos;

          // Filter out any over-pinch.
          var isOverPinched = (indexPos - thumbPos).Dot(positiveDir) < 0f;
          if (!isOverPinched) {
            thumbPoints.Add(thumbPos);
            indexPoints.Add(indexPos);
          }

        }
      }
    }

    #endregion

    #region Mesh Generation

    private StrokeObject _strokeObj;

    private void updateMeshRepresentation() {

      // Index + Thumb points -> StrokePoints in a StrokeObject.
      if (_strokeObj == null) {
        _strokeObj = gameObject.AddComponent<StrokeObject>();
      }

      // Get color via Ucon wiring.
      var brushColor = colorChannelIn.Get();

      _strokeObj.Clear();
      for (int i = 1; i < indexPoints.Count; i++) {
        var avgPos = (indexPoints[i] + thumbPoints[i]) * 0.5f;
        var prevAvgPos = (indexPoints[i - 1] + thumbPoints[i - 1]) * 0.5f;

        var right = (indexPoints[i] - thumbPoints[i]).normalized;
        var forward = (avgPos - prevAvgPos).normalized;
        if ((avgPos - prevAvgPos).sqrMagnitude < 1e-7f) continue;
        var up = Vector3.Cross(right, forward).normalized;
        var rot = Quaternion.LookRotation(forward, up);
        
        var radius = (indexPoints[i] - thumbPoints[i]).magnitude * 0.3f;

        var strokePoint = new StrokePoint() {
          pose = new Pose(avgPos, rot),
          color = brushColor,
          radius = radius,
          temp_refFrame = Matrix4x4.identity
        };

        _strokeObj.Add(strokePoint);
      }

      // StrokeObjects -> PolyMesh.
      fillPolyMeshObject.polyMesh.Clear();
      var positions = Pool<List<Vector3>>.Spawn(); positions.Clear();
      var polygons = Pool<List<Polygon>>.Spawn(); polygons.Clear();
      var smoothEdges = Pool<List<Edge>>.Spawn(); smoothEdges.Clear();
      var colors = Pool<List<Color>>.Spawn(); colors.Clear();
      try {
        strokePolyMesher.FillPolyMeshData(_strokeObj,
                                          positions, polygons, smoothEdges, colors);

        fillPolyMeshObject.polyMesh.Fill(positions, polygons, smoothEdges, colors);
      }
      finally {
        positions.Clear(); Pool<List<Vector3>>.Recycle(positions);
        polygons.Clear(); Pool<List<Polygon>>.Recycle(polygons);
        smoothEdges.Clear(); Pool<List<Edge>>.Recycle(smoothEdges);
        colors.Clear(); Pool<List<Color>>.Recycle(colors);
      }

      // Refresh the Unity mesh representation of the PolyMeshObject.
      fillPolyMeshObject.RefreshUnityMesh();
    }

    #endregion

    #region Runtime Gizmos

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy || !drawDebug) return;

      drawer.color = LeapColor.white;

      for (int i = 0; i < thumbPoints.Count; i++) {
        drawer.DrawLine(thumbPoints[i], indexPoints[i]);
      }
    }

    #endregion

  }

}

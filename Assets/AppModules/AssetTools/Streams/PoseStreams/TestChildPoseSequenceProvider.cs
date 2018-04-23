using Leap.Unity.Attributes;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class TestChildPoseSequenceProvider : MonoBehaviour,
                                               IIndexable<Pose>,
                                               IStream<Pose> {

    public string nameMustContain = "";

    [QuickButton("Send Poses As Stream", "SendAsStream")]
    public bool unusedBool;
    
    [SerializeField]
    private List<Pose> poses = new List<Pose>();

    private void Update() {
      var transforms = Pool<List<Transform>>.Spawn();
      transforms.Clear();
      poses.Clear();
      try {
        this.GetComponentsInChildren<Transform>(transforms);

        foreach (var transform in transforms
                                    .Query()
                                    .Where(t => string.IsNullOrEmpty(nameMustContain)
                                           || t.name.Contains(nameMustContain))) {
          poses.Add(transform.ToWorldPose());
        }
      }
      finally {
        transforms.Clear();
        Pool<List<Transform>>.Recycle(transforms);
      }

      if (Time.frameCount % _frameCount == 0) {
        SendAsStream();
      }
    }

    private int _frameCount = 20;

    public Pose this[int idx] {
      get {
        return poses[idx];
      }
    }

    public int Count { get { return poses.Count; } }

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    /// <summary>
    /// Opens the stream, sends all pose data through it, and closes the stream.
    /// </summary>
    public void SendAsStream() {
      OnOpen();

      foreach (var pose in poses) {
        OnSend(pose);
      }

      OnClose();
    }

  }

}

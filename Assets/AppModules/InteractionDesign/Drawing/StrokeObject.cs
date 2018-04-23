using Leap.Unity.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class StrokeObject : MonoBehaviour, IIndexable<StrokePoint> {

    [SerializeField]
    private List<StrokePoint> _data;

    [SerializeField, Disable]
    private bool _isHidden = false;
    public bool isHidden { get { return _isHidden; } }

    /// <summary>
    /// Marks the StrokeObject as hidden, firing stroke modification callbacks if this
    /// changed the "hidden" state of the the StrokeObject.
    /// </summary>
    public void HideStroke() {
      if (!_isHidden) {
        _isHidden = true;
        OnModified();
        OnStrokeModified(this);
      }
    }

    /// <summary>
    /// Unmarks the StrokeObject from being hidden, firing stroke modification callbacks
    /// if this changed the "hidden" state of the the StrokeObject.
    /// </summary>
    public void UnhideStroke() {
      if (_isHidden) {
        _isHidden = false;
        OnModified();
        OnStrokeModified(this);
      }
    }

    /// <summary>
    /// Fired when this Stroke Object is modified.
    /// </summary>
    public event Action OnModified = () => { };

    /// <summary>
    /// Fired when this Stroke Object is modified; passes itself as an argument.
    /// </summary>
    public event Action<StrokeObject> OnStrokeModified = (stroke) => { };

    void Awake() {
      _data = Pool<List<StrokePoint>>.Spawn();
      _data.Clear();
    }

    void OnDestroy() {
      _data.Clear();
      Pool<List<StrokePoint>>.Recycle(_data);
    }

    public StrokePoint this[int idx] {
      get { return _data[idx]; }
    }

    public int Count {
      get { return _data.Count; }
    }

    public void Add(StrokePoint strokePoint) {
      _data.Add(strokePoint);

      OnModified();
      OnStrokeModified(this);
    }

    public void Clear() {
      _data.Clear();
    }

    public IndexableEnumerator<StrokePoint> GetEnumerator() {
      return new IndexableEnumerator<StrokePoint>(this);
    }

  }

}

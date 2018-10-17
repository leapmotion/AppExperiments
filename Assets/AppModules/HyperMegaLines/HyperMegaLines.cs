using UnityEngine;

namespace Leap.Unity.HyperMegaStuff {

  [ExecuteInEditMode]
  public class HyperMegaLines : MonoBehaviour {

    private static HyperMegaLines _drawer;
    public static HyperMegaLines drawer {
      get {
        if (_drawer == null) {
          _drawer = FindObjectOfType<HyperMegaLines>();
        }
        if (_drawer == null) {
          Debug.LogError("No HyperMegaLines object exists in the scene, "
            + "but something is trying to access it.");
        }
        return _drawer;
      }
    }

    public Color color;
    public Shader shader;
    public bool allowInBuilds = false;

    private void Reset() {
      this.color = Color.white;
    }

    private void Update() {
      if (!Application.isEditor && !allowInBuilds && this.enabled) {
        Debug.Log("Disabling HyperMegaLines in build because allowInBuilds is " +
          "set to false.");
        this.enabled = false;
      }
    }

    private void OnDisable() {
      _numPositions = 0;
      _numIndices = 0;
    }

    private Vector3? _lastLinePos = null;
    public void DrawLine(Vector3 a, Vector3 b) {
      if (!this.enabled) return;

      if (_lastLinePos.HasValue && _lastLinePos.Value == a) {
        appendLinePosition(b);
      }
      else {
        addLine(a, b);
      }
    }

    public void DrawLines(Vector3[] linePositions) {
      if (!this.enabled) return;
      if (linePositions.Length <= 1) return;

      addLine(linePositions[0], linePositions[1]);
      for (int i = 2; i < linePositions.Length; i++) {
        appendLinePosition(linePositions[i]);
      }
    }
    public void DrawLines(Vector3[] linePositions, int numPositions) {
      if (!this.enabled) return;
      if (numPositions > linePositions.Length) numPositions = linePositions.Length;
      if (numPositions <= 1) return;
      
      addLine(linePositions[0], linePositions[1]);
      for (int i = 2; i < numPositions; i++) {
        appendLinePosition(linePositions[i]);
      }
    }
    public void DrawLines(IIndexable<Vector3> linePositions) {
      if (!this.enabled) return;
      if (linePositions.Count <= 1) return;

      addLine(linePositions[0], linePositions[1]);
      for (int i = 2; i < linePositions.Count; i++) {
        appendLinePosition(linePositions[i]);
      }
    }
    public void DrawLines(ReadonlyList<Vector3> linePositions) {
      if (!this.enabled) return;
      if (linePositions.Count <= 1) return;

      addLine(linePositions[0], linePositions[1]);
      for (int i = 2; i < linePositions.Count; i++) {
        appendLinePosition(linePositions[i]);
      }
    }

    private Vector3[] _posBuffer = new Vector3[65536];
    private Color[] _colorBuffer = new Color[65536];
    private int _numPositions = 0;
    private int[] _idxBuffer = new int[65536];
    private int _numIndices = 0;

    private void addLine(Vector3 a, Vector3 b) {
      if (!this.enabled) return;

      _idxBuffer[_numIndices++] = _numPositions;
      _colorBuffer[_numPositions] = this.color;
      _posBuffer[_numPositions++] = a;

      _idxBuffer[_numIndices++] = _numPositions;
      _colorBuffer[_numPositions] = this.color;
      _posBuffer[_numPositions++] = b;
    }
    private void appendLinePosition(Vector3 position) {
      if (!this.enabled) return;

      _idxBuffer[_numIndices++] = _numPositions - 1;

      _idxBuffer[_numIndices++] = _numPositions;
      _colorBuffer[_numPositions] = this.color;
      _posBuffer[_numPositions++] = position;
    }

    [SerializeField]
    private Mesh _mesh;
    [SerializeField]
    private Material _material;

    public void Awake() {
      if (_drawer == null) _drawer = this;

      shader = Shader.Find("Hidden/Hyper Mega Lines");
    }

    public void LateUpdate() {
      if (_mesh == null) {
        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.HideAndDontSave;
      }

      _mesh.Clear();

      _mesh.vertices = _posBuffer;
      _mesh.colors = _colorBuffer;
      _mesh.SetIndices(_idxBuffer, MeshTopology.Lines, 0);

      _numPositions = 0;
      _numIndices = 0;

      if (_material == null) {
        _material = new Material(shader);
        _material.hideFlags = HideFlags.HideAndDontSave;
      }

      Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);

      _idxBuffer.ClearWith(0);
    }

  }

}

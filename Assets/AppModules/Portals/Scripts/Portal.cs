using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Portals {
  using Attributes;

  public class Portal : MonoBehaviour {

    [SerializeField]
    private LayerMask _renderMask = 1;
    public LayerMask renderMask {
      get { return _renderMask; }
    }

    [MinValue(0)]
    [SerializeField]
    private float _width;
    public float width {
      get { return _width; }
      set { _width = value; }
    }

    [MinValue(0)]
    [SerializeField]
    private float _height;
    public float height {
      get { return _height; }
      set { _height = value; }
    }

    private bool _isInsidePortal = false;

    private Bounds _meshBounds;
    private Mesh _mesh;

    private Material _stencilMaskMaterial;
    private Material _zBlockMaterial;
    private Vector3 _prevHeadPosition;

    public bool isInsidePortal {
      get {
        return _isInsidePortal;
      }
    }

    public Rect portalRect {
      get {
        return new Rect(_width / -2, _height / -2, _width, _height);
      }
    }

    public bool isBehindPortal {
      get {
        if (_isInsidePortal) {
          return _prevHeadPosition.z <= 0;
        } else {
          return _prevHeadPosition.z > 0;
        }
      }
    }

    public float DistTo(Vector3 point) {
      Vector3 localPoint = transform.InverseTransformPoint(point);
      Bounds portalBounds = new Bounds(Vector3.zero, new Vector3(_width, _height, 0));

      Vector3 closestPoint = portalBounds.ClosestPoint(localPoint);

      //Assume lossyScale == 1
      return (localPoint - closestPoint).sqrMagnitude;
    }

    private void Awake() {
      _mesh = new Mesh();
      _mesh.hideFlags = HideFlags.HideAndDontSave;
      _mesh.name = "Portal Mesh";
    }

    private void OnEnable() {
      if (PortalManager.Instance == null) {
        Debug.LogError("Could not enable Portal " + this + " because no Portal Manager was found in the scene.");
        enabled = false;
      }
      PortalManager.Instance.AddPortal(this);

      if (_stencilMaskMaterial == null) {
        var stencilMaskShader = Shader.Find("Hidden/StencilMask");
        if (stencilMaskShader == null) {
          Debug.LogError("Could not find stencil mask shader.");
          enabled = false;
          return;
        }

        _stencilMaskMaterial = new Material(stencilMaskShader);
      }

      if (_zBlockMaterial == null) {
        var zBlockShader = Shader.Find("Hidden/ZBlock");
        if (zBlockShader == null) {
          Debug.LogError("Could not find zBlock shader.");
          enabled = false;
          return;
        }

        _zBlockMaterial = new Material(zBlockShader);
      }
    }

    private void OnDisable() {
      if (PortalManager.Instance != null) {
        PortalManager.Instance.RemovePortal(this);
      }

      _isInsidePortal = false;
    }

    private void Update() {
      Vector3 localHeadPos = transform.InverseTransformPoint(PortalManager.Instance.mainCamera.transform.position);

      if (portalRect.Contains(localHeadPos)) {
        if (localHeadPos.z < 0 && _prevHeadPosition.z >= 0) {
          _isInsidePortal = false;
        } else if (localHeadPos.z >= 0 && _prevHeadPosition.z < 0 && !PortalManager.Instance.insideAnyPortal) {
          _isInsidePortal = true;
        }
      }

      _prevHeadPosition = localHeadPos;
      updateMesh();
    }

    public void DrawMaskAndBlocker(int mask, int layer, Camera camera) {
      DrawStencilMask(mask, layer, camera);
      DrawZBlocker(mask + 1, layer, camera);
    }

    public void DrawStencilMask(int mask, int layer, Camera camera) {
      _stencilMaskMaterial.SetFloat("_Mask", mask);
      Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, _stencilMaskMaterial, layer, camera);
    }

    public void DrawZBlocker(int mask, int layer, Camera camera) {
      _zBlockMaterial.SetFloat("_Mask", mask);
      Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, _zBlockMaterial, layer, camera);
    }

    private void updateMesh() {
      Bounds newMeshBounds;

      if (portalRect.Contains(_prevHeadPosition)) {
        if (_isInsidePortal) {
          float distance = Mathf.Max(0, -_prevHeadPosition.z + PortalManager.Instance.headSize);
          newMeshBounds = new Bounds(new Vector3(0, 0, distance / -2), new Vector3(_width, _height, distance));
        } else {
          float distance = Mathf.Max(0, _prevHeadPosition.z + PortalManager.Instance.headSize);
          newMeshBounds = new Bounds(new Vector3(0, 0, distance / 2), new Vector3(_width, _height, distance));
        }
      } else {
        newMeshBounds = new Bounds(Vector3.zero, new Vector3(_width, _height, 0));
      }


      if (newMeshBounds != _meshBounds) {
        using (new ProfilerSample("Update Portal Mesh", this)) {
          _meshBounds = newMeshBounds;
          CubeBuilder.CreateCubeMesh(_mesh, _meshBounds,
                                     bottom: Face.All,
                                     left: Face.All,
                                     right: Face.All,
                                     top: Face.All,
                                     back: _isInsidePortal ? Face.None : Face.All,
                                     front: _isInsidePortal ? Face.All : Face.None);
        }
      }
    }

    private void OnDrawGizmos() {
      Gizmos.color = Color.white;
      Gizmos.matrix = transform.localToWorldMatrix;

      float w = _width / 2;
      float h = _height / 2;
      float w2 = (_width - 0.05f) / 2;
      float h2 = (_height - 0.05f) / 2;

      Gizmos.DrawLine(new Vector3(-w, -h), new Vector3(+w, -h));
      Gizmos.DrawLine(new Vector3(+w, -h), new Vector3(+w, +h));
      Gizmos.DrawLine(new Vector3(+w, +h), new Vector3(-w, +h));
      Gizmos.DrawLine(new Vector3(-w, +h), new Vector3(-w, -h));

      Gizmos.DrawLine(new Vector3(-w2, -h2), new Vector3(+w2, -h2));
      Gizmos.DrawLine(new Vector3(+w2, -h2), new Vector3(+w2, +h2));
      Gizmos.DrawLine(new Vector3(+w2, +h2), new Vector3(-w2, +h2));
      Gizmos.DrawLine(new Vector3(-w2, +h2), new Vector3(-w2, -h2));
    }
  }
}

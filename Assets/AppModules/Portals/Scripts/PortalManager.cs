using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Portals {
  using Query;

  public class PortalManager : MonoBehaviour {

    private static PortalManager _cachedInstance = null;
    public static PortalManager Instance {
      get {
        if (_cachedInstance == null) {
          _cachedInstance = FindObjectOfType<PortalManager>();
        }
        return _cachedInstance;
      }
    }

    [SerializeField]
    private float _headSize = 0.08f;
    public float headSize {
      get { return _headSize; }
      set { _headSize = value; }
    }

    [SerializeField]
    private Camera _mainCamera;
    public Camera mainCamera {
      get { return _mainCamera; }
    }

    [SerializeField]
    private LayerMask _outsideMask;

    [SerializeField]
    private bool _clearMainMask = true;

    private Camera _outsideCamera;
    private int _singleMainLayer;
    private List<int> _outsideLayers;
    private Transform _portalCameraAnchor = null;

    private Portal _insidePortal = null;
    private bool _needToUpdateMaterials = true;
    private List<PortalGroup> _groups = new List<PortalGroup>();
    private Comparison<PortalGroup> _portalSortDelegate;

    public bool insideAnyPortal {
      get {
        return _groups.Query().Any(g => g.portal.isInsidePortal);
      }
    }

    public void AddPortal(Portal portal) {
      if (_groups.Query().Any(g => g.portal == portal)) {
        throw new InvalidOperationException("Cannot add portal " + portal + " to this manager because it has already been added.");
      }

      PortalGroup group = new PortalGroup() {
        portal = portal
      };

      createPortalCamera(portal.renderMask, out group.camera, out group.layers);

      _groups.Add(group);

      if (_clearMainMask) {
        _mainCamera.cullingMask &= ~portal.renderMask;
      }

      _needToUpdateMaterials = true;
    }

    public void RemovePortal(Portal portal) {
      var i = _groups.Query().IndexOf(g => g.portal == portal);
      if (i < 0) {
        throw new InvalidOperationException("Cannot remove portal " + portal + " because it has not been added to this manager.");
      }

      destroyPortalCamera(_groups[i].camera);
      _groups.RemoveAt(i);

      _needToUpdateMaterials = true;
    }

    public void ForceRefeshOfMaskedMaterials() {
      _needToUpdateMaterials = true;
    }

    private void OnEnable() {
      _singleMainLayer = getLayersFromMask(_mainCamera.cullingMask)[0];

      createPortalCamera(_outsideMask, out _outsideCamera, out _outsideLayers);

      if (_clearMainMask) {
        _mainCamera.cullingMask &= ~_outsideMask;
      }

      _portalSortDelegate = (a, b) => {
        return a.distToCamera.CompareTo(b.distToCamera);
      };
    }

    private void OnDisable() {
      destroyPortalCamera(_outsideCamera);
    }

    private void LateUpdate() {

      //Sort portals back to front for roughly correct occlusion
      using (new ProfilerSample("Sort Portals")) {
        for (int i = 0; i < _groups.Count; i++) {
          var group = _groups[i];
          group.distToCamera = group.portal.DistTo(_mainCamera.transform.position);
          _groups[i] = group;
        }

        _groups.Sort(_portalSortDelegate);
      }

      int insideIndex = -1;

      //Calculate which portal we are inside of (if any)
      using (new ProfilerSample("Calculate Inside Portal")) {
        for (int i = 0; i < _groups.Count; i++) {
          if (_groups[i].portal.isInsidePortal) {
            insideIndex = i;
            break;
          }
        }
      }

      //If we have moved into or out of a portal, update all of the masked materials
      using (new ProfilerSample("Update Masked Materials")) {
        Portal newInsidePortal = insideIndex >= 0 ? _groups[insideIndex].portal : null;
        if (newInsidePortal != _insidePortal || _needToUpdateMaterials) {
          _insidePortal = newInsidePortal;
          _needToUpdateMaterials = false;

          if (insideIndex >= 0) {
            for (int i = 0; i < _groups.Count; i++) {
              if (i == insideIndex) continue;
              PortalMasked.SetMaskForLayers(_groups[i].layers, 2);
            }
            PortalMasked.SetMaskForLayers(_outsideLayers, 1);
            PortalMasked.SetMaskForLayers(_groups[insideIndex].layers, 0);
          } else {
            for (int i = 0; i < _groups.Count; i++) {
              PortalMasked.SetMaskForLayers(_groups[i].layers, 1);
            }
            PortalMasked.SetMaskForLayers(_outsideLayers, 0);
          }
        }
      }

      // Set order of cameras and set up portal draw calls
      using (new ProfilerSample("Update Portal Cameras")) {
        var depthOffset = _mainCamera.depth;
        if (insideIndex >= 0) {
          for (int i = 0; i < _groups.Count; i++) {
            if (i == insideIndex) continue;
            var group = _groups[i];

            if (group.portal.isBehindPortal) {
              group.camera.enabled = false;
            } else {
              group.camera.enabled = true;
              group.camera.depth = ++depthOffset;
              group.portal.DrawMaskAndBlocker(1, group.layers[0], group.camera);
            }
          }

          _outsideCamera.depth = ++depthOffset;

          var insideGroup = _groups[insideIndex];
          insideGroup.camera.depth = ++depthOffset;
          insideGroup.camera.enabled = true;

          if (!insideGroup.portal.isBehindPortal) {
            insideGroup.portal.DrawStencilMask(0, _singleMainLayer, _mainCamera);
            insideGroup.portal.DrawZBlocker(1, _outsideLayers[0], _outsideCamera);
          }
        } else {
          for (int i = 0; i < _groups.Count; i++) {
            var group = _groups[i];
            if (group.portal.isBehindPortal) {
              group.camera.enabled = false;
            } else {
              group.camera.enabled = true;
              group.camera.depth = ++depthOffset;

              group.portal.DrawMaskAndBlocker(0, group.layers[0], group.camera);
            }
          }

          _outsideCamera.depth = ++depthOffset;
        }
      }
    }

    private void createPortalCamera(LayerMask mask, out Camera camera, out List<int> layers) {
      if (_portalCameraAnchor == null) {
        var portalCameraObj = new GameObject("Portal Cameras");
        portalCameraObj.hideFlags = HideFlags.NotEditable;

        _portalCameraAnchor = portalCameraObj.transform;
        _portalCameraAnchor.transform.parent = _mainCamera.transform.parent;
        _portalCameraAnchor.SetSiblingIndex(_mainCamera.transform.GetSiblingIndex() + 1);
        _portalCameraAnchor.localPosition = Vector3.zero;
        _portalCameraAnchor.localRotation = Quaternion.identity;
        _portalCameraAnchor.localScale = Vector3.one;
      }

      GameObject cameraObj = new GameObject("Portal Camera");//TODO: better naming
      cameraObj.hideFlags = HideFlags.NotEditable;
      cameraObj.transform.parent = _portalCameraAnchor;
      cameraObj.transform.localPosition = _mainCamera.transform.localPosition;
      cameraObj.transform.localRotation = _mainCamera.transform.localRotation;
      cameraObj.transform.localScale = _mainCamera.transform.localScale;

      camera = cameraObj.AddComponent<Camera>();
      camera.clearFlags = CameraClearFlags.Nothing;
      camera.cullingMask = mask;
      camera.nearClipPlane = _mainCamera.nearClipPlane;
      camera.farClipPlane = _mainCamera.farClipPlane;
      camera.allowHDR = _mainCamera.allowHDR;
      camera.allowMSAA = _mainCamera.allowMSAA;
      if (!XRSupportUtil.IsXREnabled()) { camera.fieldOfView = _mainCamera.fieldOfView; }

      layers = getLayersFromMask(mask);
    }

    private void destroyPortalCamera(Camera camera) {
      DestroyImmediate(camera.gameObject);
    }

    private List<int> getLayersFromMask(int mask) {
      List<int> layers = new List<int>();
      for (int i = 0; i < 32; i++) {
        if ((mask & (1 << i)) != 0) {
          layers.Add(i);
        }
      }
      return layers;
    }

    [System.Serializable]
    public struct PortalGroup {
      public Portal portal;
      public Camera camera;
      public List<int> layers;
      public float distToCamera;
    }
  }
}

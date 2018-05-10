using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Portals {
  using Query;

  public class PortalMasked : MonoBehaviour {

    private static List<Material>[] _materials = new List<Material>[32];

    static PortalMasked() {
      for (int i = 0; i < 32; i++) {
        _materials[i] = new List<Material>();
      }
    }

    public static void SetMaskForLayer(int layer, int mask) {
      var list = _materials[layer];
      for (int i = list.Count; i-- != 0;) {
        if (list[i] == null) {
          list.RemoveAtUnordered(i);
          continue;
        }

        list[i].SetFloat("_PortalMask", mask);
      }
    }

    public static void SetMaskForLayers(List<int> layers, int mask) {
      foreach (var layer in layers) {
        SetMaskForLayer(layer, mask);
      }
    }

    public static void RegisterMaterial(int layer, Material material) {
      _materials[layer].Add(material);
    }

    [SerializeField]
    private bool _maskChildren = true;

    private void Awake() {
      Refresh();
    }

    public void Refresh() {
      if (PortalManager.Instance != null) {
        PortalManager.Instance.ForceRefeshOfMaskedMaterials();
      }

      List<Renderer> renderers = Pool<List<Renderer>>.Spawn();
      try {
        if (_maskChildren) {
          GetComponentsInChildren(includeInactive: true, result: renderers);
        } else {
          GetComponents(renderers);
        }

        foreach (var renderer in renderers) {
          int layer = renderer.gameObject.layer;
          foreach (var material in renderer.materials) {
            _materials[layer].Add(material);
          }
        }
      } finally {
        renderers.Clear();
        Pool<List<Renderer>>.Recycle(renderers);
      }
    }
  }
}

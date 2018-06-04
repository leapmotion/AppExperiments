using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class IdleActivePanelPaletteController : MonoBehaviour {

    public PaletteSwitcherController paletteSwitcherController;
    public Transform findInteractionObjectsWithin;

    public int activePaletteIdx = 0;
    public int idlePaletteIdx = 1;

    public float waitTimeBeforeIdle = 3f;

    private bool _haveInitializedInteractionObjects = false;
    private float _timeSinceLastPrimaryHover = 0f;
    private List<InteractionBehaviour> _intObjs = new List<InteractionBehaviour>();

    void Reset() {
      if (paletteSwitcherController == null) {
        paletteSwitcherController = GetComponent<PaletteSwitcherController>();
      }

      if (findInteractionObjectsWithin == null) {
        findInteractionObjectsWithin = this.transform;
      }
    }

    void OnEnable() {
      if (!_haveInitializedInteractionObjects) {
        findInteractionObjectsWithin.GetComponentsInChildren(true, _intObjs);
        foreach (var intObj in _intObjs) {
          intObj.OnPrimaryHoverStay += onPrimaryHoverStay;
        }

        _haveInitializedInteractionObjects = true;
      }
    }

    void OnDisable() {
      if (_haveInitializedInteractionObjects) {
        foreach (var intObj in _intObjs) {
          intObj.OnPrimaryHoverStay -= onPrimaryHoverStay;
        }
      }
    }

    void Update() {
      _timeSinceLastPrimaryHover += Time.deltaTime;

      if (_timeSinceLastPrimaryHover > waitTimeBeforeIdle) {
        paletteSwitcherController.curPaletteIdx = idlePaletteIdx;
      }
      else {
        paletteSwitcherController.curPaletteIdx = activePaletteIdx;
      }
    }

    private void onPrimaryHoverStay() {
      _timeSinceLastPrimaryHover = 0f;
    }

  }

}

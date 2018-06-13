using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Query;
using Leap.Unity.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Launcher {

  public enum HeldOrientabilityType { LockedFacing, Free }

  public enum HeldVisiblityType { Hide, StayOpen }

  public enum CurvatureType { Flat, Cylindrical, Spherical }

  public enum HandleType { Orb, Titlebar }

  public class PanelCommandReceiver : MonoBehaviour {

    #region Curvature Control

    [Header("Curvature Control")]

    public bool supportsCurvature = true;

    [SerializeField, OnEditorChange("curvatureType")]
    private CurvatureType _currentCurvatureType = CurvatureType.Spherical;
    public CurvatureType currentCurvatureType {
      get { return _currentCurvatureType; }
      set {
        if (value != _currentCurvatureType) {
          if (value == CurvatureType.Spherical) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(true);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(false);
            if (flatPanelObject != null)        flatPanelObject.SetActive(false);
          }
          else if (value == CurvatureType.Cylindrical) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(false);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(true);
            if (flatPanelObject != null)        flatPanelObject.SetActive(false);
          }
          else if (value == CurvatureType.Flat) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(false);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(false);
            if (flatPanelObject != null)        flatPanelObject.SetActive(true); 
          }

          _currentCurvatureType = value;
        }
      }
    }

    public GameObject flatPanelObject;

    [SerializeField, OnEditorChange("sphericalSpace")]
    private LeapSphericalSpace _sphericalSpace;
    public LeapSphericalSpace sphericalSpace {
      get { return _sphericalSpace; }
      set {
        _sphericalSpace = value;
        if (_sphericalSpace != null) {
          _sphericalSpace.radius = curvatureToRadius(curvatureAmount);
        }
      }
    }
    public GameObject sphericalPanelObject;

    [SerializeField, OnEditorChange("cylindricalSpace")]
    private LeapCylindricalSpace _cylindricalSpace;
    public LeapCylindricalSpace cylindricalSpace {
      get { return _cylindricalSpace; }
      set {
        _cylindricalSpace = value;
        if (_cylindricalSpace != null) {
          _cylindricalSpace.radius = curvatureToRadius(curvatureAmount);
        }
      }
    }
    public GameObject cylindricalPanelObject;

    [SerializeField, OnEditorChange("curvatureAmount")]
    private float _curvatureAmount = 0f;
    public float curvatureAmount {
      get { return _curvatureAmount; }
      set {
        value = Mathf.Clamp01(value);
        _curvatureAmount = value;
        if (sphericalSpace != null) {
          sphericalSpace.radius = curvatureToRadius(_curvatureAmount);
        }
        if (cylindricalSpace != null) {
          cylindricalSpace.radius = curvatureToRadius(_curvatureAmount);
        }
      }
    }

    private float curvatureToRadius(float curvatureAmount) {
      return Mathf.Sqrt(curvatureAmount).Map(0f, 1f, 2f, 0.2f);
    }

    /// <summary>
    /// Returns normalized curvature amount.
    /// </summary>
    public float GetCurvature() {
      return curvatureAmount;
    }
    
    /// <summary>
    /// Sets the amount of curvature and sets the radius appropriately of an attached
    /// LeapRadialSpace.
    /// </summary>
    public void SetCurvature(float normalizedCurvatureAmount) {
      curvatureAmount = normalizedCurvatureAmount;
    }

    public CurvatureType GetCurvatureType() {
      return _currentCurvatureType;
    }

    public void SetCurvatureType(CurvatureType type) {
      currentCurvatureType = type;
    }

    #endregion

    #region Held Orientability

    [Header("Held Orientability")]

    public MonoBehaviour[] lockOrientationComponents;

    public HeldOrientabilityType GetHeldOrientabilityType() {
      if (lockOrientationComponents.Query().Any(c => c.enabled)) {
        return HeldOrientabilityType.LockedFacing;
      }

      return HeldOrientabilityType.Free;
    }

    public void SetHeldOrientabilityType(HeldOrientabilityType type) {
      switch (type) {
        case HeldOrientabilityType.Free:
          foreach (var component in lockOrientationComponents) {
            component.enabled = false;
          }
          break;
        case HeldOrientabilityType.LockedFacing:
          foreach (var component in lockOrientationComponents) {
            component.enabled = true;
          }
          break;
      }
    }

    #endregion

    #region Held Visibility

    [Header("Held Visiblity")]

    [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _hideWhenHeldSwitch;
    public IPropertySwitch hideWhenHeldSwitch {
      get {
        return _hideWhenHeldSwitch as IPropertySwitch;
      }
    }

    public HeldVisiblityType heldVisibilityType {
      get {
        if (hideWhenHeldSwitch == null) {
          return HeldVisiblityType.StayOpen;
        }
        else {
          if (hideWhenHeldSwitch.GetIsOnOrTurningOn()) {
            return HeldVisiblityType.Hide;
          }
          else {
            return HeldVisiblityType.StayOpen;
          }
        }
      }
      set {
        if (hideWhenHeldSwitch == null) return;

        switch (value) {
          case HeldVisiblityType.StayOpen:
            if (hideWhenHeldSwitch.GetIsOnOrTurningOn()) {
              hideWhenHeldSwitch.AutoOff();
            }
            break;

          case HeldVisiblityType.Hide:
            if (hideWhenHeldSwitch.GetIsOffOrTurningOff()) {
              hideWhenHeldSwitch.AutoOn();
            }
            break;
        }
      }
    }

    public HeldVisiblityType GetHeldVisiblityType() {
      return heldVisibilityType;
    }

    public void SetHeldVisiblityType(HeldVisiblityType type) {
      heldVisibilityType = type;
    }

    #endregion

    #region Handle Switching

    [Header("Handle Switching")]

    [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _orbSwitch;
    public IPropertySwitch orbSwitch {
      get {
        return _orbSwitch as IPropertySwitch;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _titlebarSwitch;
    public IPropertySwitch titlebarSwitch {
      get {
        return _titlebarSwitch as IPropertySwitch;
      }
    }
    

    [SerializeField, OnEditorChange("currentHandleType")]
    private HandleType _currentHandleType = HandleType.Orb;
    public HandleType currentHandleType {
      get { return _currentHandleType; }
      set {
        var orbOn = false;
        var barOn = false;

        if (value == HandleType.Orb) {
          orbOn = true;
        }
        else if (value == HandleType.Titlebar) {
          barOn = true;
        }

        if (orbOn) {
          if (orbSwitch.GetIsOffOrTurningOff()) {
            orbSwitch.AutoOn();
          }
        }
        else {
          if (orbSwitch.GetIsOnOrTurningOn()) {
            orbSwitch.AutoOff();
          }
        }

        if (barOn) {
          if (titlebarSwitch.GetIsOffOrTurningOff()) {
            titlebarSwitch.AutoOn();
          }
        }
        else {
          if (titlebarSwitch.GetIsOnOrTurningOn()) {
            titlebarSwitch.AutoOff();
          }
        }

        _currentHandleType = value;
      }
    }

    public HandleType GetHandleType() {
      return currentHandleType;
    }

    public void SetHandleType(HandleType type) {
      currentHandleType = type;
    }

    #endregion


  }

}

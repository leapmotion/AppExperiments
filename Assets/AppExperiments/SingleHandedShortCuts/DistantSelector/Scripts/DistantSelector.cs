/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using Leap.Unity.Animation;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity;
using Leap.Unity.RuntimeGizmos;
using System;
using Leap;
using System.Linq;
using UnityEngine.Events;

namespace LeapSingleHandedShortcuts {
  [RequireComponent(typeof(InteractionManager))]
  public class DistantSelector : MonoBehaviour {
    public TextMesh StateDebugText;
    private InteractionManager interactionManager;
    public InteractionHand InteractionHand_L;
    public InteractionHand InteractionHand_R;
    private InteractionHand activeHand;

    public Transform Cam;
    public LayerMask Layermask = 1 << 8;
    public XRLineRenderer VisualLineRenderer;
    public XRLineRenderer LineRendererLasso;
    private Vector3[] lassoPoints;
    private List<Vector3> lassoPointList = new List<Vector3>();
    public Transform ProjectionPlane;

    private DistanSelectorFSM DIfsm = new DistanSelectorFSM();
    private DistantSelectableBehaviour distantHoveredObject;
    private DistantSelectableBehaviour selectedObject;

    private float hoverRayLength = 6f;
    private float hoverRayTargetLength = 6f;
    private float lastSelectionPose;
    private float poseHysteresis = .5f;

    public Transform Palm_L;
    public Transform Palm_R;
    public Transform ShoulderAdjust_L;
    public Transform ShoulderAdjust_R;

    public float RayExtendedDistance = 4f;
    public float RayRetractedDistance = .7f;
    public float RayExtensionSpeed = 6f;
    public float ReachThreshhold = .01f;

    public Gradient disabledRayColor;
    public Gradient HoveredRayColor;
    public Gradient SelectedRayColor;
    public Gradient LassoRayColor;
    public Gradient DefaultRayColor;

    public Transform[] controlPoints;
    public Vector3[] controlPositions = new Vector3[4];
    private int segmentCount = 30;

    public Action OnClearSelection;
    public UnityEvent OnDeselectAll;

    DistantSelectableBehaviour hoverCandidate;

    private DistantSelectableBehaviour[] allSelectables;
    private List<DistantSelectableBehaviour> selectionSet;
    private List<Ray> selectionRays = new List<Ray>();
    private Ray centralRay = new Ray();

    public enum ProjectionRayType { SHOULDER_WRIST, SHOULDER_WRIST_INDEX, CAMERA_PINCH_POINT, SHOULDER_PINCH_POINT, ARM }
    public ProjectionRayType HoverRayType;
    public ProjectionRayType LassoRayType;

    private float pinchConfirmDuration = .5f;
    private float pinchConfirmTime = 0f;
    private bool isHysteresisPinch;
    public bool IsHysteresisPinch {
      get {
        if (isHysteresisPinch == false) {
          float tipDistance = (activeHand.leapHand.Fingers[1].TipPosition - activeHand.leapHand.Fingers[0].TipPosition).Magnitude;
          if (tipDistance < .015f && IsFingerPointing() == false) {
            pinchConfirmTime += Time.deltaTime;
          }
          if (pinchConfirmTime > pinchConfirmDuration) {
            isHysteresisPinch = true;
            pinchConfirmTime = 0f;
          }
        }
        if (isHysteresisPinch == true) {
          if (activeHand.leapHand.PinchStrength < .6f) {
            pinchConfirmTime += Time.deltaTime;
          }
          if (pinchConfirmTime > pinchConfirmDuration) {
            isHysteresisPinch = false;
            pinchConfirmTime = 0f;
          }
        }
        return isHysteresisPinch;
      }

      set {
        isHysteresisPinch = value;
      }
    }
    FingerDirectionAverage movingAverage = new FingerDirectionAverage();


    public TextMesh DebugState;
    public TextMesh DebugPinchStrength;
    public TextMesh DebugIsHysteresis;

    void Start() {
      allSelectables = FindObjectsOfType<DistantSelectableBehaviour>();
      interactionManager = GetComponent<InteractionManager>();
      Cam = Camera.main.transform;

      selectionMesh = new Mesh();
    }

    void Update() {
      DebugState.text = DIfsm.State.ToString();
      if(activeHand != null) DebugPinchStrength.text = activeHand.leapHand.PinchStrength.ToString();
      if(activeHand != null)DebugIsHysteresis.text = IsHysteresisPinch.ToString();

      if (!InteractionHand_R.isTracked && InteractionHand_L.isTracked) activeHand = InteractionHand_L;
      else if (!InteractionHand_L.isTracked && InteractionHand_R.isTracked) activeHand = InteractionHand_R;
      else if (!activeHand) activeHand = InteractionHand_R;
      if (!IsArmExtended(activeHand.leapHand.PalmPosition.ToVector3())) ResetVisualCurve();

      switch (DIfsm.State) {
        case DistanSelectorFSM.States.Standby:
          if (activeHand.isTracked && IsArmExtended(activeHand.leapHand.PalmPosition.ToVector3())){

            if (IsFingerPointing()) {
              if(activeHand.leapHand.Fingers[0].IsExtended) DistantHoverSelect(true);
              else DistantHoverSelect(false);
            }
            else ResetVisualCurve();
          }
          //if fingerPointing change state and clear selection
          if(IsHysteresisPinch == true) BeginDrawing();
          break;
        case DistanSelectorFSM.States.Drawing:
          if (activeHand.isTracked 
            && IsArmExtended(activeHand.leapHand.PalmPosition.ToVector3())
            && activeHand.leapHand.IsPinching() == true) DrawProjectionRayArray();
          if (Input.GetKeyUp(KeyCode.Space) || IsHysteresisPinch == false) SelectorEvent(DistanSelectorFSM.Events.Select);
          break;
        case DistanSelectorFSM.States.Selected:
          if (Input.GetKeyUp(KeyCode.Space) || IsHysteresisPinch == true){
            if (activeHand.isTracked && IsArmExtended(activeHand.leapHand.PalmPosition.ToVector3())) BeginDrawing();
          }
          if (activeHand.isTracked && IsArmExtended(activeHand.leapHand.PalmPosition.ToVector3())) {

            if (IsFingerPointing()) {
              if (activeHand.leapHand.Fingers[0].IsExtended) DistantHoverSelect(true);
              else DistantHoverSelect(false);
            }
            else ResetVisualCurve();
          }
          break;
        case DistanSelectorFSM.States.Summoning:
          break;
        case DistanSelectorFSM.States.Held:
          break;
        case DistanSelectorFSM.States.Returning:
          break;
      }
      //StateDebugText.text = DIfsm.State.ToString();
    }

    private void DistantHoverSelect(bool isSelecting) {
      Gradient rayColor = isSelecting ? HoveredRayColor : SelectedRayColor;
      VisualLineRenderer.enabled = true;
      DistantSelectableBehaviour distantSelectableBehaviour = null;
      Ray hoverRay = ProjectionRay(HoverRayType);
      RaycastHit hit;
      hoverRayLength = Mathf.Lerp(hoverRayLength, hoverRayTargetLength, Time.deltaTime * RayExtensionSpeed);

      //Placing visual curve control points
      controlPoints[0].position = activeHand.leapHand.Fingers[1].TipPosition.ToVector3();
      controlPoints[1].position = hoverRay.GetPoint(.1f);
      controlPoints[2].position = hoverRay.GetPoint(hoverRayLength);
      controlPoints[3].position = hoverRay.GetPoint(hoverRayLength + .2f);

      if (IsHoverPosing()) {
        //Hovering
        if (Physics.Raycast(hoverRay, out hit, 100, Layermask)) {
          distantSelectableBehaviour = hit.transform.GetComponent<DistantSelectableBehaviour>();
          if (hit.distance > 1.5f) {
            hoverRayLength = hit.distance - .2f;
            if (distantSelectableBehaviour != null) {
              controlPoints[3].position = distantSelectableBehaviour.transform.position;
            }
            DrawVisualCurve(rayColor);
          }
          else if (hit.distance > ReachThreshhold) { 
            hoverRayLength = hit.distance - .2f;
            DrawVisualCurve(disabledRayColor);
          }
        }
        DrawVisualCurve(rayColor);
        hoverCandidate = distantSelectableBehaviour;
        if (hoverCandidate != null) {
          BeginDistantHover(hoverCandidate);
        }
        else EndDistantHover();
      }
      else hoverRayLength = RayRetractedDistance;
    }


    private bool IsHoverPosing() {
      bool posing = false;
      if (IsPalmFacingForward() && IsHandFullyExtended()) {
        hoverRayTargetLength = RayExtendedDistance;
        lastSelectionPose = Time.time;
        posing = true;
      }
      // extra checks to avoid reselecting when summoning
      else if (Time.time - lastSelectionPose < poseHysteresis) {
        if (!distantHoveredObject) {
          posing = true;
          hoverRayTargetLength = RayRetractedDistance;
        }
        else if (IsPalmFacingForward()) posing = true;
      }
      if (!posing) lastSelectionPose = 0f;
      return posing;
    }

    private bool IsFingerPointing() {
      bool isPointing = false;
      Hand hand = activeHand.leapHand;
      if (hand.Fingers[1].IsExtended
        && !hand.Fingers[2].IsExtended
        && !hand.Fingers[3].IsExtended
        && !hand.Fingers[4].IsExtended) isPointing = true;
      return isPointing;
    }

    private bool IsHandFullyExtended() {
      bool fullyExtended = false;
      if (activeHand.leapHand.GrabStrength < .5f) fullyExtended = true;
      return fullyExtended;
    }

    private bool IsPalmFacingForward() {
      bool facingForward = false;
      Vector3 palmDirection = activeHand.leapHand.PalmNormal.ToVector3();
      float palmNormalDot = Vector3.Dot(palmDirection, Cam.transform.forward * -1f);
      if (palmNormalDot < -.25f) facingForward = true;
      return facingForward;
    }

    public bool IsArmExtended(Vector3 palmPosition) {
      bool outOfReach = false;
      Vector3 flattenedHandPosition = palmPosition;
      flattenedHandPosition.y = Cam.transform.position.y;
      float reachDistance = (Cam.transform.position - flattenedHandPosition).magnitude;
      if (reachDistance > ReachThreshhold) outOfReach = true;
      return outOfReach;
    }


    public void BeginDistantHover(DistantSelectableBehaviour hovered) {
      if (distantHoveredObject) EndDistantHover();
      distantHoveredObject = hovered;
      distantHoveredObject.OnDistantHoverBegin();
    }

    public void EndDistantHover() {
      //distantHoveredObject.OnDistantHoverEnd();
      //distantHoveredObject = null;
    }

    public void DistantSelect() {
      DIfsm.ProcessEvent(DistanSelectorFSM.Events.Select);
    }

    public void DistantDeSelect() {
      selectedObject = null;
      ResetVisualCurve();
      DIfsm.ProcessEvent(DistanSelectorFSM.Events.Deselect);
    }
    private void BeginDrawing() {
      ClearSelection();
      SelectorEvent(DistanSelectorFSM.Events.Draw);
    }

    private Ray ProjectionRay(ProjectionRayType rayType) {
      //Gizmos
      RuntimeGizmoDrawer drawer = null;
      RuntimeGizmoManager.TryGetGizmoDrawer(out drawer);

      Vector3 shoulderPosition = activeHand.isLeft ? ShoulderAdjust_L.position : ShoulderAdjust_R.position;
      Vector3 palmPosition = activeHand.isLeft ? Palm_L.position : Palm_R.position;

      Vector3 ShoulderPalmDirection = (palmPosition - shoulderPosition).normalized;
      Vector3 shoulderPinchDirection = (palmPosition - shoulderPosition).normalized;
      Vector3 ArmDirection = (1f * activeHand.leapHand.Arm.Direction.ToVector3()).normalized;
      Vector3 camPinchDirection = activeHand.leapHand.GetPinchPosition() - Cam.transform.position;

      Vector3 FingerDirection = activeHand.leapHand.Fingers[1].Direction.ToVector3();
      movingAverage.ComputeAverage(FingerDirection);

      Vector3 rayDirection = Vector3.zero;
      Vector3 rayOrigin = Vector3.zero;

      switch (rayType) {
        case ProjectionRayType.SHOULDER_WRIST:
          rayOrigin = shoulderPosition;
          rayDirection = ShoulderPalmDirection;
          break;
        case ProjectionRayType.SHOULDER_WRIST_INDEX:
          rayOrigin = activeHand.leapHand.Fingers[1].TipPosition.ToVector3();
          rayDirection = ShoulderPalmDirection + movingAverage.Average;
          break;
        case ProjectionRayType.CAMERA_PINCH_POINT:
          rayOrigin = activeHand.leapHand.GetPinchPosition();
          rayDirection = camPinchDirection;
          break;
        case ProjectionRayType.SHOULDER_PINCH_POINT:
          rayOrigin = activeHand.leapHand.GetPinchPosition();
          rayDirection = shoulderPinchDirection;
          break;
        case ProjectionRayType.ARM:
          rayOrigin = activeHand.leapHand.WristPosition.ToVector3();
          rayDirection = ArmDirection;
          break;
      }
      Ray ray = new Ray(rayOrigin, rayDirection);

      // Draw Gizmo lines
      if (drawer != null) {
        //drawer.drawFingerRay(ray);
      }

      return ray;
    }

    void DrawProjectionRayArray() {
      RuntimeGizmoDrawer drawer = null;
      RuntimeGizmoManager.TryGetGizmoDrawer(out drawer);

      float distanceThreshold = .01f;
      Ray newRay = ProjectionRay(LassoRayType);
      centralRay = new Ray();
      Vector3 averageOrigin = Vector3.zero;
      Vector3 averageDirection = Vector3.zero;

      bool isTooClose = false;
      //is current finger point close to the origin of any others
      foreach (Ray r in selectionRays) {
        float distance = (r.origin - newRay.origin).magnitude;
        if (distance < distanceThreshold) isTooClose = true;
        averageOrigin += r.origin;
        averageDirection += r.direction;
        centralRay = new Ray(averageOrigin / selectionRays.Count, averageDirection);
        if (drawer != null) {
          //drawer.drawSelectionRay(r);
        }
      }
      if (!isTooClose) {
        selectionRays.Add(newRay);
        lassoPointList.Add(PlaneRayIntersection(newRay));
      }
      if (drawer != null) {
        //drawer.drawCentralRay(centralRay);
      }
      UpdateSelection();

      //Placing visual curve control points
      controlPoints[0].position = newRay.origin;
      controlPoints[1].position = newRay.GetPoint(RayRetractedDistance);
      controlPoints[2].position = newRay.GetPoint(hoverRayLength);
      controlPoints[3].position = PlaneRayIntersection(newRay);
      DrawVisualCurve(LassoRayColor);
    }

    private void DrawLasso(List<Vector3> points) {
      lassoPoints = points.ToArray();
      LineRendererLasso.SetVertexCount(points.Count);
      LineRendererLasso.SetPositions(lassoPoints);
    }


    private void UpdateSelection() {
      foreach (DistantSelectableBehaviour d in allSelectables) {
        //if (IsWithinClosest(d)) d.OnDistantSelect();

        Vector3 objectDirection = d.transform.position - centralRay.origin;

        Vector3 projectObjectToPlane = PlaneRayIntersection(new Ray(centralRay.origin, centralRay.origin + objectDirection * 10f));
        if (IsPointInPolygon(lassoPointList, projectObjectToPlane)) d.OnDistantSelect();

        else d.OnDistantDeselect();
      }
      DrawLasso(lassoPointList);
      SelectionTriangulator(selectionRays, lassoPointList);
    }

    private bool IsWithinClosest(DistantSelectableBehaviour d) {
      RuntimeGizmoDrawer drawer = null;
      RuntimeGizmoManager.TryGetGizmoDrawer(out drawer);
      Vector3 objectDirection = d.transform.position - centralRay.origin;
      float dotToObject = Vector3.Dot(centralRay.direction.normalized, objectDirection.normalized);
      bool selectTheObject = false;
      foreach (Ray r in selectionRays) {
        float dotToVertex = Vector3.Dot(centralRay.direction.normalized, r.direction.normalized);
        if (dotToObject > dotToVertex) {
          drawer.drawTrueRay(new Ray(centralRay.origin, objectDirection));
          selectTheObject = true;
        }
        else {
          //drawer.drawFalseRay(new Ray(centralRay.origin, objectDirection));
          selectTheObject = false;
        }
      }
      return selectTheObject;
    }


    //http://www.habrador.com/tutorials/math/9-useful-algorithms/

    //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
    public static bool IsPointInPolygon(List<Vector3> lassoPoints, Vector3 projectedObjPos) {
      List<Vector2> polygonPoints = new List<Vector2>(MyVector3Extension.toVector2Array(lassoPoints.ToArray<Vector3>()));
      Vector2 point = new Vector2(projectedObjPos.x, projectedObjPos.y);
      //Step 1. Find a point outside of the polygon
      //Pick a point with a x position larger than the polygons max x position, which is always outside
      Vector2 maxXPosVertex = polygonPoints[0];
      for (int i = 1; i < polygonPoints.Count; i++) {
        if (polygonPoints[i].x > maxXPosVertex.x) {
          maxXPosVertex = polygonPoints[i];
        }
      }
      //The point should be outside so just pick a number to make it outside
      Vector2 pointOutside = maxXPosVertex + new Vector2(10f, 0f);
      //Step 2. Create an edge between the point we want to test with the point thats outside
      Vector2 l1_p1 = point;
      Vector2 l1_p2 = pointOutside;
      //Step 3. Find out how many edges of the polygon this edge is intersecting
      int numberOfIntersections = 0;
      for (int i = 0; i < polygonPoints.Count; i++) {
        //Line 2
        Vector2 l2_p1 = polygonPoints[i];
        int iPlusOne = ClampListIndex(i + 1, polygonPoints.Count);
        Vector2 l2_p2 = polygonPoints[iPlusOne];
        //Are the lines intersecting?
        if (AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true)) {
          numberOfIntersections += 1;
        }
      }
      //Step 4. Is the point inside or outside?
      bool isInside = true;
      //The point is outside the polygon if number of intersections is even or 0
      if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0) {
        isInside = false;
      }
      return isInside;
    }

    public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints) {
      bool isIntersecting = false;
      float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);
      //Make sure the denominator is > 0, if not the lines are parallel
      if (denominator != 0f) {
        float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
        float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
        //Are the line segments intersecting if the end points are the same
        if (shouldIncludeEndPoints) {
          //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
          if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f) {
            isIntersecting = true;
          }
        }
        else {
          //Is intersecting if u_a and u_b are between 0 and 1
          if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f) {
            isIntersecting = true;
          }
        }
      }
      return isIntersecting;
    }
    //Clamp list indices
    //Will even work if index is larger/smaller than listSize, so can loop multiple times
    public static int ClampListIndex(int index, int listSize) {
      index = ((index % listSize) + listSize) % listSize;

      return index;
    }

    //==============================================================================================================================


    private void SelectorEvent(DistanSelectorFSM.Events newEvent) {
      DIfsm.ProcessEvent(newEvent);
      //OnPowerBallStateChange.Invoke((int)powerBallFSM.State);
      //PowerBallStateIndex = (int)powerBallFSM.State;
    }

    private void ClearSelection() {
      OnDeselectAll.Invoke();
      selectionRays = new List<Ray>();
      lassoPointList = new List<Vector3>();
    }
    private void VisualizeSelectionRays() { }

    class DistanSelectorFSM {
      public enum States { Standby, Drawing, Selected, Summoning, Held, Returning };
      public States State { get; set; }

      public enum Events { Standby, Draw, Select, Deselect, Summon, Hold, Dismiss };

      private Action[,] fsm;

      public DistanSelectorFSM() {
        this.fsm = new Action[6, 7]
        {
          //Standby,          Draw,              Select,             Deselect,           Summon,              Hold,               dismiss
          {null,              this.Draw,         this.Select,        this.DoNothing,     null,                this.Hold,          this.Dismiss},       //Standby
          {null,              null,              this.Select,        null,               null,                null,               null        },       //Drawing
          {null,              this.Draw,         this.Select,        this.Deselect,      this.Summon,         null,               null        },       //Selected
          {null,              null,              this.Standby,       null,               null,                this.Hold,          this.Dismiss},       //Summoning
          {this.Standby,      null,              null,               this.Standby,       null,                this.Hold,          this.Dismiss},       //Held
          {this.Standby,      null,              null,               this.Standby,       null,                this.DoNothing,     this.Dismiss}        //Returning
        };
      }

      public void ProcessEvent(Events theEvent) {
        if (this.fsm[(int)this.State, (int)theEvent] != null) {
          this.fsm[(int)this.State, (int)theEvent].Invoke();
          Debug.Log(this.State.ToString());
        }
        else Debug.LogError("Null Command - Current State: " + this.State.ToString() + " | Command: " + theEvent.ToString());
      }
      private void Standby() { this.State = States.Standby; }
      private void Draw() { this.State = States.Drawing; }
      private void Select() { this.State = States.Selected; }
      private void Deselect() { this.State = States.Standby; }
      private void Summon() { this.State = States.Summoning; }
      private void Hold() { this.State = States.Held; }
      private void Dismiss() { this.State = States.Returning; }
      private void DoNothing() { }
    }

    void DrawVisualCurve(Gradient rayColor) {
      VisualLineRenderer.colorGradient = rayColor;
      VisualLineRenderer.enabled = true;
      for (int i = 1; i <= segmentCount; i++) {
        float t = i / (float)segmentCount;
        Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints[0].position, controlPoints[1].position, controlPoints[2].position, controlPoints[3].position);
        VisualLineRenderer.SetVertexCount((segmentCount));
        VisualLineRenderer.SetPosition((i - 1), pixel);
      }
    }

    private void ResetVisualCurve() {
      VisualLineRenderer.colorGradient = SelectedRayColor;
      VisualLineRenderer.enabled = false;
    }



    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
      float u = 1 - t;
      float tt = t * t;
      float uu = u * u;
      float uuu = uu * u;
      float ttt = tt * t;

      Vector3 p = uuu * p0;
      p += 3 * uu * t * p1;
      p += 3 * u * tt * p2;
      p += ttt * p3;

      //return (1 - t) * (1 - t) * (1 - t) * p0 + 3 * (1 - t) * (1 - t) * t * p1 + 3 * (1 - t) * t * t * p2 + t * t * t * p3;
      return p;
    }


    public Vector3 PlaneRayIntersection(Ray ray) {
      Vector3 intersectionPoint = Vector3.zero;
      Vector3 planeNormal = -ProjectionPlane.up;
      float denominator = Vector3.Dot(ray.direction, planeNormal);
      if (denominator > 0.00001f) {
        float distanceToPlane = Vector3.Dot(ProjectionPlane.position - ray.origin, planeNormal) / denominator;

        intersectionPoint = ray.origin + ray.direction * distanceToPlane;
      }
      else {
        Debug.Log("No intersection");
      }
      return intersectionPoint;
    }

    public Transform SelectionMeshXform;
    Mesh selectionMesh;



    private void SelectionTriangulator(List<Ray> selectionRays, List<Vector3> lassoPointList) {
      List<int> newTriangles = new List<int>();
      List<Vector3> newVertices = new List<Vector3>();
      for(int i = 0; i < selectionRays.Count; i++) {
        newVertices.Add(selectionRays[i].origin);
        newVertices.Add(lassoPointList[i]);
      }
      for(int i = 0; i < newVertices.Count -2; i += 2) {
        //forward quad
        newTriangles.Add(i);
        newTriangles.Add(i + 1);
        newTriangles.Add(i + 2);

        newTriangles.Add(i + 2);
        newTriangles.Add(i + 1);
        newTriangles.Add(i + 3);

        //backward quad
        newTriangles.Add(i + 2);
        newTriangles.Add(i + 1);
        newTriangles.Add(i);

        newTriangles.Add(i + 3);
        newTriangles.Add(i + 1);
        newTriangles.Add(i + 2);
      }

      Vector2[] UVs = new Vector2[newVertices.Count];
      for(int i = 0; i < UVs.Length - 2; i += 2) {
        UVs[i]     = new Vector2(0, 0);
        UVs[i + 1] = new Vector2(0, 1);
        UVs[i + 2] = new Vector2(1, 0);
        UVs[i + 3] = new Vector2(1, 1);
      }
      
      selectionMesh.Clear();
      selectionMesh.vertices = newVertices.ToArray();
      selectionMesh.RecalculateBounds();
      selectionMesh.triangles = newTriangles.ToArray();
      selectionMesh.uv = UVs.ToArray();
      SelectionMeshXform.GetComponent<MeshFilter>().mesh = selectionMesh;
    }
  }

//-------------------------------------------------------------------------

  public class FingerDirectionAverage {
    private Queue<Vector3> samples = new Queue<Vector3>();
    private int sampleSize = 10;
    private Vector3 allVectors;
    public Vector3 Average { get; private set; }

    public void ComputeAverage(Vector3 newSample) {
      allVectors += newSample;
      samples.Enqueue(newSample);
      if (samples.Count > sampleSize) {
        allVectors -= samples.Dequeue();
      }
      Average = allVectors / samples.Count;
    }
  }

  public static class MyVector3Extension {
    public static Vector2[] toVector2Array(this Vector3[] v3) {
      return System.Array.ConvertAll<Vector3, Vector2>(v3, getV3fromV2);
    }

    public static Vector2 getV3fromV2(Vector3 v3) {
      return new Vector2(v3.x, v3.y);
    }
  }

  public static class GizmoExtensions {
    public static void drawFingerRay(this RuntimeGizmoDrawer drawer, Ray ray) {
      drawer.color = Color.cyan.WithAlpha(.2f);
      drawer.DrawLine(ray.origin, ray.origin + ray.direction * 7f);
    }

    public static void drawSelectionRay(this RuntimeGizmoDrawer drawer, Ray ray) {
      drawer.color = Color.white.WithAlpha(.05f);
      drawer.DrawLine(ray.origin, ray.origin + ray.direction * 7f);
      Debug.DrawRay(ray.origin, ray.direction * 10f);
    }
    public static void drawCentralRay(this RuntimeGizmoDrawer drawer, Ray ray) {
      drawer.color = Color.red;
      drawer.DrawLine(ray.origin, ray.origin + ray.direction * 7f);
    }
    public static void drawTrueRay(this RuntimeGizmoDrawer drawer, Ray ray) {
      drawer.color = Color.green.WithAlpha(.03f);
      drawer.DrawLine(ray.origin, ray.origin + ray.direction * 7f);
    }
    public static void drawFalseRay(this RuntimeGizmoDrawer drawer, Ray ray) {
      drawer.color = Color.magenta.WithAlpha(.01f);
      drawer.DrawLine(ray.origin, ray.origin + ray.direction * 7f);
    }
  }
}

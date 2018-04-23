using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class PullTabSelector : MonoBehaviour, IRuntimeGizmoComponent {

    [Header("Pull Tab")]
    public Transform pullTabOrigin;
    public IntObj pullTabIntObj;

    [Header("Marbles (each must implement ???Marble)")]
    [SerializeField]
    [ImplementsInterface(typeof(IGameObjectSequenceProvider))]
    private MonoBehaviour _marbleSequenceProvider;
    public IGameObjectSequenceProvider marbles {
      get {
        return _marbleSequenceProvider as IGameObjectSequenceProvider;
      }
    }

    public Transform nullMarble;

    public Transform activeMarbleParent;

    public int activeMarbleIdx = 0;

    public float marbleSpacing = 0.02f;

    public AnimationCurve openCloseCurve = DefaultCurve.SigmoidUp;


    [Header("Move Next Trigger")]
    [SerializeField, ImplementsInterface(typeof(ITrigger))]
    private MonoBehaviour _moveNextTrigger;
    public ITrigger moveNextTrigger {
      get {
        return _moveNextTrigger as ITrigger;
      }
    }



    [Header("LineRenderer")]
    public LineRenderer lineRenderer;

    [Header("Setting Hand Materials")]
    public Material colorSetMaterial;
    public Material backOfHandMaterial;
    public Material backOfHandMaterial2;
    public float colorSetLerpCoeff = 30f;

    [Header("Debug")]
    public bool drawDebug = true;

    private Vector3[] _marblePositions;
    private Vector3[] _openMarblePositions;
    private Vector3[] _closedMarblePositions;
    private Vector3[] _lineRendererPositions;

    private bool _isListOpen = false;
    private float _listOpenCloseAmount = 0f; // 0 is closed
    public float listOpenCloseAmount { get { return _listOpenCloseAmount; } }

    private int _preGrabMarbleIdx;

    private Color _targetSetMaterialColor;

    //private Vector3 _lastPullTabEndDir = Vector3.right;

    private void OnValidate() {
      if (colorSetMaterial != null) {
        _targetSetMaterialColor = colorSetMaterial.color;
      }
    }

    private void Start() {
      pullTabIntObj.OnGraspStay += setPullTabNormal;
    }

    private void setPullTabNormal() {
      pullTabIntObj.rigidbody.rotation = pullTabOrigin.transform.rotation;

      if (_isListOpen) {
        pullTabIntObj.rigidbody.rotation = Quaternion.FromToRotation(
                                             -(pullTabIntObj.rigidbody.rotation * Vector3.right),
                                             _lineRendererPositions[_lineRendererPositions.Length - 1]
                                               .From(_lineRendererPositions[_lineRendererPositions.Length - 2]).normalized)
                                           * pullTabIntObj.rigidbody.rotation;
      }
    }

    private void Update() {
      if (pullTabIntObj.isGrasped) {
        if (!_isListOpen) {
          _preGrabMarbleIdx = activeMarbleIdx;
        }
        _isListOpen = true;
      }
      else {
        _isListOpen = false;
      }

      updateMarbles();
      
      // gross
      if (!_isListOpen) {
        pullTabIntObj.rigidbody.rotation = pullTabOrigin.transform.rotation;
        pullTabIntObj.transform.rotation = pullTabOrigin.transform.rotation;
      }
    }

    private void updateMarbles() {
      if (_marblePositions == null || _marblePositions.Length != marbles.Count) {
        _marblePositions = new Vector3[marbles.Count + 2];
        _openMarblePositions = new Vector3[_marblePositions.Length];
        _closedMarblePositions = new Vector3[_marblePositions.Length];
        _lineRendererPositions = new Vector3[marbles.Count + 3];
      }

      fillOpenMarblePositions(_openMarblePositions);

      fillClosedMarblePositions(_closedMarblePositions);

      updateMarblePositions();

      updateSetMaterials();
    }

    private void updateMarblePositions() {

      // Open/close animation
      float targetOpenCloseProgress = (_isListOpen ? 1 : 0);
      _listOpenCloseAmount = Mathf.Lerp(_listOpenCloseAmount, targetOpenCloseProgress, 8f * Time.deltaTime);
      var effCloseOpenAmount = openCloseCurve.Evaluate(_listOpenCloseAmount);

      for (int i = 0; i < _marblePositions.Length; i++) {
        _marblePositions[i] = Vector3.Lerp(_closedMarblePositions[i], _openMarblePositions[i], effCloseOpenAmount);
      }

      // List marble placement
      // TODO: Null 'marble' placement at 0
      for (int i = 0; i < _marblePositions.Length; i++) {
        if (i > 0 && (i - 1) < marbles.Count) {
          var marbleTransform = marbles[i - 1].transform;
          marbleTransform.position = _marblePositions[i];
        }

        // (Line renderer between marbles)
        _lineRendererPositions[i] = _marblePositions[i];
      }

      // "Active Marble" placement
      if (_listOpenCloseAmount > 0.95f) {
        updateActiveMarbleIdx(_marblePositions);
      }
      else if (_listOpenCloseAmount < 0.10f) {
        if (moveNextTrigger.didFire) {
          activeMarbleIdx += 1;
          activeMarbleIdx %= marbles.Count;
        }
      }

      var activeMarbleInList = marbles[activeMarbleIdx];
      if (activeMarbleParent.childCount == 0) {
        activeMarbleInList = loadActiveMarble(activeMarbleIdx);
      }
      var powerHandPlaceholder = activeMarbleParent.GetChild(0).GetComponent<PowerHandPlaceholder>();
      if (powerHandPlaceholder == null || powerHandPlaceholder.powerIndex != activeMarbleIdx) {
        unloadActiveMarble();

        activeMarbleInList = loadActiveMarble(activeMarbleIdx);
      }
      var actualActiveMarble = activeMarbleParent.GetChild(0);
      actualActiveMarble.transform.position = _marblePositions[_marblePositions.Length - 1];

      // "Null" marble placement
      if (nullMarble != null) {
        nullMarble.transform.position = _marblePositions[0];
      }

      // Last two line renderer positions (active marble, pull tab)
      _lineRendererPositions[_lineRendererPositions.Length - 2] = _marblePositions[_marblePositions.Length - 1];
      _lineRendererPositions[_lineRendererPositions.Length - 1] = pullTabIntObj.transform.position;

      if (lineRenderer != null) {
        lineRenderer.positionCount = _lineRendererPositions.Length;
        lineRenderer.SetPositions(_lineRendererPositions);
      }

      // Also update scale of the marbles
      for (int i = 0; i < marbles.Count; i++) {
        float scaleMult = marbles[i].transform.position.From(this.transform.position).sqrMagnitude.Map(0.02f * 0.02f, 0f, 1f, 1.2f);

        marbles[i].transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, effCloseOpenAmount) * scaleMult;
      }
      if (nullMarble != null) {
        nullMarble.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, effCloseOpenAmount);
      }
      if (activeMarbleParent.childCount > 0) {
        activeMarbleParent.GetChild(0).transform.localScale = Vector3.one;
      }
    }

    private void updateActiveMarbleIdx(Vector3[] marblePositions) {
      float closestSqrDist = float.PositiveInfinity;
      int closestMarbleIdx = -1;
      for (int i = 1; i < marblePositions.Length - 1; i++) {
        var testSqrDist = (marblePositions[i] - this.transform.position).sqrMagnitude;
        if (testSqrDist < closestSqrDist || closestMarbleIdx == -1) {
          closestMarbleIdx = i;
          closestSqrDist = testSqrDist;
        }
      }

      bool ignore = false;
      var activeSqrDist = (marblePositions[marblePositions.Length - 1] - this.transform.position).sqrMagnitude;
      if (activeSqrDist < closestSqrDist) {
        closestSqrDist = _preGrabMarbleIdx;
        ignore = true;
      }

      var nullSqrDist = (marblePositions[0] - this.transform.position).sqrMagnitude;
      if (nullSqrDist < closestSqrDist) {
        activeMarbleIdx = _preGrabMarbleIdx;
        return;
      }

      if (!ignore) {
        activeMarbleIdx = closestMarbleIdx - 1 /* minus 1 because there's an extra slot at "0" for "no marble" */;
      }
    }

    private void unloadActiveMarble() {
      Destroy(activeMarbleParent.GetChild(0).gameObject);
    }

    private GameObject loadActiveMarble(int index) {
      var activeMarbleClone = Instantiate(marbles[index], activeMarbleParent);
      activeMarbleClone.transform.localPosition = Vector3.zero;
      activeMarbleClone.transform.localScale = Vector3.zero;

      if (colorSetMaterial != null) {
        var renderer = activeMarbleClone.GetComponentInChildren<Renderer>();
        if (renderer != null) {
          var color = renderer.sharedMaterial.color;

          _targetSetMaterialColor = color;
        }
      }

      return activeMarbleClone;
    }

    private void updateSetMaterials() {
      //Color? colorFromColorSetMaterial = null;
      if (colorSetMaterial != null) {
        //colorFromColorSetMaterial = 
        colorSetMaterial.color
                                  = Color.Lerp(colorSetMaterial.color,
                                               _targetSetMaterialColor,
                                               colorSetLerpCoeff * Time.deltaTime);
      }
      if (backOfHandMaterial != null) {
        var activeMarbleObj = GetActiveMarbleObj();
        if (activeMarbleObj != null) {
          backOfHandMaterial.mainTexture = activeMarbleObj.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture;
          backOfHandMaterial2.mainTexture = activeMarbleObj.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture;

          // Woops nevermind, we don't care about the color of the texture on the hand!
          //if (colorFromColorSetMaterial != null) {
          //  backOfHandMaterial.color = colorFromColorSetMaterial.Value;
          //}
          //else {
          //  backOfHandMaterial.color = Color.Lerp(backOfHandMaterial.color,
          //                                        _targetSetMaterialColor,
          //                                        colorSetLerpCoeff * Time.deltaTime);
          //}
        }
      }
    }

    private GameObject GetActiveMarbleObj() {
      if (activeMarbleParent != null && activeMarbleParent.childCount != 0) {
        return activeMarbleParent.GetChild(0).gameObject;
      }

      return null;
    }

    private void fillOpenMarblePositions(Vector3[] marblePositions) {

      var pullTabLength = this.transform.position.From(pullTabOrigin.transform.position).magnitude;

      var pullLength = pullTabIntObj.transform.position.From(pullTabOrigin.position).magnitude;

      var pullOutDir = pullTabOrigin.transform.right * -1;

      var curveStart = this.transform.position;
      Vector3 curveEnd = pullTabIntObj.transform.position;
      var controlPoint = curveStart + (pullOutDir * pullLength / 3f);

      var curveLength = estimateCurveLength(curveStart, curveEnd, controlPoint);

      //var lengthBetweenMarbles = marbleSpacing;
      var totalStripLength = marbleSpacing * (marblePositions.Length - 1 + 1);

      var beginPoint = curveStart;
      if (totalStripLength > curveLength) {
        beginPoint += (totalStripLength - curveLength) * -pullOutDir;
      }

      var tFraction_AllMarbles = 1 / (float)(marblePositions.Length - 1 + (1 /* leave extra space at end of curve */));

      var curveStripLenRatio = totalStripLength / curveLength;

      var tStart = 1 - (1 * curveStripLenRatio);
      var tStep = tFraction_AllMarbles * curveStripLenRatio;

      if (tStep < 0.001f) {
        Debug.LogError("Bad tStep!! " + tStep, this);
        tStep = 0.001f;
      }

      RuntimeGizmoDrawer drawer = null;
      {
        RuntimeGizmoManager.TryGetGizmoDrawer(out drawer);
      }

      if (drawer != null && drawDebug) {
        drawer.color = LeapColor.forest;
        drawer.DrawWireSphere(beginPoint, 0.04f);

        drawer.color = LeapColor.blue;
        drawer.DrawWireSphere(curveStart, 0.04f);
        drawer.DrawWireSphere(curveEnd, 0.04f);
        drawer.DrawWireSphere(controlPoint, 0.04f);
      }
      
      var t = tStart;
      if (t > 0) t = 0;
      for (int i = 0; i < marblePositions.Length; i++) {
        Vector3 marblePos;
        if (t < 0) {
          marblePos = beginPoint + pullOutDir * (marbleSpacing * i);
        }
        else {
          marblePos = evalCurve(curveStart, curveEnd, controlPoint, t);
        }
        marblePositions[i] = marblePos;

        if (drawer != null && drawDebug) {
          drawer.color = Color.white;
          drawer.DrawWireSphere(marblePos, 0.03f);
        }

        t += tStep;
      }

    }

    private Vector3 evalCurve(Vector3 curveStart, Vector3 curveEnd, Vector3 controlPoint, float t) {
      var cs = curveStart - controlPoint;
      var ce = curveEnd - controlPoint;
      var it = (1 - t);
      return controlPoint + cs * (it * it) + ce * (t * t);
    }

    private float estimateCurveLength(Vector3 curveStart, Vector3 curveEnd, Vector3 controlPoint,
                                 int numDivisions = 64) {
      var sum = 0f;
      Vector3? lastEval = null;
      var tFraction = 1 / (float)numDivisions;
      for (int i = 0; i <= numDivisions; i++) {
        var t = i * tFraction;
        if (!lastEval.HasValue) {
          lastEval = evalCurve(curveStart, curveEnd, controlPoint, t);
        }
        else {
          var eval = evalCurve(curveStart, curveEnd, controlPoint, t);
          sum += (lastEval.Value - eval).magnitude;
          lastEval = eval;
        }
      }

      return sum;
    }

    private void fillClosedMarblePositions(Vector3[] closedMarblePositions) {
      for (int i = 0; i < _closedMarblePositions.Length - 1; i++) {
        closedMarblePositions[i] = this.transform.position;
      }
      closedMarblePositions[closedMarblePositions.Length - 1] = this.transform.position;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {

    }

  }

}

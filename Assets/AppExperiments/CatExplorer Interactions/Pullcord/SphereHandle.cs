using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.Attributes;
using Leap.Unity.Gestures;
using TMPro;
//using DG.Tweening;
//using TMPro;
//using Klak.Math;
//using Klak.VectorMathExtension;


public class SphereHandle : MonoBehaviour {


	//When user releases SphereHandle, PullcordController sets the destination restingPos to either endpoint (0-1) and this script handles the animation to restingPos

	public Transform restingPos;
	public float threshold = 0.09f;
	public SphereHandle other;
	public MeshRenderer rim;
	public Transform hint;
	public AudioSource audioSource;

	[HideInInspector]
	public bool isInHand = false;

	[HideInInspector]
	public bool isPinched = false;

	[HideInInspector]
	public bool lockedToRestingPos = false;

	[HideInInspector]
	public Leap.Hand pinchingHand = null;
	
	private InteractionBehaviour intBeh;
	private MeshRenderer pillRend;
	private LineRenderer lineRenderer;
 
	private Color initialColor, initialLineColor, initialRimColor;
	private Vector3 initialScale, initialLocalPos, initialHintScale;
	private bool showhint = true;
	private Vector3 spherePositionTarget;
	private int numberOfTimesToShowTheHint = 1;

	[HideInInspector]
	public static int npinches = 0;

	[HideInInspector]
	public State state = State.Default;

	[HideInInspector]
	public float currentVelocity = 0;

	private Vector3 prevPos = Vector3.zero;

	[HideInInspector]
	public enum State {
		Default,
		Hovered,
		Pinched,
		Inactive
	}

	//private UIParams UIParams;

	public Action OnPinchStart, OnPinchEnd;

	private float springSpeed = 100f;	
	Vector3 _vposition;

	public bool independentHint = false;
	private int myNPinches = 0;

	private void OnEnable() {
		intBeh = GetComponent<InteractionBehaviour>();
		
	}

	private float stretchLimit = 0.6f;

	private PinchGesture PinchL, PinchR;

	void Awake () {
		//UIParams = GameObject.FindWithTag("Manager").GetComponent<Manager>().UIParams;		
		lineRenderer = GetComponent<LineRenderer>();
		pillRend = transform.GetChild(0).GetComponent<MeshRenderer>();

		PinchL = GameObject.Find("PinchL").GetComponent<PinchGesture>();
		PinchR = GameObject.Find("PinchR").GetComponent<PinchGesture>();

		if(hint == null)
			hint = transform.GetComponentInChildren<TextMeshPro>() ? transform.GetComponentInChildren<TextMeshPro>().transform : null;

		initialColor = pillRend.material.color;
		if(rim != null) initialRimColor = rim.material.color;
		initialScale = transform.localScale;
		initialLocalPos = transform.localPosition;
		initialLineColor = lineRenderer.material.color;

		if(hint != null)
			initialHintScale = hint.localScale;

		//HideHint();
		
		spherePositionTarget = restingPos.position;
	}

	public void HideSphere(){
		pillRend.enabled = false;
	}

	public void ShowSphere(){
		pillRend.enabled = true;		
	}

	void Update () {
		
		spherePositionTarget = restingPos.position;
			
		if(state != State.Inactive){
			if(!isPinched && intBeh.isPrimaryHovered ){
				
				Vector3 midpoint = Midpoint(intBeh.primaryHoveringHand);
				float sqrDistToHand = (restingPos.position - midpoint).sqrMagnitude;//midpoint.SqrDist(restingPos.position);
				
				if(sqrDistToHand < threshold*threshold){
					spherePositionTarget = midpoint;
					if(state != State.Hovered) SetHover();

					if(Pinching(intBeh.primaryHoveringHand)){
						pinchingHand = intBeh.primaryHoveringHand;
						isPinched = true;
						if(state != State.Pinched) SetPinch();
					}
				} else {
					spherePositionTarget = restingPos.position;					
					if(state != State.Default) SetDefault();
				}
			}

			if(pinchingHand != null){
				
				//check if pinching hand is still there

				bool pinchingHandIsInView = false;
				int nHands = Hands.Provider.CurrentFrame.Hands.Count;

				if(nHands > 0){
					if(nHands > 1){
						if(pinchingHand.IsLeft == Hands.Provider.CurrentFrame.Hands[0].IsLeft || pinchingHand.IsLeft == Hands.Provider.CurrentFrame.Hands[1].IsLeft) pinchingHandIsInView = true;
					} else {
						if(pinchingHand.IsLeft == Hands.Provider.CurrentFrame.Hands[0].IsLeft) pinchingHandIsInView = true;
					}
				}

				float d = (restingPos.position - transform.position).sqrMagnitude;//transform.position.SqrDist(restingPos.position);
				
							

				if(Pinching(pinchingHand) && pinchingHandIsInView && d < stretchLimit*stretchLimit){
					spherePositionTarget = Midpoint(pinchingHand);
				} else {
					//back to non-pinched state
					Vector3 midpoint = Midpoint(pinchingHand);
					float sqrDistToHand = (restingPos.position - midpoint).sqrMagnitude;//midpoint.SqrDist(restingPos.position);

					if(sqrDistToHand < threshold*threshold){
						spherePositionTarget = midpoint;
						if(state != State.Hovered) SetHover();
					} else {
						spherePositionTarget = restingPos.position;
						if(state != State.Default) SetDefault();
					}

					isPinched = false;
					pinchingHand = null;
				}
			}

		}

		// if(!lockedToRestingPos)
			// transform.position += (spherePositionTarget - transform.position) * 0.1f * Time.deltaTime * 100;
		transform.position = SpringPosition(transform.position, spherePositionTarget);

		// gameObject.MotionP3().AimSpringAt(spherePositionTarget, ratio, bounce);
			
		// else
		// 	transform.position = spherePositionTarget;

		
	}

	private Vector3 Step( Vector3 current, Vector3 target, float t)
	{
		var e = Mathf.Exp(-t * Time.deltaTime);
		return Vector3.Lerp(target, current, e);
	}

	Vector3 SpringPosition(Vector3 current, Vector3 target)
	{
		_vposition = Step(_vposition, Vector3.zero, springSpeed * 0.35f);
		_vposition += (target - current) * (springSpeed * 0.1f);
		return current + _vposition * Time.deltaTime;
	}

	void SetHover(){
		state = State.Hovered;
		if(OnPinchEnd != null) OnPinchEnd();

		// DOTween.Pause("sphere");
		// DOTween.Kill("sphere");

		//transform.DOScale(initialScale * 1.3f, 0.2f);//.SetId("sphere");
		transform.localScale = initialScale*1.3f;

		Color half = Color.Lerp(initialLineColor, Color.white,0.5f);

		//pillRend.material.DOColor(half, 0.1f);
		pillRend.material.color = half;
		//lineRenderer.material.DOColor(half, 0.1f);
		lineRenderer.material.color = half;
		//if(rim != null) rim.material.DOColor(initialRimColor, 0.1f);
		if (rim != null) rim.material.color = initialRimColor;

		if (showhint && ((npinches < numberOfTimesToShowTheHint+1) || (independentHint && myNPinches < numberOfTimesToShowTheHint+1))) 
			ShowHint();
	}

	void SetPinch(){
		state = State.Pinched;
		if(OnPinchStart != null) OnPinchStart();
		// DOTween.Pause("sphere");
		// DOTween.Kill("sphere");
			
		//transform.DOScale(initialScale * 0.3f, 0.2f);//.SetId("sphere");
		transform.localScale = initialScale * 0.3f;
		//pillRend.material.DOColor(Color.white, 0.1f);//.SetId("sphere");
		pillRend.material.color = Color.white;
		//lineRenderer.material.DOColor(Color.white, 0.1f);//.SetId("sphere");
		lineRenderer.material.color = Color.white;

		//if (rim != null) rim.material.DOColor(Color.white, 0.1f);//.SetId("sphere");
		if (rim != null) rim.material.color = Color.white;

		//audioSource.PlayOneShot(UIParams.spherePinch);

		HideHint();
		
	}

	public void SetDefault(){
		state = State.Default;
		if(OnPinchEnd != null) OnPinchEnd();
		
		spherePositionTarget = restingPos.position;					
		isPinched = false;

		// DOTween.Pause("sphere");
		// DOTween.Kill("sphere");

		//if(rim != null) rim.material.DOColor(initialRimColor, 0.1f);//.SetId("sphere");
		if (rim != null) rim.material.color = initialRimColor;
		//transform.DOScale(initialScale, 0.2f);//.SetId("sphere");
		transform.localScale = initialScale;
		//pillRend.material.DOColor(initialColor, 0.1f);//.SetId("sphere");
		pillRend.material.color = initialColor;
		//lineRenderer.material.DOColor(initialLineColor, 0.1f);//.SetId("sphere");
		lineRenderer.material.color = initialLineColor;

		HideHint();
	}

	private Vector3 Midpoint(Leap.Hand hand){
		Vector3 indexPos = hand.GetIndex().TipPosition.ToVector3();
		Vector3 thumbPos = hand.GetThumb().TipPosition.ToVector3();
		return (indexPos + thumbPos)/2;
	}

	private bool Pinching(Leap.Hand hand){
		Vector3 indexPos = hand.GetIndex().TipPosition.ToVector3();
		Vector3 thumbPos = hand.GetThumb().TipPosition.ToVector3();

		
		bool isPinching = false;
		if (hand.IsLeft) isPinching = PinchL.isActive;
		else isPinching = PinchR.isActive;
		

		//bool isPinching = hand.IsPinching();

		//return hand.IsPinching() && (indexPos.SqrDist(thumbPos) < 0.0009f);
		return isPinching;
	}

	void HideHint(){
		//hint.DOScale(Vector3.zero, 0.2f);
		if(hint!=null)
			hint.localScale = Vector3.zero;
	}

	void ShowHint(){
		npinches++;	
		myNPinches++;
		//hint.DOScale(initialHintScale, 0.2f);
		if (hint != null)
			hint.localScale = initialHintScale;
	}

}

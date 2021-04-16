using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullcordController : MonoBehaviour
{
	//PullcordController assigns pullcordValue to 0 to 1 value for how far down user pulls the SphereHandle.
	//PullcordController also sets the SphereHandle's restingPos to either defaultPos or extensionPos (i.e. the endpoints) based on distance

	public float pullcordValue = 0;

	public Transform defaultPos;
	public Transform extensionPos;
	public Transform point;

	public AudioClip switchOn, switchOff;

	[HideInInspector]
	public bool clicked = false;

	private float maxDist;
	private LineDebugger lineDebugger;
	private float initalLength;
	private SphereHandle sphereHandle;
	private Vector3 initialPointScale;
	private MeshRenderer pointRend;
	private Color initalPointColor;
	private AudioSource audioSource;

	private Rigidbody rb;
	private float currentYVelocity = 0;
	private float prevPosY;
	private float thresholdYVelocity = -0.01f;

	private bool startingFromOFF = true;

	// Use this for initialization
	void Awake()
	{
		sphereHandle = GetComponent<SphereHandle>();
		lineDebugger = GetComponent<LineDebugger>();
		initalLength = (lineDebugger.pointB.position - lineDebugger.pointA.position).sqrMagnitude;//lineDebugger.pointA.position.SqrDist(lineDebugger.pointB.position);
		pointRend = point.GetComponent<MeshRenderer>();
		audioSource = point.GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody>();

		maxDist = defaultPos.position.y - extensionPos.position.y;
		initialPointScale = point.localScale;
		initalPointColor = pointRend.material.color;

		extensionPos.GetComponent<MeshRenderer>().material.color = Color.white;
		extensionPos.GetComponent<LineRenderer>().enabled = false;
		extensionPos.GetComponent<MeshRenderer>().enabled = false;

		prevPosY = transform.position.y;

	}

	private void OnEnable()
	{
		sphereHandle.OnPinchEnd += Release;
		sphereHandle.OnPinchStart += PinchStart;
	}

	private void OnDisable()
	{
		sphereHandle.OnPinchEnd -= Release;
		sphereHandle.OnPinchStart -= PinchStart;
	}

	void PinchStart()
	{
		float midpoint = (defaultPos.position.y + extensionPos.position.y) * 0.5f + 0.2f * (extensionPos.position.y - defaultPos.position.y);

		if (transform.position.y > midpoint)
		{
			startingFromOFF = true;
		}
		else
		{
			startingFromOFF = false;
		}
	}

	void Release()
	{

		if (currentYVelocity < thresholdYVelocity)
		{
			clicked = true;
			ShowClick();
			sphereHandle.restingPos = extensionPos;
		}
	}

	// Update is called once per frame
	void Update()
	{

		currentYVelocity = transform.position.y - prevPosY;

		float positionDelta = Mathf.Abs(transform.localPosition.x - defaultPos.localPosition.x);

		if (positionDelta >= 0.01f)
		{
			ManagePullcord();
		}

		float midpoint = (defaultPos.position.y + extensionPos.position.y) * 0.5f + 0.2f * (extensionPos.position.y - defaultPos.position.y);
		float bottompoint = extensionPos.position.y - 0.05f;

		if (sphereHandle.isPinched)
		{
			extensionPos.GetComponent<LineRenderer>().enabled = true;
			extensionPos.GetComponent<MeshRenderer>().enabled = true;

			if ((transform.position.y <= midpoint && transform.position.y >= bottompoint) && !clicked)
			{
				clicked = true;
				ShowClick();
				sphereHandle.restingPos = extensionPos;
			}

			if ((transform.position.y > midpoint || (transform.position.y < bottompoint && !startingFromOFF)) && clicked)
			{
				clicked = false;
				HideClick();
				sphereHandle.restingPos = defaultPos;
			}
		}
		else
		{
			extensionPos.GetComponent<LineRenderer>().enabled = false;
			extensionPos.GetComponent<MeshRenderer>().enabled = false;

		}

		prevPosY = transform.position.y;

	}


	void ShowClick()
	{
		pointRend.material.color = Color.white;
		audioSource.PlayOneShot(switchOn);
	}

	void HideClick()
	{
		pointRend.material.color = initalPointColor;
		audioSource.PlayOneShot(switchOff);
	}

	void ManagePullcord()
	{

		float currentDelta = Mathf.InverseLerp(defaultPos.localPosition.x, extensionPos.localPosition.x, transform.localPosition.x);
		currentDelta = Mathf.Clamp01(Mathf.Abs(currentDelta));

		if(currentDelta != pullcordValue){
			pullcordValue = currentDelta;

			Debug.Log("UPDATED PULLCORD VALUE TO: " + pullcordValue);
		}
		
	}
}

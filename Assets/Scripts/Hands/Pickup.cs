using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class Pickup : MonoBehaviour {

	SteamVR_TrackedObject trackObj;
	SteamVR_Controller.Device device;

	void Awake () 
	{
		trackObj = GetComponent<SteamVR_TrackedObject> ();
	}

	void FixedUpdate () 
	{
		device = SteamVR_Controller.Input((int)trackObj.index);
		if (device.GetTouch (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Held");
		}
		if (device.GetTouchDown (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Held Down");
		}/*
		if (device.GetTouchUp (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Held Up");
		}
		if (device.GetPress (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Press");
		}
		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Press Down");
		}
		if (device.GetPressUp (SteamVR_Controller.ButtonMask.Trigger)) 
		{
			Debug.Log ("Press Up");
		}
		*/

	}

	void OnTriggerStay (Collider col)
	{
		if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
		{
			Debug.Log ("Collided with the" + col.name);
			col.attachedRigidbody.isKinematic = true;
			col.gameObject.transform.SetParent (this.gameObject.transform);
		}
		if (device.GetTouchUp (SteamVR_Controller.ButtonMask.Trigger)) {
			col.gameObject.transform.SetParent (null);
			col.attachedRigidbody.isKinematic = false;

			tossObject (col.attachedRigidbody);
		}
	}

	void tossObject(Rigidbody rigidBody)
	{	
		Transform origin = trackObj.origin ? trackObj.origin : trackObj.transform.parent;

		if (origin != null) 
		{
			rigidBody.velocity = origin.TransformVector (device.velocity);
			rigidBody.angularVelocity = origin.TransformVector (device.angularVelocity);
		}
		else 
		{
			rigidBody.velocity = device.velocity;
			rigidBody.angularVelocity = device.angularVelocity;
		}

	}
}

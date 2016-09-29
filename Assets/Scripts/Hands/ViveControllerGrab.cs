using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ViveControllerGrab : MonoBehaviour
{
    public GameObject currentlySelectedObject;
    public bool isLeftHand;
    public bool isGrabbing = false;
    
    private SteamVR_TrackedObject trackedObj;
    private FixedJoint grabJoint;
    private bool grabbed = false;
    private GameObject jointPoint = null;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {

    }

    void Update()
    {

        // Get the controller device.
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        // If something is selected.
        if (currentlySelectedObject != null)
        {

            // If there isn't a joint yet on the selected object.
            if (grabJoint == null && device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
            {

                Connect();
            }

            // If a joint does exist and the user releases the button
            else if (grabJoint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                // Disconnect the joint and store rigid body.
                Rigidbody rigidbody = Disconnect();

                var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
                if (origin != null)
                {
                    rigidbody.velocity = origin.TransformVector(device.velocity);
                    rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity) * 0.01f;

                }
                else
                {
                    rigidbody.velocity = device.velocity;
                    rigidbody.angularVelocity = device.angularVelocity * 0.01f;
                }

                rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

            }
        }

        if (isLeftHand)
        {
            //if (device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
            //{
            //}
        }
        else
        {
        }

    }

    public void OnTriggerStay(Collider other)
    {
        // If the controller comes into contact with a selectable object.
        if (other.GetComponent<ViveGrabbable>() != null && !isGrabbing)
        {
            // If an object is already selected.
            if (currentlySelectedObject != null && other.gameObject == currentlySelectedObject)
            {
                // Deselect current object.
                currentlySelectedObject.GetComponent<ViveGrabbable>().OnHoverLeave();
            }

            currentlySelectedObject = other.gameObject;
            other.GetComponent<ViveGrabbable>().OnHover();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ViveGrabbable>() && !isGrabbing)
        {
            other.GetComponent<ViveGrabbable>().OnHoverLeave();
            if (other.gameObject == currentlySelectedObject)
            {
                currentlySelectedObject = null;
            }
        }
    }

    public void Connect()
    {
        // Object is now being grabbed
        isGrabbing = true;

        // Highlight the grabbed object.
        var viveGrabbable = currentlySelectedObject.GetComponent<ViveGrabbable>();
        if (viveGrabbable != null)
        {
            viveGrabbable.isActive = true;
            viveGrabbable.OnGrab();
        }

        jointPoint = new GameObject();
        jointPoint.name = "Joint Point";
        jointPoint.transform.parent = this.transform;
        jointPoint.transform.position = this.GetComponent<Collider>().transform.position;
        jointPoint.AddComponent<Rigidbody>().isKinematic = true;


        // Add a joint to the object for grab.
        grabJoint = currentlySelectedObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = jointPoint.GetComponent<Rigidbody>();

        currentlySelectedObject.GetComponent<Rigidbody>().useGravity = false;

    }

    public Rigidbody Disconnect()
    {
        var viveGrabbable = currentlySelectedObject.GetComponent<ViveGrabbable>();
        if (viveGrabbable != null)
        {
            viveGrabbable.isActive = false;
        }

        isGrabbing = false;
        Rigidbody rigidbody = grabJoint.GetComponent<Rigidbody>();
        Object.Destroy(grabJoint);
        Destroy(jointPoint);
        grabJoint = null;

        currentlySelectedObject.GetComponent<Rigidbody>().useGravity = true;
        currentlySelectedObject = null;

        return rigidbody;
    }
}

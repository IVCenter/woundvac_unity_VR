using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(GrabHighlight), typeof(Rigidbody), typeof(Collider))]
public class ViveGrabbable : NetworkBehaviour
{

    [SyncVar]
    public bool highlightObject = false;

    public bool isTrainee = false; // If is Trainee side then disable gravity on start

    public bool isGrabbed = false; // This solves object jitter across network cuz it disables gravity when object is grabbed by the player

    private Rigidbody rigid;

    public void OnHover()
    {
        highlightObject = true;
    }

    public void OnHoverLeave()
    {
        highlightObject = false;
    }

    public virtual void OnGrab()
    {
        highlightObject = false;
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        if (isTrainee) rigid.useGravity = false; // don't use gravity for Trainee
    }

    void Update()
    {
        if (!isTrainee)
        {
            if (isGrabbed)
            {
                if (rigid.useGravity)
                {
                    rigid.useGravity = false;
                }
            }
            else
            {
                if (!rigid.useGravity)
                {
                    rigid.useGravity = true;
                }
            }
        }

        if (highlightObject)
        {
            GetComponent<GrabHighlight>().DrawOutline();
        }
        else
        {
            GetComponent<GrabHighlight>().EraseOutline();
        }
    }
}

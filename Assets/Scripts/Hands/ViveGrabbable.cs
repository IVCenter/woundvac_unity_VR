using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(GrabHighlight), typeof(Rigidbody), typeof(Collider))]
public class ViveGrabbable : NetworkBehaviour
{
    [SyncVar]
    public bool isGrabbed = false;

    [SyncVar]
    public bool highlightObject = false;

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
    }

    void Update()
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

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(GrabHighlight), typeof(Rigidbody), typeof(Collider))]
public class ViveGrabbable : NetworkBehaviour
{
    [SyncVar]
    public bool isActive = false;

    private Rigidbody rigid;

    public void OnHover()
    {
        GetComponent<GrabHighlight>().makeTransparent();
    }

    public void OnHoverLeave()
    {
        GetComponent<GrabHighlight>().makeOpaque();
    }

    public virtual void OnGrab()
    {
        GetComponent<GrabHighlight>().makeOpaque();
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isActive)
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
}

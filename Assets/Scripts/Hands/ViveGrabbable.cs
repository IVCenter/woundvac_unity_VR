using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GrabHighlight), typeof(Rigidbody), typeof(Collider))]
public class ViveGrabbable : MonoBehaviour {

    public bool isActive = false;

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

	void Start () {
	
	}
	
	void Update () {
	
	}
}

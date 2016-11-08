using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncHands : NetworkBehaviour {

    private Transform viveHandLeft;
    private SteamVR_Controller.Device leftHandDevice;


    private Animator anim;

    public enum GESTURE
    {
        IDLE,
        POINT,
        GRAB,
    }

    [SyncVar]
    public GESTURE gesture = GESTURE.IDLE;

    void Awake ()
    {
        anim = GetComponent<Animator>();
    }

    void Start () {
	    
	}
	
    private void AnimateHand()
    {
        switch (gesture)
        {
            case GESTURE.IDLE:
                anim.SetBool("Point", false);
                anim.SetBool("GripBall", false);
                anim.SetBool("Idle", true);
                break;
            case GESTURE.POINT:
                //anim.SetBool("GripBall", false);
                //anim.SetBool("Idle", false);
                anim.SetBool("Point", true);
                break;

            case GESTURE.GRAB:
                //anim.SetBool("Point", false);
                //anim.SetBool("Idle", false);
                anim.SetBool("GripBall", true);
                break;
            default:
                break;
        }
    }

	void LateUpdate () {


        if (isServer)
        {
            if (viveHandLeft != null)
            {
                this.transform.position = viveHandLeft.position;
                this.transform.rotation = viveHandLeft.rotation;

                if (leftHandDevice.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
                {
                    gesture = GESTURE.POINT;
                }
                else if (leftHandDevice.GetPress(SteamVR_Controller.ButtonMask.Trigger))
                {
                    gesture = GESTURE.GRAB;
                }
                else
                {
                    gesture = GESTURE.IDLE;
                }

            }
            else
            {
                Debug.Log("Looking for LeftHand");
                var go = GameObject.Find("HandLeftPos");
                if (go)
                {
                    viveHandLeft = go.transform;
                    // Assigning ViveControllerGrab.leftHandAnimator to be this animator
                    leftHandDevice = viveHandLeft.parent.GetComponent<ViveControllerGrab>().device;
                }
            }
        }





        AnimateHand();

    }
}

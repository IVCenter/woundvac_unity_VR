using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncHands : NetworkBehaviour {

    private Transform viveHand;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        if (isServer)
        {
            if (viveHand)
            {
                this.transform.position = viveHand.position;
                this.transform.rotation = viveHand.rotation;
            }
            else
            {
                var go = GameObject.Find("Hand - Left");
                if (go)
                {
                    viveHand = go.transform;
                }
            }
        }
	}
}

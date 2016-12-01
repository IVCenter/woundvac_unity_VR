using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GhostVacuum : NetworkBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null || transform.parent.tag != "VuforiaTracker")
        {
            GameObject tracker = GameObject.FindGameObjectWithTag("VuforiaTracker");
            if (tracker != null)
            {
                transform.parent = tracker.transform;
                transform.localPosition = Vector3.zero;
            }
        }
    }

}

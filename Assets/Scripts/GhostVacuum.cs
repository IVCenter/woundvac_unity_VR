﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GhostVacuum : NetworkBehaviour
{
    public GameObject[] syncObjects;

    // Use this for initialization
    void Start()
    {
        if (isClient) {
            CmdSpawnWithLocalAuthority();
        }
    }

    // Update is called once per frame
    void Update()
    {
//        if (transform.parent == null || transform.parent.tag != "VuforiaTracker")
//        {
//            GameObject tracker = GameObject.FindGameObjectWithTag("VuforiaTracker");
//            if (tracker != null)
//            {
//                transform.parent = tracker.transform;
//                transform.localPosition = Vector3.zero;
//            }
//        }
    }

    [Command]
    void CmdSpawnWithLocalAuthority()
    {
        //        GameObject item = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
        //        NetworkServer.Spawn(item);
        //        NetworkServer.SpawnWithClientAuthority(go, connectionToClient);
        Debug.Log("*********************** CmdSpawnWithLocalAuthority");
        foreach (GameObject go in syncObjects)
        {
            if (go == null) continue;

            GameObject g = Instantiate(go) as GameObject;
            NetworkServer.SpawnWithClientAuthority(g, connectionToClient);
            Debug.Log("Server spawned object with client authority: " + g.name);

        }
    }

}

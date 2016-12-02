using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    public GameObject[] syncObjects;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

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
            Debug.Log("Client spawned object: " + g.name);

        }
    }

    public override void OnStartServer()
    {
        Debug.Log("============== Server Started");
        foreach (GameObject go in syncObjects)
        {
            if (go == null) continue;

            GameObject g = Instantiate(go) as GameObject;
            NetworkServer.Spawn(g);
            Debug.Log("NetworkServer spawned object: " + g.name);

        }
    }

    public override void OnStartClient()
    {
        Debug.Log("============== Client Started");

//        CmdSpawnWithLocalAuthority();
    }
}

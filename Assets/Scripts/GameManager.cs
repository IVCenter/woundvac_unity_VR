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

    public override void OnStartServer()
    {
        Debug.Log("Server Started");
        foreach (GameObject go in syncObjects)
        {
            GameObject g = Instantiate(go) as GameObject;
            NetworkServer.Spawn(g);
            Debug.Log("NetworkServer spawned object: " + g.name);

        }
    }

    public override void OnStartClient()
    {
        Debug.Log("Client Started");
        foreach (GameObject go in syncObjects)
        {
            //GameObject g = Instantiate(go) as GameObject;
            //NetworkServer.Spawn(g);
            //Debug.Log("NetworkServer spawned object: " + g.name);

        }
    }
}

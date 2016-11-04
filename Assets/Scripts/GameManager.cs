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
        //base.OnStartServer();
        Debug.Log("Server Started");
        foreach (GameObject go in syncObjects)
        {
            NetworkServer.Spawn(go);
            Debug.Log("NetworkServer spawned object: " + go.name);

        }
    }
}

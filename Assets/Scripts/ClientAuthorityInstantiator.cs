using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

// This object is being spawned by client's NetworkManager as player prefab, it is put in the SpawnInfo->Player Prefab section. 
// the reason for this is that only the objects spawned on the client are given local authority, so this script can invoke Command
// attribute on the server side to spawn objects with client authority. We spawn GhostVacuumBody and GhostVacuumPad this way so that
// they can be controlled on the client side and have their movement synced to the server side as well
public class ClientAuthorityInstantiator : NetworkBehaviour
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

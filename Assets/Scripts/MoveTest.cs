using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MoveTest : NetworkBehaviour
{
    public enum AUTHORITY {
        NONE,
        CLIENT_MOVE,
        SERVER_MOVE,
    }
    public AUTHORITY authority = AUTHORITY.CLIENT_MOVE;
    public float movementSpeed = 10f;
    public float rotationSpeed = 100f;

    void Update()
    {
        if (authority == AUTHORITY.NONE) return;
        if (authority == AUTHORITY.CLIENT_MOVE && isClient) Move();
        if (authority == AUTHORITY.SERVER_MOVE && isServer) Move();
    }

    private void Move()
    {
        float translation = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

    }

    //public override void OnStartServer()
    //{
    //    Debug.Log("NetworkServer spawned object: " + this.name);
    //    NetworkServer.Spawn(this.gameObject);
    //}

}

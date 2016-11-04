using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Move : NetworkBehaviour
{

    public float movementSpeed = 10f;
    public float rotationSpeed = 100f;

    void Update()
    {
        if (!isServer) return;

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

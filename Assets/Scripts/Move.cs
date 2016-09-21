using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{

    public float movementSpeed = 10f;
    public float rotationSpeed = 100f;

    void Update()
    {

        float translation = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

    }
}

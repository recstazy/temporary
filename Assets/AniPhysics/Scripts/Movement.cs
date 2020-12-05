using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Rigidbody body;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float mouseSensivity;

    private Vector3 movement;
    private float rotation;

    #endregion

    #region Properties
    
    #endregion

    private void Update()
    {
        movement = Vector3.zero;
        rotation = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            movement += speed * body.transform.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movement += -speed * body.transform.forward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movement += speed * body.transform.right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movement += -speed * body.transform.right;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            rotation = Input.GetAxis("Mouse X") * mouseSensivity;
        }
    }

    private void FixedUpdate()
    {
        body.AddForce(movement, ForceMode.Acceleration);
        body.AddTorque(Vector3.up * rotation, ForceMode.Acceleration);
    }
}

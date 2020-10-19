using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootObjects : MonoBehaviour
{
    [SerializeField]
    private Rigidbody prefab;

    [SerializeField]
    private float mass = 1f;

    [SerializeField]
    private float velocity = 1f;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        var position = Camera.main.transform.position;
        var direction = Camera.main.transform.forward;

        var instance = Instantiate(prefab, position, Quaternion.identity, null);
        instance.mass = mass;
        instance.AddForce(direction * velocity, ForceMode.VelocityChange);
        Destroy(instance.gameObject, 5f);
    }
}

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PTConveyorBelt : MonoBehaviour
{

    [SerializeField]
    GameObject PTCore;
    private PTController ptc;

    public float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ptc = PTCore.GetComponent<PTController>();
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            return;
        }

        Vector3 beltDirection = new Vector3(0, 0, ptc.isLoading ? 1 : -1);

        Vector3 beltVelocity = transform.TransformDirection(beltDirection.normalized) * speed;

        Vector3 current = rb.linearVelocity;
        Vector3 alongBelt = Vector3.Project(current, beltVelocity.normalized);
        Vector3 other = current - alongBelt;

        Vector3 blend = Vector3.Lerp(alongBelt, beltVelocity, 0.75f);
        rb.linearVelocity = blend + other;


        // rb.linearVelocity = blend;
       // Debug.Log(rb.linearVelocity);

        //Vector3 beltForce = transform.TransformDirection(direction.normalized) * speed;
        //rb.AddForce(beltForce, ForceMode.Acceleration);

    }

}

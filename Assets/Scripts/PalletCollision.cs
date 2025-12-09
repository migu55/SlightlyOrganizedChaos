using UnityEngine;

public class PalletCollision : MonoBehaviour
{

    public GameObject pallet;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = pallet.GetComponent<Rigidbody>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            rb.isKinematic = true;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            rb.isKinematic = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

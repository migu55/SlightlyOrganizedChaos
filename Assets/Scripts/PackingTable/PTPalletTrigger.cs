using System;
using UnityEngine;

public class PTPalletTrigger : MonoBehaviour
{

    [SerializeField]
    GameObject PTCore;

    private PTController ptc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ptc = PTCore.GetComponent<PTController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pallet" && other.transform.parent.gameObject.GetComponent<Pallet>() != null) //check since pallet prefab does not have any colliders
        {
            if (!ptc.hasPallet)
            {
                ptc.LoadPallet(other.transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ptc.currentPallet == null) return;


        if (other.transform.parent.gameObject != null && other.transform.parent.gameObject == ptc.currentPallet) //same prefab collider logic as above
        {
            ptc.RemovePallet();
        } else
        {
            return;
        }
    }

}

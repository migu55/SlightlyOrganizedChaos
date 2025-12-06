using UnityEngine;

public class PickupTrigger : MonoBehaviour
{
    [HideInInspector] public ForkliftPickup pickupController;

    private void Awake()
    {
        if (pickupController == null)
            pickupController = GetComponentInParent<ForkliftPickup>();

        Debug.Log(pickupController);
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pallet in Trigger");
        pickupController?.OnTriggerEnterFork(other);
    }

    private void OnTriggerExit(Collider other)
    {
        pickupController?.OnTriggerExitFork(other);
    }
}

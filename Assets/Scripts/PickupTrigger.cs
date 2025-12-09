using UnityEngine;

public class PickupTrigger : MonoBehaviour
{
    [HideInInspector] public ForkliftPickup pickupController;

    private void Awake()
    {
        if (pickupController == null)
            pickupController = GetComponentInParent<ForkliftPickup>();
    }


    private void OnTriggerEnter(Collider other)
    {
        pickupController?.OnTriggerEnterFork(other);
    }

    private void OnTriggerExit(Collider other)
    {
        pickupController?.OnTriggerExitFork(other);
    }
}

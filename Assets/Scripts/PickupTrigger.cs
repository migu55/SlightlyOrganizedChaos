using UnityEngine;

/// <summary>
/// Trigger collider component for forklift forks.
/// Detects when pallets enter/exit the fork zone and forwards events to ForkliftPickup controller.
/// Place this component on a child GameObject with a trigger collider at the fork location.
/// </summary>
public class PickupTrigger : MonoBehaviour
{
    // Reference to parent ForkliftPickup controller (hidden in inspector, auto-found at runtime)
    [HideInInspector] public ForkliftPickup pickupController;

    /// <summary>
    /// Initializes by finding the ForkliftPickup component in parent hierarchy.
    /// </summary>
    private void Awake()
    {
        // Auto-find the pickup controller on parent if not set
        if (pickupController == null)
            pickupController = GetComponentInParent<ForkliftPickup>();

        Debug.Log(pickupController);
    }

    /// <summary>
    /// Called when a collider enters the fork trigger zone.
    /// Forwards the event to ForkliftPickup to handle pallet attachment.
    /// </summary>
    /// <param name="other">The collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pallet in Trigger");
        // Forward to pickup controller using null-conditional operator
        pickupController?.OnTriggerEnterFork(other);
    }

    /// <summary>
    /// Called when a collider exits the fork trigger zone.
    /// Forwards the event to ForkliftPickup to handle pallet detachment.
    /// </summary>
    /// <param name="other">The collider that exited the trigger</param>
    private void OnTriggerExit(Collider other)
    {
        // Forward to pickup controller using null-conditional operator
        pickupController?.OnTriggerExitFork(other);
    }
}

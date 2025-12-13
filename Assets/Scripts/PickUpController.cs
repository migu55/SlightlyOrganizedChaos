using UnityEngine;

/// <summary>
/// Enables objects (like boxes/pallets) to be picked up and carried by the player.
/// Implements Interactable interface to allow player interaction via InteractionController.
/// Toggles between picked up (parented to player) and dropped (independent physics) states.
/// </summary>
public class PickUpController : MonoBehaviour, Interactable
{
    // Tracks whether this object is currently picked up by a player
    private bool pickedUp;
    
    // Reference to forklift's pickup system (used to check if already on forklift)
    private ForkliftPickup forkliftPickup;

    /// <summary>
    /// Initializes by finding the ForkliftPickup component to prevent conflicts.
    /// Searches parent hierarchy first, then searches entire scene as fallback.
    /// </summary>
    void Start()
    {
        // Try to find ForkliftPickup on parent or nearby
        forkliftPickup = GetComponentInParent<ForkliftPickup>();
        if (forkliftPickup == null)
        {
            forkliftPickup = FindObjectOfType<ForkliftPickup>();
        }
    }

    /// <summary>
    /// Interactable interface implementation. Toggles pickup state when player interacts.
    /// When picked up: disables physics, parents to player's BoxPoint, centers and rotates to match player.
    /// When dropped: unparents, re-enables physics to fall/collide naturally.
    /// </summary>
    /// <param name="interactor">The GameObject interacting (typically the player)</param>
    public void Interact(GameObject interactor)
    {
        if (!pickedUp)
        {
            // Pick up the object
            // Disable physics while held
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            
            // Parent to player's BoxPoint (carry position)
            gameObject.transform.SetParent(interactor.transform.Find("BoxPoint").gameObject.transform);
            
            // Center at BoxPoint position
            gameObject.transform.localPosition = Vector3.zero;
            
            // Rotate to match player orientation with 90-degree offset
            gameObject.transform.rotation = interactor.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            
            pickedUp = true;
        } 
        else
        {
            // Drop the object
            // Unparent from player
            gameObject.transform.SetParent(null);
            
            // Re-enable physics so it falls and collides normally
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            
            pickedUp = false;
        }
    }

    void Update()
    {
        
    }
}

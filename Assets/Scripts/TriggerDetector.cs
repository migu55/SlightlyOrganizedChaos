using UnityEngine;

/// <summary>
/// Detects when a player enters or exits a trigger zone (typically for interactions like forklift mounting).
/// Tracks trigger state and which player is currently in the zone.
/// Used by InteractionController to determine if interactions are possible.
/// </summary>
public class TriggerDetector : MonoBehaviour
{
    // Indicates whether a player is currently in the trigger zone
    public bool triggered = false;
    
    // Reference to the player GameObject currently in the trigger (null if none)
    public GameObject player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    /// <summary>
    /// Called when a collider enters the trigger zone.
    /// Sets triggered state and stores player reference if it's a player entering.
    /// </summary>
    /// <param name="other">The collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only respond if not already triggered and the entering object is a player
        if (!triggered && other.gameObject.tag == "Player")
        {
            Debug.Log("Player entered trigger");
            triggered = true;
            player = other.gameObject;
        }
    }

    /// <summary>
    /// Called when a collider exits the trigger zone.
    /// Clears triggered state and player reference if it's the tracked player leaving.
    /// </summary>
    /// <param name="other">The collider that exited the trigger</param>
    private void OnTriggerExit(Collider other)
    {
        // Only respond if currently triggered and the exiting object is a player
        if (triggered && other.gameObject.tag == "Player")
        {
            triggered = false;
            player = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

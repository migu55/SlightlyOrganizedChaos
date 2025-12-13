using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detects when a player enters or exits a trigger zone (typically for interactions like forklift mounting).
/// Tracks trigger state and which player is currently in the zone.
/// Used by InteractionController to determine if interactions are possible.
/// </summary>
public class TriggerDetector : MonoBehaviour
{
    // Indicates whether a player is currently in the trigger zone
    public bool triggered = false;
<<<<<<< HEAD
    
    // Reference to the player GameObject currently in the trigger (null if none)
    public GameObject player;
    
=======
    public List<GameObject> player;
>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new List<GameObject>();
    }

    /// <summary>
    /// Called when a collider enters the trigger zone.
    /// Sets triggered state and stores player reference if it's a player entering.
    /// </summary>
    /// <param name="other">The collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
<<<<<<< HEAD
        // Only respond if not already triggered and the entering object is a player
        if (!triggered && other.gameObject.tag == "Player")
=======
        if (other.gameObject.tag == "Player")
>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
        {
                triggered = true;
                if(!player.Contains(other.gameObject)) {
                    player.Add(other.gameObject);
                }
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
            player.Remove(other.gameObject);
            if(player.Count == 0) triggered = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

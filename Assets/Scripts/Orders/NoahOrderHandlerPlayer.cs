using UnityEngine;
using UnityEngine.InputSystem;

public class NoahOrderHandlerPlayer : MonoBehaviour
{
    // Wether this player has opened the product order menu
    public bool isMenuOpen = false;

    // Used to tell when a player has entered the manager's office.
    private OfficeTriggerController officeTrigger;

    // Initializes order spawning related objects into the player object lists, and handles setting the interact to also include
    // opening the menu of the office laptop.
    void Start()
    {
        GameObject officeTriggerObject = GameObject.FindGameObjectWithTag("OfficeTrigger");
        officeTrigger = officeTriggerObject.GetComponent<OfficeTriggerController>();
    }

    // Handles when a player enters triggers for the office and the laptop within. When a player enters the office,
    // add them to the list of sources for the text above the laptop to aim at.
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BoxSpawner"))
        {
            other.gameObject.GetComponent<NoahOrderHandlerTrigger>().playersInArea.Add(gameObject);
        }
        if (other.gameObject.CompareTag("OfficeTrigger"))
        {
            officeTrigger.AddToAimConstraint(gameObject);
        }
    }

    // When a player leaves a trigger, they are removed from the lists they were added to when they entered.
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BoxSpawner"))
        {
            other.gameObject.GetComponent<NoahOrderHandlerTrigger>().playersInArea.Remove(gameObject);
        }
        if (other.gameObject.CompareTag("OfficeTrigger"))
        {
            officeTrigger.RemoveFromAimConstraint(gameObject);
        }
    }
}

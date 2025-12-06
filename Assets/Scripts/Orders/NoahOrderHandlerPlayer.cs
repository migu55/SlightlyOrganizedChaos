using UnityEngine;
using UnityEngine.InputSystem;

public class NoahOrderHandlerPlayer : MonoBehaviour
{
    public bool isMenuOpen = false;

    // actions and interact are used only for assigning the interact input to enabling the menu, will have to be
    // changed to account for multiple players.
    private InputActions actions;
    private InputAction interact;
    private OfficeTriggerController officeTrigger; // Used to tell when a player has entered the manager's office.

    // Initializes order spawning related objects into the player object lists, and handles setting the interact to also include
    // opening the menu of the office laptop.
    void Start()
    {
        actions = new InputActions();
        interact = actions.Player.Interact;
        interact.Enable();
        GameObject officeTriggerObject = GameObject.FindGameObjectWithTag("OfficeTrigger");
        officeTrigger = officeTriggerObject.GetComponent<OfficeTriggerController>();
    }

    // Handles when a player enters triggers for the office and the laptop within. When a player enters the office,
    // disable the particle cloud blocking vision to the trucks, and set add them to the list of sources for the text
    // above the laptop to aim at.
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

    // When a player leaves a trigger, they are removed from the lists they were added to when they entered. This also
    // triggers the office trigger to check if it should turn the particles back on.
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

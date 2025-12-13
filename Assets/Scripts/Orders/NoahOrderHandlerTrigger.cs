using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class NoahOrderHandlerTrigger : MonoBehaviour, Interactable
{
    // List of players in the area available to open the menu, used for external scripts to call OpenOrCloseMenu on players
    public List<GameObject> playersInArea;
    
    [SerializeField]
    // Text box reference used to toggle visibility on and off
    private TextMeshPro textBox;
    [SerializeField]
    // UI controller reference to pass to the InputBridge when a player opens the menu
    private UIController uiController;
    [SerializeField]
    // Reference to the camera that views the order menu, to show/hide the menu for the player that opened it
    private Camera menuCamera;
    [SerializeField]
    // Ready system reference used to track booleans to ensure the correct open/close behaviour at the right times
    private ClockInHandler readySystem;

    // Aim constraint component of the canvas object, just used for initialization
    private AimConstraint pointer;
    // Reference to the menu, used to call Enable/DisableMenu() when the menu is opened or closed
    private ProductOrderingMenu menu;
    // Reference to the UI action map, to be enabled/disabled when players open/close the menu so they can go to and from UI controls
    private InputActionMap uiControls;

    // Initialization, setting references and starting conditions
    void Start()
    {
        textBox.GetComponent<Renderer>().enabled = false;
        pointer = GetComponentInChildren<AimConstraint>();
        pointer.RemoveSource(0);
        menu = GameObject.FindGameObjectWithTag("Order Menu").GetComponent<ProductOrderingMenu>();
        uiControls = new InputActions().UI;
        playersInArea = new();
    }

    // Used to track if any player has pressed their cancel button, used to close the menu when they have it open
    void Update()
    {
        foreach (var p in playersInArea)
        {
            if (p.GetComponent<PlayerInput>().actions.FindAction("Cancel").IsPressed())
            {
                // I suppose since it's calling OpenOrCloseMenu, they can use it to open the menu too
                OpenOrCloseMenu(p);
            }
        }
    }

    // Used to open or close the order menu when the player interacts/cancels while in the trigger
    public void OpenOrCloseMenu(GameObject playerObj)
    {
        // If the player object is missing its other script, prevent any actions since they rely on that script
        if (!playerObj.TryGetComponent<NoahOrderHandlerPlayer>(out var orderHandler))
        {
            return;
        }

        // Player input systems
        var input = playerObj.GetComponent<PlayerInput>();
        var bridge = playerObj.GetComponent<InputBridge>();

        // If any players have the menu open, don't allow anyone else to interact with it. Avoids voiding players
        bool anyOpen = false;
        foreach (GameObject p in playersInArea)
        {
            if (p.GetComponent<NoahOrderHandlerPlayer>().isMenuOpen)
            {
                anyOpen = true;
            }
        }

        // Only open the menu if the player interacting is not already in the menu, and nobody else is in the menu,
        // and the game is not in the tutorial status
        if (!orderHandler.isMenuOpen && !anyOpen && (readySystem.firstTutorial || !readySystem.tutorialEnabled))
        {
            OpenMenu(playerObj);
            orderHandler.isMenuOpen = true;
            bridge.SetController(uiController);
            input.SwitchCurrentActionMap("UI");
        }
        // If the player has the menu open already, and the game is not in a tutorial mode, close the menu
        else if (readySystem.firstTutorial || !readySystem.tutorialEnabled)
        {
            CloseMenu(playerObj);
            orderHandler.isMenuOpen = false;
            bridge.ClearController();
            input.SwitchCurrentActionMap("Player");
        }
    }

    // Opens the order menu for the player passed to the function
    private void OpenMenu(GameObject player)
    {
        // Gets the player camera and sets the menu camera to display in place of it
        Camera playerCamera = player.GetComponent<PlayerController>().GetCamera();
        menuCamera.rect = playerCamera.rect;
        menuCamera.depth = playerCamera.depth + 1;
        playerCamera.enabled = false;
        menuCamera.enabled = true;

        // Activate menu handling
        menu.EnableMenu();
        uiControls.Enable();
    }

    private void CloseMenu(GameObject player)
    {
        // Gets the player camera and re-enables it, hiding the menu camera
        Camera playerCamera = player.GetComponent<PlayerController>().GetCamera();
        playerCamera.enabled = true;
        menuCamera.enabled = false;

        // Deactivate menu handling
        menu.DisableMenu();
        uiControls.Disable();
    }

    // Implementation of Interactable interface, just used for OpenOrCloseMenu
    public void Interact(GameObject interactor)
    {
        OpenOrCloseMenu(interactor);
    }

}

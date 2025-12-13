using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

/// <summary>
/// Manages player interactions with interactable objects (like forklifts, buttons, etc.).
/// Uses trigger detection to determine when players are in range and routes input to the appropriate handler.
/// Supports multiple players by matching input devices to PlayerInput components.
/// </summary>
public class InteractionController : MonoBehaviour
{
    // Detects when players enter/exit interaction range via trigger colliders
    public TriggerDetector detector;
    
    // The GameObject that initiated the interaction (typically the player)
    public GameObject interactor;
    
    // Input system actions container
    private InputActions inputActions;
    
    // Specific interact action binding
    private InputAction interaction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    /// <summary>
    /// Initializes components and creates input action bindings.
    /// Called before Start.
    /// </summary>
    private void Awake()
    {
        // Get trigger detector to check when players are in range
        detector = GetComponent<TriggerDetector>();
        
        // Create new input actions instance
        inputActions = new InputActions();
    }

    /// <summary>
    /// Enables the interact input action and subscribes to interaction events.
    /// </summary>
    private void OnEnable()
    {
        // Get reference to the Interact action from Player action map
        interaction = inputActions.Player.Interact;
        interaction.Enable();

        // Subscribe to interaction performed event
        // DoInteraction is automatically called when Interact binding is triggered
        interaction.performed += DoInteraction;
    }

    /// <summary>
    /// Unsubscribes from interaction events to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        interaction.performed -= DoInteraction;
    }

    /// <summary>
    /// Handles interaction input from any player.
    /// Matches the input device to the correct player, verifies they're in range via trigger detection,
    /// then routes to either Interactable interface or specific component handlers (like ButtonHandler).
    /// </summary>
    /// <param name="obj">Input callback context containing device and action info</param>
    // TODO: Currently if a player is on a forklift and another player comes up and interacts, 
    // it will kick the player on the forklift into the void...
    private void DoInteraction(InputAction.CallbackContext obj)
    {
        // Find which player triggered the input by matching their input device
        var device = obj.control.device;
        var playerInput = PlayerInput.all.FirstOrDefault(p => p.devices.Contains(device));

        if (playerInput == null)
        {
            Debug.LogWarning("No matching PlayerInput found for this interaction.");
            return;
        }

        // Get the PlayerController component to identify the player GameObject
        var playerController = playerInput.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("No PlayerController found on PlayerInput object.");
            return;
        }

        var playerObj = playerController.gameObject;

        // Only execute interaction if this player is in range (detector triggered) and is the detected player
        if (detector.triggered && detector.player == playerObj)
        {
            // Try Interactable interface first (used by forklift, etc.)
            var interactable = GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(playerObj);
            }
            // Otherwise try specific component handlers like ButtonHandler
            else if (TryGetComponent(out ButtonHandler button))
            {
                button.AddTestPalletsToZoneTracker();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

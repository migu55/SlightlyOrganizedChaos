using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

// To be placed on the forklift
public class InteractionController : MonoBehaviour
{
    public TriggerDetector detector;
    public GameObject interactor;
    private InputActions inputActions;
    private InputAction interaction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void Awake()
    {
        detector = GetComponent<TriggerDetector>();
        inputActions = new InputActions(); //create new InputActions
    }

    private void OnEnable()
    {
        interaction = inputActions.Player.Interact; //get reference to interaction action
        interaction.Enable();

        //create a DoInteraction callback function
        //DoInteraction automatically called when Interaction binding performed
        interaction.performed += DoInteraction;
    }

    private void OnDisable()
    {
        interaction.performed -= DoInteraction;
    }

    // TO:DO: Currently if a player is on a forklift and another player comes up and interacts, it will kick the player on the forklift into the void...
    private void DoInteraction(InputAction.CallbackContext obj)
    {

        // Find which player triggered the input based on their device
        var device = obj.control.device;
        var playerInput = PlayerInput.all.FirstOrDefault(p => p.devices.Contains(device));

        if (playerInput == null)
        {
            Debug.LogWarning("No matching PlayerInput found for this interaction.");
            return;
        }

        var playerController = playerInput.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("No PlayerController found on PlayerInput object.");
            return;
        }

        var playerObj = playerController.gameObject;

        // Only run if the detector is triggered
        if (detector.triggered && detector.player == playerObj)
        {
            var interactable = GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(playerObj);
            }
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

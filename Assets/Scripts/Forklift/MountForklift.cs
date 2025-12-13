using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles mounting and dismounting the forklift vehicle.
/// Manages camera switching, player positioning, input mapping, and physics state transitions.
/// Attach this component to the forklift GameObject.
/// </summary>
public class MountForklift : MonoBehaviour, Interactable
{
    // Audio controller for engine start/stop sounds
    public ForkliftAudioController audioController;
    
    // Cached reference to the player's transform when mounted
    private Transform playerTransform;
    
    // Cached reference to the camera's transform
    private Transform cameraTransform;
    
    // Forklift's rigidbody for physics control
    private Rigidbody forkliftRigidbody;
    
    // Player's rigidbody for physics state management
    private Rigidbody playerRigidbody;
    
    // Reference to the player's camera to disable when mounted
    private Camera playerCamera;
    
    // Reference to the forklift's camera to enable when mounted
    private Camera forkliftCamera;

    // Reference to the currently mounted player GameObject
    private GameObject player;
    
    // Tracks whether a player is currently mounted on the forklift
    public bool mounted = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find and cache the forklift's camera component
        forkliftCamera = GetComponentInChildren<Camera>();
        
        // Cache the forklift's rigidbody component
        forkliftRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Interactable interface implementation. Called when a player interacts with the forklift.
    /// </summary>
    /// <param name="interactor">The GameObject attempting to interact (typically the player)</param>
    public void Interact(GameObject interactor)
    {
        MountOrDismount(interactor, interactor.GetComponentInChildren<Camera>());
    }

    /// <summary>
    /// Toggles between mounting and dismounting the forklift.
    /// Prevents mounting if another player is already using the forklift.
    /// </summary>
    /// <param name="player">The player GameObject attempting to mount/dismount</param>
    /// <param name="playerCamera">The player's camera to switch with forklift camera</param>
    public void MountOrDismount(GameObject player, Camera playerCamera)
    {
        // Prevent mounting if another player is already mounted
        if(mounted && this.player != player)
        {
            return;
        }
        
        // Cache player and camera references
        this.player = player;
        this.playerCamera = playerCamera;

        if (!mounted)
        {
            // Check if player is carrying something - prevent mounting if so
            var pickupController = player.GetComponentInChildren<PickUpController>();
            if (pickupController == null)
            {
                Mount();
            }
        }
        else
        {
            Dismount();
        }
    }

    /// <summary>
    /// Mounts the player onto the forklift.
    /// - Switches cameras from player to forklift view
    /// - Disables player physics and movement
    /// - Parents player to forklift at mount point
    /// - Switches input controls to forklift action map
    /// </summary>
    private void Mount()
    {
        audioController.PlayEngineStart();
        
        // Switch camera from player to forklift view
        forkliftCamera.rect = playerCamera.rect;
        forkliftCamera.depth = playerCamera.depth + 1;
        playerCamera.enabled = false;
        forkliftCamera.enabled = true;
        
        // Enable forklift physics
        forkliftRigidbody.isKinematic = false;

        // Disable player physics and collision while mounted
        playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.isKinematic = true;
        player.GetComponent<CapsuleCollider>().enabled = false;
        
        // Position player at forklift's mount point and parent to forklift
        player.transform.position = transform.Find("MountPoint").position;
        player.transform.rotation = transform.Find("MountPoint").rotation;
        player.transform.SetParent(transform);

        // Switch player controls to forklift input system
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<InputBridge>().enabled = true;
        player.GetComponent<InputBridge>().SetController(GetComponent<ForkliftController>());
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Forklift");

        mounted = true;
    }

    /// <summary>
    /// Dismounts the player from the forklift.
    /// - Restores player camera and disables forklift camera
    /// - Re-enables player physics and collision
    /// - Positions player at dismount point
    /// - Restores player input controls
    /// </summary>
    public void Dismount()
    {
        if (!mounted) return;
        
        // Play engine stop audio
        audioController.StopEngineIdle();
        audioController.PlayEngineStop();
<<<<<<< HEAD
        
        // Switch camera back to player view
        Debug.Log("Dismounting");
=======
        //enabling the player camera and disabling the forklift camera
>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
        playerCamera.enabled = true;
        forkliftCamera.enabled = false;
        
        // Disable forklift physics
        forkliftRigidbody.isKinematic = true;

        // Unparent player and position at dismount point
        player.transform.SetParent(null);
        player.transform.position = transform.Find("DismountPoint").position;
        player.transform.rotation = transform.Find("DismountPoint").rotation;

        // Restore player controls and input system
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<InputBridge>().ClearController();
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        player.GetComponent<InputBridge>().enabled = true;

        // Re-enable player physics and collision
        playerRigidbody.isKinematic = false;
        player.GetComponent<CapsuleCollider>().enabled = true;
        
        // Clear cached references
        playerTransform = null;
        playerRigidbody = null;

        mounted = false;
    }
}

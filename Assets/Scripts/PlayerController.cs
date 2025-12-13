using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player character movement, camera, jumping, and rotation.
/// Handles physics-based movement with ground detection, third-person camera with obstruction avoidance,
/// and input from Unity's new Input System. Supports multiple players with AudioListener management.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Movement and rotation speed multipliers
    public float movementSpeed = 1;
    public float rotateSpeed = 1;

    // Maximum number of jumps allowed before landing
    public int maxJumps = 1;
    
    // Maximum distance for third-person camera from player
    public float cameraDistance = 20f;
    
    // Remaining jumps before needing to land
    private int jumpsRemaining = 0;
    
    // Current movement input (x = strafe, y = forward/back)
    private Vector2 movement;
    
    // Current camera rotation input (x = yaw, y = pitch)
    private Vector2 rotation;

    // Reference to player's camera
    public Camera playerCamera;
    
    // Reference to player's visual body/mesh with animator
    public GameObject body;

    // Extra distance added to ground check raycast
    public float extraDistance = 0.1f;
    
    // Tracks whether player was grounded in previous frame
    private bool wasGrounded = true;
    
    // Player's rigidbody for physics-based movement
    Rigidbody rb;

    // Audio listener component (only one should be active to avoid warnings)
    private AudioListener audioListener;

    // Ray used for camera obstruction detection
    private Ray ray;
    
    // Pivot point for camera rotation (child transform)
    private GameObject rayStart;

    /// <summary>
    /// Initializes player camera, manages AudioListener for multiplayer, and finds camera pivot point.
    /// Locks cursor for first-person-like camera control.
    /// </summary>
    private void Start()
    {
        // Lock and hide cursor for camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find this player's camera and audio listener
        playerCamera = GetComponentInChildren<Camera>();
        audioListener = playerCamera != null ? playerCamera.GetComponent<AudioListener>() : null;

        // Count existing PlayerControllers to manage AudioListener
        // Only the first player should have AudioListener enabled to avoid Unity warnings
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        if (players.Length == 1)
        {
            // First player - enable AudioListener
            if (audioListener != null)
                audioListener.enabled = true;
        }
        else
        {
            // Additional players - disable AudioListener
            if (audioListener != null)
                audioListener.enabled = false;
        }

        // Find camera rotation pivot point
        rayStart = transform.Find("RayStart")?.gameObject;
        if (rayStart == null)
        {
            rayStart = gameObject;
        }
    }

    /// <summary>
    /// Caches component references early in initialization.
    /// </summary>
    private void Awake()
    {
        // Optional: Load custom controller mappings
        // SDLMappingLoader.LoadCustomMapping();

        // Cache rigidbody for physics-based movement
        rb = GetComponent<Rigidbody>();
        
        // Cache camera reference
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
    }
    
    /// <summary>
    /// Input callback for movement. Updates animator parameters for walk animations.
    /// </summary>
    /// <param name="value">Movement input vector (x = strafe, y = forward/back)</param>
    public void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
        
        // Update animator blend tree parameters for directional movement
        body.GetComponent<Animator>().SetFloat("Vert", movement.y);
        body.GetComponent<Animator>().SetFloat("Hor", movement.x);
    }

    /// <summary>
    /// Input callback for camera rotation.
    /// </summary>
    /// <param name="value">Rotation input vector (x = yaw, y = pitch)</param>
    public void OnRotation(InputValue value)
    {
        rotation = value.Get<Vector2>();
    }

    /// <summary>
    /// Checks if the player is standing on solid ground using a downward raycast.
    /// </summary>
    /// <returns>True if player is grounded, false if in the air</returns>
    private bool IsGrounded()
    {
        Collider col = GetComponent<Collider>();
        
        // Calculate raycast distance based on collider height plus extra buffer
        float checkDistance = (col != null ? col.bounds.extents.y : 0.5f) + extraDistance;
        
        return Physics.Raycast(transform.position, Vector3.down, checkDistance);
    }

    /// <summary>
    /// Input callback for jump. Allows jumping if jumps remain before landing.
    /// Applies instant upward velocity and triggers jump animation.
    /// </summary>
    /// <param name="value">Jump button input value</param>
    public void OnJump(InputValue value)
    {
        if (value.isPressed && jumpsRemaining > 0)
        {
            // Apply instant upward velocity for responsive jump feel
            rb.AddForce(new Vector3(0, 20f, 0), ForceMode.VelocityChange);
            jumpsRemaining--;
            
            // Trigger jump animation
            body.GetComponent<Animator>().SetBool("IsJump", true);
        }
    }

    /// <summary>
    /// Returns the player's camera component.
    /// </summary>
    /// <returns>Player's camera reference</returns>
    public Camera GetCamera()
    {
        return playerCamera;
    }

    /// <summary>
    /// Called every frame. Corrects body position drift if it occurs.
    /// </summary>
    public void Update() {
        // Reset body position if it drifts from center
        if(body.transform.localPosition.x <= -0.0075f) {
            body.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Handles collision with pallets by making them kinematic (prevents physics glitches).
    /// </summary>
    /// <param name="collision">Collision data</param>
    public void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.CompareTag("Pallet")) {
            Rigidbody palletRb = collision.gameObject.GetComponent<Rigidbody>();
            palletRb.isKinematic = true;
        }
    }

    /// <summary>
    /// Re-enables pallet physics when player stops colliding with it.
    /// </summary>
    /// <param name="collision">Collision data</param>
    public void OnCollisionExit(Collision collision) {
        if(collision.gameObject.CompareTag("Pallet")) {
            Rigidbody palletRb = collision.gameObject.GetComponent<Rigidbody>();
            palletRb.isKinematic = false;
        }
    }
    

    /// <summary>
    /// Called every physics update. Handles all physics-based movement, jumping, 
    /// camera positioning with obstruction avoidance, and camera rotation.
    /// </summary>
    private void FixedUpdate()
    {
        // === GROUND DETECTION AND JUMP RESET ===
        bool isGrounded = IsGrounded();
        
        if (isGrounded)
        {
            // Refresh available jumps when landing
            jumpsRemaining = maxJumps;
            body.GetComponent<Animator>().SetBool("IsJump", false);
        }
        else
        {
            // Apply extra gravity when airborne for snappier jump feel
            if (rb != null)
            {
                rb.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
        }
        
        wasGrounded = isGrounded;

        // === PLAYER MOVEMENT ===
        Vector2 movementVector = movement;
        Vector2 rotationVector = rotation;

        // Convert 2D input to 3D movement direction
        Vector3 moveDir = new Vector3(movementVector.x, 0, movementVector.y);

        // Rotate player body based on horizontal rotation input (yaw)
        transform.Rotate(0, rotationVector.x * rotateSpeed, 0);
        
        // Apply movement force in player's local direction
        rb.AddForce(transform.TransformDirection(moveDir), ForceMode.VelocityChange);

        // === CAMERA PITCH ROTATION ===
        float inputZ = -rotationVector.y; // Vertical camera input (pitch)
        float rotationAmount = inputZ * rotateSpeed;

        // Get current pitch angle normalized to -180 to 180 range
        float currentX = rayStart.transform.localEulerAngles.x;
        if (currentX > 180f) currentX -= 360f;

        // Apply pitch rotation with clamping to prevent over-rotation
        float newX = Mathf.Clamp(currentX + rotationAmount, -45f, 45f);
        Vector3 localEuler = rayStart.transform.localEulerAngles;
        rayStart.transform.localEulerAngles = new Vector3(newX, localEuler.y, localEuler.z);
        
        // Match camera rotation to pivot rotation
        playerCamera.transform.localEulerAngles = rayStart.transform.localEulerAngles;

        // === CAMERA OBSTRUCTION DETECTION ===
        // Cast ray backward from pivot to detect walls/obstacles
        ray = new Ray(rayStart.transform.position, -rayStart.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hitInfo, cameraDistance))
        {
            // Obstruction detected - position camera at hit point to prevent clipping
            if (playerCamera != null)
            {
                playerCamera.transform.position = hitInfo.point;
            }
        }
        else
        {
            // No obstruction - position camera at full distance behind player
            if (playerCamera != null)
            {
                Vector3 cameraOffset = -rayStart.transform.forward * cameraDistance;
                playerCamera.transform.position = rayStart.transform.position + cameraOffset;
            }
        }
    }
}

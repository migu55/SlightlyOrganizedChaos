using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //create private internal references
    public float movementSpeed = 1;
    public float rotateSpeed = 1;

    public int maxJumps = 1;
    public float cameraDistance = 20f;
    private int jumpsRemaining = 0;
    private Vector2 movement;
    private Vector2 rotation;

    public Camera playerCamera;
    public GameObject body;

    public float extraDistance = 0.1f; // extra distance to check for grounding
    private bool wasGrounded = true; // track previous grounded state
    Rigidbody rb;

    private AudioListener audioListener;

    private Ray ray;
    
    private GameObject rayStart;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find this player's camera
        playerCamera = GetComponentInChildren<Camera>();
        audioListener = playerCamera != null ? playerCamera.GetComponent<AudioListener>() : null;

        // Count existing PlayerControllers in the scene
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        // If this is the first player (index 0), enable AudioListener
        // Otherwise disable it to avoid "multiple listeners" warning
        if (players.Length == 1)
        {
            if (audioListener != null)
                audioListener.enabled = true;
        }
        else
        {
            if (audioListener != null)
                audioListener.enabled = false;
        }

        rayStart = transform.Find("RayStart")?.gameObject;
        if (rayStart == null)
        {
            Debug.LogWarning("RayStart child not found; using player GameObject as fallback.");
            rayStart = gameObject;
        }
    }

    private void Awake()
    {
        // SDLMappingLoader.LoadCustomMapping(); // Inject SDL2 mapping first ✅

        rb = GetComponent<Rigidbody>(); //get rigidbody, responsible for enabling collision with other colliders
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
    }
    
    public void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
        body.GetComponent<Animator>().SetFloat("Vert", movement.y);
        body.GetComponent<Animator>().SetFloat("Hor", movement.x);
    }

    public void OnRotation(InputValue value)
    {
        rotation = value.Get<Vector2>();
    }

    private bool IsGrounded()
    {
        Collider col = GetComponent<Collider>();
        float checkDistance = (col != null ? col.bounds.extents.y : 0.5f) + extraDistance;
        return Physics.Raycast(transform.position, Vector3.down, checkDistance);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && jumpsRemaining > 0)
        {
            rb.AddForce(new Vector3(0, 20f, 0), ForceMode.VelocityChange);
            jumpsRemaining--;
            body.GetComponent<Animator>().SetBool("IsJump", true);
        }
    }

    public Camera GetCamera()
    {
        return playerCamera;
    }

    public void Update() {
        if(body.transform.localPosition.x <= -0.0075f) {
            body.transform.localPosition = Vector3.zero;
        }
    }

    public void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.CompareTag("Pallet")) {
            Debug.Log("Collided with pallet");
            Rigidbody palletRb = collision.gameObject.GetComponent<Rigidbody>();
            palletRb.isKinematic = true;
        }
    }

    public void OnCollisionExit(Collision collision) {
        if(collision.gameObject.CompareTag("Pallet")) {
            Debug.Log("Collision exit with pallet");
            Rigidbody palletRb = collision.gameObject.GetComponent<Rigidbody>();
            palletRb.isKinematic = false;
        }
    }
    

    // called every physics update
    private void FixedUpdate()
    {
        bool isGrounded = IsGrounded();
        
        // refresh available jumps if we're on the ground
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            body.GetComponent<Animator>().SetBool("IsJump", false);
        }
        else
        {
            // Apply extra downward force when in the air (double gravity effect)
            if (rb != null)
            {
                rb.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
        }
        
        wasGrounded = isGrounded;

        Vector2 movementVector = movement;
        Vector2 rotationVector = rotation;

        Vector3 moveDir = new Vector3(movementVector.x, 0, movementVector.y);

        transform.Rotate(0, rotationVector.x * rotateSpeed, 0);
        rb.AddForce(transform.TransformDirection(moveDir), ForceMode.VelocityChange);

        float inputZ = -rotationVector.y; // forward/back input
        float rotationAmount = inputZ * rotateSpeed;

        // get current local X as signed angle
        float currentX = rayStart.transform.localEulerAngles.x;
        if (currentX > 180f) currentX -= 360f;

        // apply rotation and clamp to reasonable limits
        float newX = Mathf.Clamp(currentX + rotationAmount, -45f, 45f);
        Vector3 localEuler = rayStart.transform.localEulerAngles;
        rayStart.transform.localEulerAngles = new Vector3(newX, localEuler.y, localEuler.z);
        playerCamera.transform.localEulerAngles = rayStart.transform.localEulerAngles;

        
        ray = new Ray(rayStart.transform.position, -rayStart.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, cameraDistance))
        {
            if (playerCamera != null)
            {
                // Move camera to the ray hit point and point it back at the player (slightly above center)
                playerCamera.transform.position = hitInfo.point;
            }
        }
        else
        {
            if (playerCamera != null)
            {
                // Position the camera at a fixed distance behind the player
                Vector3 cameraOffset = -rayStart.transform.forward * cameraDistance;
                playerCamera.transform.position = rayStart.transform.position + cameraOffset;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls forklift movement, steering, camera, and fork operations.
/// Manages physics-based movement with rear-wheel steering, fork lifting/tilting,
/// camera zoom/obstruction detection, and wheel rotation animations.
/// </summary>
public class ForkliftController : MonoBehaviour
{
    // Camera rotation sensitivity
    public float rotateSpeed = 1;

    // Reference to audio controller for engine and hydraulic sounds
    public ForkliftAudioController audioController;
    
    // Current movement input (-1 to 1, forward/backward)
    private float movement;
    
    // Current steering input (x = steering, y unused)
    private Vector2 rotation;

    // Calculated lifting movement for this frame
    private Vector3 lifting;
    
    // Calculated tilting movement for this frame
    private Vector3 tilting;

    // Camera mouse/stick delta for rotation
    private Vector2 camDelta;
    
    // Current zoom input value
    private float zoomAmount;
    
    // User's requested zoom distance (positive value)
    private float targetCameraDistance;
    
    // Camera's actual distance after obstruction clamping and smoothing
    private float actualCameraDistance;
    
    // Maximum camera distance from forklift
    public float cameraDistance = 20f;
    
    // Camera pivot point that rotates with mouse input
    private GameObject RotatePoint;
    
    // The forklift's camera component
    private GameObject forkliftCamera;
    
    // The fork assembly GameObject that lifts and tilts
    private GameObject forks;
    
    // Individual wheel GameObjects for steering and rotation animation
    private GameObject rearRightWheel;
    private GameObject rearLeftWheel;
    private GameObject frontLeftWheel;
    private GameObject frontRightWheel;
    
    // Forklift's rigidbody for physics-based movement
    Rigidbody rb;

    // Ray used for camera obstruction detection
    private Ray ray;
    
    // Target steering angles for rear wheels (Y-axis rotation only)
    private float rearLeftSteerAngle = 0f;
    private float rearRightSteerAngle = 0f;
    
    // Accumulated wheel rolling rotation angles (X-axis rotation only)
    private float rearLeftSpinAngle = 0f;
    private float rearRightSpinAngle = 0f;
    private float frontLeftSpinAngle = 0f;
    private float frontRightSpinAngle = 0f;

    /// <summary>
    /// Initializes forklift components by finding and caching references to child objects.
    /// Sets up initial camera distance values.
    /// </summary>
    void Start()
    {
        // Cache rigidbody for physics-based movement
        rb = GetComponent<Rigidbody>();
        
        // Find and cache all child GameObjects
        RotatePoint = transform.Find("RotatePoint").gameObject;
        forkliftCamera = transform.Find("RotatePoint").Find("ForkliftCamera").gameObject;
        forks = transform.Find("Lift").gameObject;
        rearRightWheel = transform.Find("Wheel_R_Back").gameObject;
        rearLeftWheel = transform.Find("Wheel_L_Back").gameObject;
        frontLeftWheel = transform.Find("Wheel_L_Front").gameObject;
        frontRightWheel = transform.Find("Wheel_R_Front").gameObject;

        // Initialize camera distances to default value
        targetCameraDistance = cameraDistance;
        actualCameraDistance = cameraDistance;
    }

    /// <summary>
    /// Input callback for steering control. Stores horizontal steering input.
    /// </summary>
    /// <param name="rotationInput">Steering input vector (x = horizontal steering)</param>
    public void Steering(Vector2 rotationInput)
    {
        rotation = rotationInput;
    }

    /// <summary>
    /// Input callback for movement control. Stores forward/backward movement input.
    /// </summary>
    /// <param name="movementInput">Movement input vector (y = forward/backward)</param>
    public void Move(Vector2 movementInput)
    {
        movement = movementInput.y;
    }

    /// <summary>
    /// Input callback for camera zoom control.
    /// </summary>
    /// <param name="zoomInput">Zoom input vector (y = zoom in/out)</param>
    public void Zoom(Vector2 zoomInput)
    {
        zoomAmount = zoomInput.y;
    }

    /// <summary>
    /// Input callback for interact button. Dismounts player if currently mounted.
    /// </summary>
    /// <param name="value">Interaction button state</param>
    public void Interact(bool value)
    {
        var mountForklift = GetComponent<MountForklift>();
        if (GetComponent<MountForklift>().mounted)
        {
            GetComponent<MountForklift>().Dismount();
        }
    }

    /// <summary>
    /// Input callback for fork lift/tilt control. Calculates lifting and tilting movements
    /// with clamping to prevent exceeding physical limits.
    /// </summary>
    /// <param name="value">Fork control input (y = lift up/down, x = tilt forward/back)</param>
    public void Forks(Vector2 value)
    {
        float liftAmount = value.y;
        float tiltAmount = value.x;
        float liftSpeed = 2f; // Vertical lifting speed (meters per second)
        float tiltSpeed = 25f; // Tilt rotation speed (degrees per second)
        float minLiftY = 2.2f; // Minimum fork height
        float maxLiftY = 8f;   // Maximum fork height
        float minTiltX = -15f; // Maximum backward tilt
        float maxTiltX = 15f;  // Maximum forward tilt

        if (forks == null) return;

        // Calculate lifting movement
        Vector3 lift = Vector3.up * liftAmount * liftSpeed * Time.fixedDeltaTime;
        
        // Clamp lifting when approaching limits to prevent overshooting
        if (liftAmount > 0)
        {
            float remainingUpDistance = maxLiftY - forks.transform.localPosition.y;
            if (remainingUpDistance < lift.y)
            {
                lift.y = Mathf.Max(0, remainingUpDistance);
            }
        }
        else if (liftAmount < 0)
        {
            float remainingDownDistance = forks.transform.localPosition.y - minLiftY;
            if (remainingDownDistance < Mathf.Abs(lift.y))
            {
                lift.y = -Mathf.Max(0, remainingDownDistance);
            }
        }
        lifting = lift;

        // Calculate tilting movement
        Vector3 tilt = Vector3.right * tiltAmount * tiltSpeed * Time.fixedDeltaTime;
        tilt = Vector3.ClampMagnitude(tilt, maxTiltX - forks.transform.localEulerAngles.z);
        tilting = tilt;
    }

    /// <summary>
    /// Input callback for camera rotation control via mouse or joystick.
    /// </summary>
    /// <param name="camDelta">Camera rotation delta (x = yaw, y = pitch)</param>
    public void Camera(Vector2 camDelta)
    {
        this.camDelta = camDelta;
    }
    /// <summary>
    /// Fixed update called at fixed physics intervals.
    /// Handles all physics-based movement, steering, fork operations, camera updates, and wheel animations.
    /// </summary>
    void FixedUpdate()
    {
        // Convert movement input to local direction vector
        Vector3 moveDir = new Vector3(0, 0, movement);

        // === REAR WHEEL STEERING ===
        // Rear wheels steer (like a forklift) for tight turning radius
        float maxWheelSteer = 30f; // Maximum steering angle in degrees
        float wheelLerpSpeed = 8f; // Steering interpolation speed (higher = more responsive)
        
        // Calculate desired steering angle from input, inverted for correct direction
        float desiredSteer = -Mathf.Clamp(rotation.x * maxWheelSteer, -maxWheelSteer, maxWheelSteer);

        // Smoothly interpolate steering angles toward target (Y-axis rotation)
        rearLeftSteerAngle = Mathf.LerpAngle(rearLeftSteerAngle, desiredSteer, Time.fixedDeltaTime * wheelLerpSpeed);
        rearRightSteerAngle = Mathf.LerpAngle(rearRightSteerAngle, desiredSteer, Time.fixedDeltaTime * wheelLerpSpeed);

        // Apply both steering (Y) and rolling spin (X) to rear wheels
        // Kept separate to prevent gimbal lock and rotation conflicts
        if (rearLeftWheel != null)
        {
            rearLeftWheel.transform.localRotation = Quaternion.Euler(rearLeftSpinAngle, rearLeftSteerAngle, 0f);
        }

        if (rearRightWheel != null)
        {
            rearRightWheel.transform.localRotation = Quaternion.Euler(rearRightSpinAngle, rearRightSteerAngle, 0f);
        }

        // === PHYSICS-BASED MOVEMENT ===
        // Apply instant velocity change for responsive forklift movement
        rb.AddForce(transform.TransformDirection(moveDir), ForceMode.VelocityChange);
        
        // Modulate engine pitch based on throttle input (simulates RPM)
        audioController.ChangeEngineIdlePitch(0.5f + Mathf.Abs(movement) * 0.5f);

        // Calculate local forward velocity for direction-based steering
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float forwardVel = localVel.z;

        // === FORKLIFT BODY ROTATION (STEERING) ===
        // Only rotate forklift body when moving (prevents spinning in place)
        float moveThreshold = 0.1f; // Minimum velocity to allow steering
        float steeringSpeed = 0.5f; // Steering rotation speed multiplier
        
        if (rb != null)
        {
            if (rb.linearVelocity.sqrMagnitude > moveThreshold * moveThreshold)
            {
                // Reverse steering direction when moving backward
                float dir = forwardVel < 0f ? -1f : 1f;
                transform.Rotate(0f, rotation.x * Time.fixedDeltaTime * 100f * dir * steeringSpeed, 0f);
            }
        }

        // === FORK OPERATIONS ===
        // Apply calculated lifting movement from input
        if (lifting != Vector3.zero)
        {
            // Apply lifting movement and clamp to physical limits
            Vector3 newPosition = forks.transform.localPosition + lifting;
            newPosition.y = Mathf.Clamp(newPosition.y, 2.2f, 8f);
            forks.transform.localPosition = newPosition;
            Debug.Log(lifting);
            
            // Play hydraulic sound (forward or reverse pitch based on direction)
            if(lifting.y > 0)
            {
                audioController.PlayForkLiftSound();
            }
            else
            {
                audioController.PlayForkLiftSoundReverse();
            }
        }
        else
        {
            // Stop hydraulic sound when not lifting
            audioController.StopForkLiftSound();
        }
        
        // Apply calculated tilting movement from input
        if (tilting != Vector3.zero)
        {
            Vector3 currentEuler = forks.transform.localEulerAngles;
            float currentTiltX = currentEuler.x;
            
            // Normalize angle to -180 to 180 range for proper clamping
            if (currentTiltX > 180f) currentTiltX -= 360f;
            
            // Apply tilt and clamp to safe operating range
            float newTiltX = Mathf.Clamp(currentTiltX + tilting.x, -15f, 15f);
            forks.transform.localEulerAngles = new Vector3(newTiltX, currentEuler.y, currentEuler.z);
        }

        // === CAMERA ZOOM AND OBSTRUCTION HANDLING ===
        if (forkliftCamera != null)
        {
            Vector3 camPos = forkliftCamera.transform.localPosition;

            // Camera distance parameters
            float minDistance = 2f;  // Minimum zoom distance
            float maxDistance = cameraDistance; // Maximum zoom distance
            float zoomSensitivity = 0.75f; // Zoom input multiplier
            float smoothSpeed = 5f; // Camera position smoothing speed

            // Apply zoom input to target distance
            targetCameraDistance -= zoomAmount * zoomSensitivity;
            targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);

            // Raycast backward from camera pivot to detect obstructions
            // Ignores forklift and player layers to prevent self-collision
            ray = new Ray(RotatePoint.transform.position, -RotatePoint.transform.forward);
            float obstructionDistance = float.PositiveInfinity;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, ~LayerMask.GetMask("Forklift", "Player")))
            {
                obstructionDistance = hitInfo.distance;
            }

            // Clamp camera distance if obstructed
            float allowedDistance = float.IsPositiveInfinity(obstructionDistance) ? targetCameraDistance : Mathf.Min(targetCameraDistance, obstructionDistance);

            // Smoothly interpolate actual camera distance (automatically moves back out when obstruction clears)
            actualCameraDistance = Mathf.Lerp(actualCameraDistance, allowedDistance, Time.fixedDeltaTime * smoothSpeed);

            // Apply final camera position (negative Z in local space)
            forkliftCamera.transform.localPosition = new Vector3(camPos.x, camPos.y, -actualCameraDistance);
        }

        // === CAMERA ROTATION ===
        // Rotate camera pivot with mouse/stick input (yaw = left/right, pitch = up/down)
        float sensitivity = rotateSpeed;

        // Calculate rotation deltas from input
        float yawDelta = camDelta.x * sensitivity;
        float pitchDelta = -camDelta.y * sensitivity; // Inverted so mouse up looks up

        // Get current rotation and normalize pitch to -180 to 180 range
        Vector3 current = RotatePoint.transform.localEulerAngles;
        float currentPitch = current.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        // Apply rotation with pitch clamping to prevent over-rotation
        float newPitch = Mathf.Clamp(currentPitch + pitchDelta, -45f, 45f);
        float newYaw = current.y + yawDelta;

        // Set final camera pivot rotation
        RotatePoint.transform.localEulerAngles = new Vector3(newPitch, newYaw, current.z);

        // === WHEEL ROTATION ANIMATION ===
        // Update wheel spin based on actual forward velocity
        RotateWheels(forwardVel);




    }


    /// <summary>
    /// Calculates and applies wheel rotation animation based on forward velocity.
    /// Converts linear velocity to angular rotation using wheel radius.
    /// Maintains separate spin and steering angles to prevent gimbal lock.
    /// </summary>
    /// <param name="velocity">Forward velocity in meters per second</param>
    void RotateWheels(float velocity)
    {
        // Estimate wheel radius from mesh bounds (default to 0.5m if not found)
        GameObject sample = rearLeftWheel ?? rearRightWheel ?? frontLeftWheel ?? frontRightWheel;
        float radius = 0.5f;
        if (sample != null)
        {
            var rend = sample.GetComponent<Renderer>();
            if (rend != null)
            {
                // Use largest extent as radius approximation
                radius = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.y, rend.bounds.extents.z);
                if (radius <= 0f) radius = 0.5f;
            }
        }

        // Convert linear velocity (m/s) to RPM
        // Formula: rpm = (velocity / circumference) * 60
        float rpm = velocity / (2f * Mathf.PI * radius) * 60f;

        // Convert RPM to degrees per second
        float degreesPerSecond = rpm * 360f / 60f;

        // Calculate rotation delta for this frame
        float deltaDegrees = degreesPerSecond * Time.fixedDeltaTime;

        // Accumulate spin angles for all wheels (independent of steering)
        rearLeftSpinAngle += deltaDegrees;
        rearRightSpinAngle += deltaDegrees;
        frontLeftSpinAngle += deltaDegrees;
        frontRightSpinAngle += deltaDegrees;
        
        // Apply rolling rotation to rear wheels (X-axis) combined with steering (Y-axis)
        // Using separate tracked angles prevents steering from interfering with spin
        if (rearLeftWheel != null)
        {
            rearLeftWheel.transform.localRotation = Quaternion.Euler(rearLeftSpinAngle, rearLeftSteerAngle, 0f);
        }
        if (rearRightWheel != null)
        {
            rearRightWheel.transform.localRotation = Quaternion.Euler(rearRightSpinAngle, rearRightSteerAngle, 0f);
        }
        
        // Apply rolling rotation to front wheels (no steering on front wheels)
        if (frontLeftWheel != null)
        {
            frontLeftWheel.transform.localRotation = Quaternion.Euler(frontLeftSpinAngle, 0f, 0f);
        }
        if (frontRightWheel != null)
        {
            frontRightWheel.transform.localRotation = Quaternion.Euler(frontRightSpinAngle, 0f, 0f);
        }
    }

}

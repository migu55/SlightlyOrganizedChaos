using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ForkliftController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float rotateSpeed = 1;

    public ForkliftAudioController audioController;
    private float movement;
    private Vector2 rotation;

    private Vector3 lifting;
    private Vector3 tilting;

    private Vector2 camDelta;
    private float zoomAmount;
    // the user's requested zoom distance (positive)
    private float targetCameraDistance;
    // the camera's actual distance after obstruction clamping and smoothing
    private float actualCameraDistance;
    public float cameraDistance = 20f;
    private GameObject RotatePoint;
    private GameObject forkliftCamera;
    private GameObject forks;
    private GameObject rearRightWheel;
    private GameObject rearLeftWheel;
    private GameObject frontLeftWheel;
    private GameObject frontRightWheel;
    Rigidbody rb;

    private Ray ray;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>(); //get rigidbody, responsible for enabling collision with other colliders
        RotatePoint = transform.Find("RotatePoint").gameObject;
        forkliftCamera = transform.Find("RotatePoint").Find("ForkliftCamera").gameObject;
        forks = transform.Find("Lift").gameObject;
        rearRightWheel = transform.Find("Wheel_R_Back").gameObject;
        rearLeftWheel = transform.Find("Wheel_L_Back").gameObject;
        frontLeftWheel = transform.Find("Wheel_L_Front").gameObject;
        frontRightWheel = transform.Find("Wheel_R_Front").gameObject;

        // initialize camera distances
        targetCameraDistance = cameraDistance;
        actualCameraDistance = cameraDistance;
    }

    public void Steering(Vector2 rotationInput)
    {
        rotation = rotationInput;
    }

    public void Move(Vector2 movementInput)
    {
        movement = movementInput.y;
    }

    public void Zoom(Vector2 zoomInput)
    {
        zoomAmount = zoomInput.y;
    }

    public void Interact(bool value)
    {
        var mountForklift = GetComponent<MountForklift>();
        if (GetComponent<MountForklift>().mounted)
        {
            GetComponent<MountForklift>().Dismount();
        }
    }

    public void Forks(Vector2 value)
    {
        float liftAmount = value.y;
        float tiltAmount = value.x;
        float liftSpeed = 2f; // adjust to taste
        float tiltSpeed = 25f; // degrees per second
        float minLiftY = 2.2f;
        float maxLiftY = 8f;
        float minTiltX = -15f;
        float maxTiltX = 15f;

        if (forks == null) return;

        // Apply lifting
        Vector3 lift = Vector3.up * liftAmount * liftSpeed * Time.fixedDeltaTime;
        // Only clamp magnitude when moving upward to prevent getting stuck at max height
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

        // Apply tilting
        Vector3 tilt = Vector3.right * tiltAmount * tiltSpeed * Time.fixedDeltaTime;
        tilt = Vector3.ClampMagnitude(tilt, maxTiltX - forks.transform.localEulerAngles.z);
        tilting = tilt;
    }

    public void Camera(Vector2 camDelta)
    {
        this.camDelta = camDelta;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(0, 0, movement);

        // steer rear wheels based on horizontal rotation input
        float maxWheelSteer = 30f; // degrees
        float wheelLerpSpeed = 8f; // higher = snappier
        // desired steer angle based on input
        float desiredSteer = -Mathf.Clamp(rotation.x * maxWheelSteer, -maxWheelSteer, maxWheelSteer);

        if (rearLeftWheel != null)
        {
            Vector3 e = rearLeftWheel.transform.localEulerAngles;
            // smoothly interpolate the y-angle toward the desired steer
            float newY = Mathf.LerpAngle(e.y, desiredSteer, Time.fixedDeltaTime * wheelLerpSpeed);
            rearLeftWheel.transform.localEulerAngles = new Vector3(e.x, newY, e.z);
        }

        if (rearRightWheel != null)
        {
            Vector3 e = rearRightWheel.transform.localEulerAngles;
            float newY = Mathf.LerpAngle(e.y, desiredSteer, Time.fixedDeltaTime * wheelLerpSpeed);
            rearRightWheel.transform.localEulerAngles = new Vector3(e.x, newY, e.z);
        }

        rb.AddForce(transform.TransformDirection(moveDir), ForceMode.VelocityChange);
        audioController.ChangeEngineIdlePitch(0.5f + Mathf.Abs(movement) * 0.5f);

        float moveThreshold = 0.1f;
        float steeringSpeed = 0.5f;
        if (rb != null)
        {
            // use local forward velocity to determine direction
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            float forwardVel = localVel.z;

            if (rb.linearVelocity.sqrMagnitude > moveThreshold * moveThreshold)
            {
                float dir = forwardVel < 0f ? -1f : 1f;
                transform.Rotate(0f, rotation.x * Time.fixedDeltaTime * 100f * dir * steeringSpeed, 0f);
            }
        }

        // Apply lifting
        if (lifting != Vector3.zero)
        {
            Vector3 newPosition = forks.transform.localPosition + lifting;
            newPosition.y = Mathf.Clamp(newPosition.y, 2.2f, 8f);
            forks.transform.localPosition = newPosition;
            Debug.Log(lifting);
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
            audioController.StopForkLiftSound();
        }
        // Apply tilting
        if (tilting != Vector3.zero)
        {
            Vector3 currentEuler = forks.transform.localEulerAngles;
            float currentTiltX = currentEuler.x;
            if (currentTiltX > 180f) currentTiltX -= 360f;
            float newTiltX = Mathf.Clamp(currentTiltX + tilting.x, -15f, 15f);
            forks.transform.localEulerAngles = new Vector3(newTiltX, currentEuler.y, currentEuler.z);
        }

        if (forkliftCamera != null)
        {
            Vector3 camPos = forkliftCamera.transform.localPosition;

            // Tuneable values
            float minDistance = 2f; // closest allowed by user
            float maxDistance = cameraDistance; // farthest allowed by user
            float zoomSensitivity = 0.75f; // how strongly input affects target
            float smoothSpeed = 5f; // smoothing for actual camera movement

            // Apply zoom input directly to targetCameraDistance each frame
            targetCameraDistance -= zoomAmount * zoomSensitivity;
            targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);

            // Raycast from the RotatePoint toward the camera to detect obstructions
            ray = new Ray(RotatePoint.transform.position, -RotatePoint.transform.forward);
            float obstructionDistance = float.PositiveInfinity;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, ~LayerMask.GetMask("Forklift")))
            {
                obstructionDistance = hitInfo.distance;
            }

            // allowedDistance is the temporary clamped distance (if something is in the way)
            float allowedDistance = float.IsPositiveInfinity(obstructionDistance) ? targetCameraDistance : Mathf.Min(targetCameraDistance, obstructionDistance);

            // smooth actual camera distance towards the allowed distance; when obstruction clears
            // allowedDistance will equal targetCameraDistance and the camera will move back out
            actualCameraDistance = Mathf.Lerp(actualCameraDistance, allowedDistance, Time.fixedDeltaTime * smoothSpeed);

            // apply camera local position (Z is negative in this setup)
            forkliftCamera.transform.localPosition = new Vector3(camPos.x, camPos.y, -actualCameraDistance);
        }

        // rotate RotatePoint with mouse movement (left/right -> yaw, up/down -> pitch)
        float sensitivity = rotateSpeed; // tweak to taste

        // compute yaw and pitch changes
        float yawDelta = camDelta.x * sensitivity;
        float pitchDelta = -camDelta.y * sensitivity; // invert Y so moving mouse up looks up

        // get current local angles and convert pitch to -180..180 range for clamping
        Vector3 current = RotatePoint.transform.localEulerAngles;
        float currentPitch = current.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        // apply and clamp pitch
        float newPitch = Mathf.Clamp(currentPitch + pitchDelta, -45f, 45f);
        float newYaw = current.y + yawDelta;

        // set new local rotation
        RotatePoint.transform.localEulerAngles = new Vector3(newPitch, newYaw, current.z);


        RotateWheels(rb.linearVelocity.z);




    }


    void RotateWheels(float velocity)
    {
        // pick a sample wheel to estimate radius (fallback to 0.5m)
        GameObject sample = rearLeftWheel ?? rearRightWheel ?? frontLeftWheel ?? frontRightWheel;
        float radius = 0.5f;
        if (sample != null)
        {
            var rend = sample.GetComponent<Renderer>();
            if (rend != null)
            {
                // use the largest extent as an approximation of radius
                radius = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.y, rend.bounds.extents.z);
                if (radius <= 0f) radius = 0.5f;
            }
        }

        // convert linear velocity (m/s) to RPM:
        // rpm = (velocity / (2 * PI * radius)) * 60
        float rpm = (velocity / (2f * Mathf.PI * radius)) * 60f;

        // convert RPM to degrees per second (360 degrees per revolution, 60 seconds per minute)
        float degreesPerSecond = rpm * 360f / 60f; // = rpm * 6

        // apply rotation this frame around local X axis
        float deltaDegrees = degreesPerSecond * Time.fixedDeltaTime;

        // if (rearLeftWheel != null) rearLeftWheel.transform.Rotate(deltaDegrees, 0f, 0f, Space.Self);
        // if (rearRightWheel != null) rearRightWheel.transform.Rotate(deltaDegrees, 0f, 0f, Space.Self);
        if (frontLeftWheel != null) frontLeftWheel.transform.Rotate(deltaDegrees, 0f, 0f, Space.Self);
        if (frontRightWheel != null) frontRightWheel.transform.Rotate(deltaDegrees, 0f, 0f, Space.Self);
    }

}

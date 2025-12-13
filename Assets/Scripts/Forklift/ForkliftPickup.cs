using UnityEngine;

/// <summary>
/// Manages pallet attachment to forklift forks using physics joints.
/// Creates ConfigurableJoint connections with spring/damping to keep pallets stable during lifting and movement.
/// Automatically attaches pallets when they enter the fork trigger zone.
/// </summary>
public class ForkliftPickup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform of the forks where the pallet connects.")]
    public Transform forkTransform;

    [Header("Joint Settings")]
    [Tooltip("Spring force pulling pallet to forks (higher = tighter connection).")]
    public float attachSpring = 800f;

    [Tooltip("Damping to prevent oscillation (higher = less bouncy).")]
    public float attachDamping = 120f;

    [Tooltip("Force threshold to break the joint if collision is too strong.")]
    public float breakForce = 5000f;

    [Tooltip("Max distance from fork anchor before joint breaks.")]
    public float maxDistance = 1f;

    // Currently active joint connecting pallet to forks
    private ConfigurableJoint currentJoint;
    
    // Reference to the currently attached pallet's rigidbody
    private Rigidbody currentPallet;

    /// <summary>
    /// Checks if the forklift is currently carrying the specified pallet.
    /// </summary>
    /// <param name="pallet">The pallet rigidbody to check</param>
    /// <returns>True if this pallet is currently attached to the forks</returns>
    public bool IsCarrying(Rigidbody pallet)
    {
        return currentPallet == pallet;
    }

    /// <summary>
    /// Attaches a pallet to the forks using a ConfigurableJoint with spring forces.
    /// Creates a kinematic rigidbody on the forks if needed, then configures a joint
    /// with limited movement and rotation to keep the pallet stable but physically realistic.
    /// </summary>
    /// <param name="pallet">The pallet's rigidbody to attach</param>
    public void AttachPallet(Rigidbody pallet)
    {
        if (pallet == null || forkTransform == null) return;

        currentPallet = pallet;

        // Ensure fork has a kinematic rigidbody to connect joint to
        Rigidbody forkRb = forkTransform.GetComponent<Rigidbody>();
        if (forkRb == null)
        {
            // Add kinematic rigidbody to forks - moves with animation/script, not affected by physics
            forkRb = forkTransform.gameObject.AddComponent<Rigidbody>();
            forkRb.isKinematic = true;
            Debug.Log("Added kinematic Rigidbody to forks");
        }

        // Create a ConfigurableJoint on the pallet GameObject
        currentJoint = pallet.gameObject.AddComponent<ConfigurableJoint>();
        currentJoint.connectedBody = forkRb;

        // Configure linear motion - limited movement with spring tension along all axes
        currentJoint.xMotion = ConfigurableJointMotion.Limited;
        currentJoint.yMotion = ConfigurableJointMotion.Limited;
        currentJoint.zMotion = ConfigurableJointMotion.Limited;
        
        // Allow some rotation for realism but keep it limited to prevent spinning
        currentJoint.angularXMotion = ConfigurableJointMotion.Limited;
        currentJoint.angularYMotion = ConfigurableJointMotion.Limited;
        currentJoint.angularZMotion = ConfigurableJointMotion.Limited;

        // Set anchor point on pallet at its center
        currentJoint.anchor = Vector3.zero;
        currentJoint.autoConfigureConnectedAnchor = true;

        // Configure linear movement limits (how far pallet can drift from forks)
        var linearLimit = new SoftJointLimit { limit = maxDistance, bounciness = 0f };
        currentJoint.linearLimit = linearLimit;

        // Configure angular limits (restricts rotation to 5 degrees in all directions)
        var angularLimit = new SoftJointLimit { limit = 5f, bounciness = 0f };
        currentJoint.lowAngularXLimit = angularLimit;
        currentJoint.highAngularXLimit = angularLimit;
        currentJoint.angularYLimit = angularLimit;
        currentJoint.angularZLimit = angularLimit;

        // Set spring parameters for position stability - pulls pallet toward fork position
        var spring = new JointDrive
        {
            positionSpring = attachSpring,
            positionDamper = attachDamping,
            maximumForce = Mathf.Infinity
        };
        currentJoint.xDrive = spring;
        currentJoint.yDrive = spring;
        currentJoint.zDrive = spring;

        // Set angular spring at half strength to prevent spinning while allowing natural settling
        var angularSpring = new JointDrive
        {
            positionSpring = attachSpring * 0.5f,
            positionDamper = attachDamping * 0.5f,
            maximumForce = Mathf.Infinity
        };
        currentJoint.angularXDrive = angularSpring;
        currentJoint.angularYZDrive = angularSpring;

        // Set break thresholds - joint breaks if forces/torques exceed these values
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakForce;

        Debug.Log($"Pallet attached to forklift with ConfigurableJoint (spring: {attachSpring}, damping: {attachDamping})");
    }

    /// <summary>
    /// Detaches the currently attached pallet by destroying the joint.
    /// The pallet becomes fully physics-driven again after detachment.
    /// </summary>
    public void DetachPallet()
    {
        // Destroy the joint component if it exists
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        // Clear pallet reference
        currentPallet = null;
        Debug.Log("Pallet detached from forklift");
    }

    /// <summary>
    /// Trigger callback when a collider enters the fork zone.
    /// Automatically attaches pallets that enter if nothing is currently attached.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone</param>
    public void OnTriggerEnterFork(Collider other)
    {
        // Don't attach if already carrying something
        if (currentJoint != null) return;

        // Check if the entering object is a pallet with a rigidbody
        if (other.attachedRigidbody != null && other.CompareTag("Pallet"))
        {
            AttachPallet(other.attachedRigidbody);
        }
    }

    /// <summary>
    /// Trigger callback when a collider exits the fork zone.
    /// Detaches the pallet if it's the currently attached one leaving the zone.
    /// </summary>
    /// <param name="other">The collider that exited the trigger zone</param>
    public void OnTriggerExitFork(Collider other)
    {
        if (currentPallet == null) return;

        // Detach if the exiting object is the currently attached pallet
        if (other.attachedRigidbody == currentPallet)
        {
            Debug.Log("Pallet exited fork trigger, detaching");
            DetachPallet();
        }
    }
}

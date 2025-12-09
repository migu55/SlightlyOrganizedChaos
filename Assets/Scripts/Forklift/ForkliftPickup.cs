using UnityEngine;

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

    private ConfigurableJoint currentJoint;
    private Rigidbody currentPallet;

    public bool IsCarrying(Rigidbody pallet)
    {
        return currentPallet == pallet;
    }

    /// <summary>
    /// Attaches a pallet using a spring joint, keeping it dynamic but tethered to the forks.
    /// </summary>
    public void AttachPallet(Rigidbody pallet)
    {
        if (pallet == null || forkTransform == null) return;

        currentPallet = pallet;

        // Ensure fork has a kinematic rigidbody to connect joint to
        Rigidbody forkRb = forkTransform.GetComponent<Rigidbody>();
        if (forkRb == null)
        {
            forkRb = forkTransform.gameObject.AddComponent<Rigidbody>();
            forkRb.isKinematic = true; // kinematic so it moves with script, not physics
        }

        // Create a ConfigurableJoint on the pallet
        currentJoint = pallet.gameObject.AddComponent<ConfigurableJoint>();
        currentJoint.connectedBody = forkRb;

        // Configure motion - limited movement with spring tension
        currentJoint.xMotion = ConfigurableJointMotion.Limited;
        currentJoint.yMotion = ConfigurableJointMotion.Limited;
        currentJoint.zMotion = ConfigurableJointMotion.Limited;
        
        // Allow some rotation for realism but dampen it
        currentJoint.angularXMotion = ConfigurableJointMotion.Limited;
        currentJoint.angularYMotion = ConfigurableJointMotion.Limited;
        currentJoint.angularZMotion = ConfigurableJointMotion.Limited;

        // Set anchor point on pallet (center)
        currentJoint.anchor = Vector3.zero;
        currentJoint.autoConfigureConnectedAnchor = true;

        // Set linear limits
        var linearLimit = new SoftJointLimit { limit = maxDistance, bounciness = 0f };
        currentJoint.linearLimit = linearLimit;

        // Set angular limits (small angles)
        var angularLimit = new SoftJointLimit { limit = 5f, bounciness = 0f };
        currentJoint.lowAngularXLimit = angularLimit;
        currentJoint.highAngularXLimit = angularLimit;
        currentJoint.angularYLimit = angularLimit;
        currentJoint.angularZLimit = angularLimit;

        // Set spring parameters for tight connection
        var spring = new JointDrive
        {
            positionSpring = attachSpring,
            positionDamper = attachDamping,
            maximumForce = Mathf.Infinity
        };
        currentJoint.xDrive = spring;
        currentJoint.yDrive = spring;
        currentJoint.zDrive = spring;

        // Angular damping to prevent spinning
        var angularSpring = new JointDrive
        {
            positionSpring = attachSpring * 0.5f,
            positionDamper = attachDamping * 0.5f,
            maximumForce = Mathf.Infinity
        };
        currentJoint.angularXDrive = angularSpring;
        currentJoint.angularYZDrive = angularSpring;

        // Set break force/torque
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakForce;

    }

    /// <summary>
    /// Detaches the pallet by destroying the joint and restoring physics.
    /// </summary>
    public void DetachPallet()
    {
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        currentPallet = null;
    }

    /// <summary>
    /// Called by trigger or interaction system when pallet enters fork zone.
    /// </summary>
    public void OnTriggerEnterFork(Collider other)
    {
        if (currentJoint != null) return; // already carrying something

        if (other.attachedRigidbody != null && other.CompareTag("Pallet"))
        {
            AttachPallet(other.attachedRigidbody);
        }
    }

    /// <summary>
    /// Called by trigger or interaction system when pallet exits fork zone.
    /// </summary>
    public void OnTriggerExitFork(Collider other)
    {
        if (currentPallet == null) return;

        if (other.attachedRigidbody == currentPallet)
        {
            DetachPallet();
        }
    }
}

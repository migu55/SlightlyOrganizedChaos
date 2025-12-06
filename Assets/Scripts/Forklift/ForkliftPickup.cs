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

        // pallet.isKinematic = false; // ensure pallet is dynamic
        currentPallet = pallet;

        // // Create a ConfigurableJoint on the pallet
        // currentJoint = pallet.gameObject.AddComponent<ConfigurableJoint>();

        // // Set the connected rigid body to the forklift (or a body on the forks)
        // Rigidbody forkliftRb = GetComponent<Rigidbody>();
        // if (forkliftRb != null)
        // {
        //     currentJoint.connectedBody = forkliftRb;
        // }

        // // Configure joint to allow free movement but with spring tension
        // // Lock all linear and angular axes - the spring will pull back to anchor
        // currentJoint.xMotion = ConfigurableJointMotion.Limited;
        // currentJoint.yMotion = ConfigurableJointMotion.Limited;
        // currentJoint.zMotion = ConfigurableJointMotion.Limited;
        // currentJoint.angularXMotion = ConfigurableJointMotion.Free;
        // currentJoint.angularYMotion = ConfigurableJointMotion.Free;
        // currentJoint.angularZMotion = ConfigurableJointMotion.Free;

        // // Set anchor point on pallet (local origin is usually fine)
        // currentJoint.anchor = Vector3.zero;

        // // Set connected anchor relative to forklift's rigidbody
        // // Adjust this offset to match where the pallet should sit on the forks
        // if (forkTransform != null)
        // {
        //     Vector3 relativePos = forkliftRb != null
        //         ? forkliftRb.transform.InverseTransformPoint(forkTransform.position)
        //         : Vector3.zero;
        //     currentJoint.connectedAnchor = relativePos;
        // }

        // // Set spring parameters for gentle tension
        // var linearLimit = new SoftJointLimit { limit = maxDistance };
        // currentJoint.linearLimit = linearLimit;

        // var spring = new JointDrive
        // {
        //     positionSpring = attachSpring,
        //     positionDamper = attachDamping,
        //     maximumForce = Mathf.Infinity
        // };
        // currentJoint.xDrive = spring;
        // currentJoint.yDrive = spring;
        // currentJoint.zDrive = spring;

        // // Angular damping so pallet doesn't spin wildly
        // var angularSpring = new JointDrive
        // {
        //     positionSpring = 0f,
        //     positionDamper = 50f,
        //     maximumForce = Mathf.Infinity
        // };
        // currentJoint.angularXDrive = angularSpring;
        // currentJoint.angularYZDrive = angularSpring;

        // // Set break force/torque
        // currentJoint.breakForce = breakForce;
        // currentJoint.breakTorque = breakForce;

        // Debug.Log($"Pallet attached to forklift with spring joint (spring: {attachSpring}, damping: {attachDamping})");
    }

    /// <summary>
    /// Detaches the pallet by destroying the joint and restoring physics.
    /// </summary>
    public void DetachPallet()
    {
        // currentPallet.isKinematic = true;
        currentPallet = null;
        // if (currentPallet == null) return;

        // if (currentJoint != null)
        // {
        //     Destroy(currentJoint);
        //     currentJoint = null;
        // }

        // // Pallet is already dynamic; joint destruction leaves it free
        // currentPallet = null;
        Debug.Log("Pallet detached from forklift");
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
            Debug.Log("Pallet exited fork trigger, detaching");
            DetachPallet();
        }
    }
}

using UnityEngine;

public class PalletCollisionWatcher : MonoBehaviour
{
    public ForkliftPickup forkliftPickup; // reference to the current pickup script
    public float detachThreshold = 2f;    // downward velocity threshold for collisions
    public float bottomTolerance = 0.05f; // distance from pallet bottom to consider contact "bottom hit"

    private Rigidbody rb;
    private Collider palletCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        palletCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckDetach(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckDetach(collision);
    }

    private void CheckDetach(Collision collision)
{
    if (forkliftPickup == null || !forkliftPickup.IsCarrying(rb)) return;

    // Detach if downward velocity is strong
    if (collision.relativeVelocity.y < -detachThreshold)
    {
        forkliftPickup.DetachPallet();
        return;
    }

    // Compute the bottom of the pallet once
    float bottomY = float.MaxValue;
    foreach (Collider col in GetComponentsInChildren<Collider>())
    {
        bottomY = Mathf.Min(bottomY, col.bounds.min.y);
    }

    // Detach if any contact point is at the bottom
    foreach (ContactPoint contact in collision.contacts)
    {
        // Convert world point to local space relative to the pallet
        Vector3 localPoint = transform.InverseTransformPoint(contact.point);
        Debug.Log($"Collision contact relative to pallet: {localPoint}");

        if (contact.point.y <= bottomY + bottomTolerance)
        {
            forkliftPickup.DetachPallet();
            break;
        }
    }
}

}

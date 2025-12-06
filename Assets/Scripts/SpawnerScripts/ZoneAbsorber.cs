using UnityEngine;

/// <summary>
/// Attach this script to a zone collider (zoneA or zoneB). When a Pallet enters, it notifies the assigned TruckReceiver to absorb the pallet.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZoneAbsorber : MonoBehaviour
{
    [Tooltip("Assign the TruckReceiver (on the truck) that should absorb pallets entering this zone.")]
    public TruckReceiver truckReceiver;

    public GameObject hostTruck;

    // Dedupe fields: track last absorb timestamp per pallet instance ID to ignore repeated triggers
    private System.Collections.Generic.Dictionary<int, float> _lastAbsorbTime = new System.Collections.Generic.Dictionary<int, float>();
    private const float DEDUPE_WINDOW = 0.2f; // seconds

    private void OnTriggerEnter(Collider other)
    {
        if (hostTruck == null) return;
        TruckReceiver hostReceiver = hostTruck.GetComponent<TruckReceiver>();
        if (hostReceiver == null || !hostReceiver.enabled) return;
        if (truckReceiver == null) return;

        // Resolve the pallet deterministically to avoid matching other clones.
        Pallet pallet = null;
        GameObject resolvedPalletObj = null;

        // 1) Prefer the collider's own GameObject if it has a Pallet component (collider on root)
        if (other.gameObject.TryGetComponent<Pallet>(out pallet))
        {
            resolvedPalletObj = other.gameObject;
        }
        else
        {
            // 2) If collider has an attached Rigidbody, prefer the Rigidbody's GameObject (common parent root)
            if (other.attachedRigidbody != null)
            {
                var rbGo = other.attachedRigidbody.gameObject;
                if (rbGo.TryGetComponent<Pallet>(out pallet))
                {
                    resolvedPalletObj = rbGo;
                }
            }
        }

        // 3) Fallback: check the root GameObject, but only accept if it directly has a Pallet component.
        if (pallet == null)
        {
            var rootGo = other.transform.root.gameObject;
            if (rootGo != other.gameObject && rootGo != (other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : null))
            {
                // only check root as a last resort
                if (rootGo.TryGetComponent<Pallet>(out pallet))
                {
                    resolvedPalletObj = rootGo;
                }
            }
        }

        if (pallet != null && resolvedPalletObj != null && pallet.CompareTag("Pallet"))
        {
            // Diagnostic log: show which specific collider caused this trigger so we can see duplicate calls
            string attachedRbName = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject.name : "(none)";
            Debug.Log($"ZoneAbsorber: Trigger by collider id={other.GetInstanceID()} name={other.name} root={other.transform.root.name} attachedRb={attachedRbName} -> palletId={pallet.gameObject.GetInstanceID()} palletName={pallet.gameObject.name} (resolvedFrom={resolvedPalletObj.name})");

            int palletId = pallet.gameObject.GetInstanceID();
            float now = Time.time;
            if (_lastAbsorbTime.TryGetValue(palletId, out float last) && now - last < DEDUPE_WINDOW)
            {
                Debug.Log($"ZoneAbsorber: Ignoring duplicate trigger for palletId={palletId} (dt={now-last:F3}s)");
                return;
            }

            _lastAbsorbTime[palletId] = now;

            truckReceiver.AbsorbPallet(pallet.gameObject);
        }
    }
}

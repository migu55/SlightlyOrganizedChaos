// filepath: /Users/BCIT/Desktop/GameDevTerm1/GameProjectChaos/maingamerepo/Assets/Scripts/SpawnerScripts/Pallet.cs
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Pallet : MonoBehaviour
{
    // Type of box
    public string typeOfBox;

    // List of boxes in this pallet
    public List<BoxData> palletBoxes = new List<BoxData>();

    // Amount of pallets
    public int amtOfPallet;

    [SerializeField]
    public List<Collider> zones = new List<Collider>();

    // New inspector-assignable prefabs for pallet-specific boxes A, B, C
    [Header("Pallet Box Prefabs")]
    [Tooltip("Prefab to use for Box type A when this pallet instantiates boxes")]
    public GameObject boxPrefabA;
    [Tooltip("Prefab to use for Box type B when this pallet instantiates boxes")]
    public GameObject boxPrefabB;
    [Tooltip("Prefab to use for Box type C when this pallet instantiates boxes")]
    public GameObject boxPrefabC;

    [Header("Detection")]
    [Tooltip("Layer mask used to detect existing Box objects when filling zones. Assign the layer(s) your Box prefabs use.")]
    public LayerMask boxLayerMask = ~0; // default: everything

    // Fall detection configuration - when exceeded, boxes will be released
    [Header("Fall Detection")]
    [Tooltip("Angle threshold in degrees to trigger falling. Uses the angle between the pallet's up vector and world up (0 = upright).")]
    private float fallThreshold = 60f;
    [Tooltip("How often (seconds) to check pallet tilt.")]
    public float fallCheckInterval = 0.25f;
    [Tooltip("Nudge distance to move a box upward if it overlaps geometry before enabling physics.")]
    public float overlapNudge = 0.05f;
    [Tooltip("Optional impulse applied to boxes when they are released.")]
    public float releaseImpulse = 1f;

    // Simplified: only Tilt detection is supported to avoid yaw-related false positives
    public enum DetectionMode { Tilt }
    public DetectionMode detectionMode = DetectionMode.Tilt;

    [Header("Triggering")]
    [Tooltip("If true, when this pallet exceeds the threshold it will make all pallets in the scene fall. If false, only this pallet will release.")]
    public bool triggerAllPallets = false;

    [Header("Startup")]
    [Tooltip("Delay (seconds) after spawn before fall detection becomes active to avoid immediate triggers on spawn.")]
    public float startupDelay = 0.1f;

    // internal guard so we only trigger the falling once (until explicitly reset)
    private bool hasFallen = false;

    // Baseline orientation recorded at startup (used so newly-spawned pallets don't immediately trigger)
    private Quaternion initialLocalRotation;
    private bool canDetectFall = false;

    // small helper to avoid tilt accumulation due to tiny jitter: when pallet is stable for this many seconds
    private float tiltStableTimer = 0f;
    private const float stableResetTime = 0.5f; // seconds of low movement before we rebaseline

    // record when this pallet was created (used to avoid affecting pallets spawned after a broadcast)
    private float spawnTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FillEmptyZonesWithBoxes();

        // record baseline and start a short delay before enabling detection
        initialLocalRotation = transform.localRotation;
        canDetectFall = false;
        spawnTime = Time.time;
        StartCoroutine(EnableFallDetectionAfterDelay());

        // start watching for tilt -> fall
        StartCoroutine(WatchTiltAndRelease());
    }

    // enable fall detection after a short startup delay so spawned pallets don't immediately trigger
    IEnumerator EnableFallDetectionAfterDelay()
    {
        yield return new WaitForSeconds(startupDelay);
        initialLocalRotation = transform.localRotation;
        canDetectFall = true;
    }

    // Update is called once per frame
    void Update()
    {
        // (intentionally empty; detection runs in coroutine)
    }

    /// <summary>
    /// [OPTIONAL] Initialize this Pallet from a PalletData instance, spawning Box GameObjects as children.
    /// </summary>
    public void InitializeFromPalletData(PalletData data, GameObject boxPrefab)
    {
        typeOfBox = data.typeOfBox;
        amtOfPallet = data.amtOfPallet;

        // Always copy the data list so pallet.palletBoxes reflects the underlying data even if boxPrefab is null
        palletBoxes = new List<BoxData>();
        if (data.boxDataList != null)
        {
            palletBoxes.AddRange(data.boxDataList);
        }

        if (data.boxDataList != null && data.boxDataList.Count > 0)
        {
            // Instantiate visible box GameObjects as children, choosing prefab per BoxData.typeOfBox
            foreach (BoxData boxData in data.boxDataList)
            {
                // choose the best prefab: pallet-local A/B/C or the provided fallback boxPrefab
                GameObject prefabToUse = GetPrefabForType(boxData?.typeOfBox, boxPrefab);
                if (prefabToUse == null)
                    continue; // no prefab available, skip visual instantiation

                GameObject boxObj = Object.Instantiate(prefabToUse, this.transform);
                // Ensure the spawned box becomes a child of the pallet and is kinematic so it doesn't fall/move by physics
                MakeBoxKinematic(boxObj);
                // Rotate the spawned box 90 degrees on Y (local rotation relative to the pallet)
                boxObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                Box boxScript = boxObj.GetComponent<Box>();
                if (boxScript == null)
                    boxScript = boxObj.AddComponent<Box>();
                boxScript.typeOfBox = boxData.typeOfBox;
                // Note: palletBoxes already contains the BoxData entries from above
            }
        }

        // reset baseline detection when re-initializing visuals/data
        initialLocalRotation = transform.localRotation;
        canDetectFall = false;
        StartCoroutine(EnableFallDetectionAfterDelay());
    }

    // Ensure the spawned box has a Rigidbody and is kinematic (not affected by physics) so it stays attached to the pallet.
    // If the prefab doesn't include a Rigidbody, one will be added.
    private void MakeBoxKinematic(GameObject boxObj)
    {
        if (boxObj == null) return;

        if (boxObj.transform.parent != this.transform)
            boxObj.transform.SetParent(this.transform);

        Rigidbody rb = boxObj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = boxObj.AddComponent<Rigidbody>();

        // Correct physics behavior
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.detectCollisions = false; // prevents explosion on spawn
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Helper: choose pallet-local prefab based on type string, fallback to provided prefab
    private GameObject GetPrefabForType(string typeOfBox, GameObject fallback)
    {
        if (string.IsNullOrEmpty(typeOfBox))
            return fallback;
        string t = typeOfBox.Trim().ToUpperInvariant();
        if (t.Length == 0) return fallback;
        switch (t[0])
        {
            case 'A':
                return boxPrefabA != null ? boxPrefabA : fallback;
            case 'B':
                return boxPrefabB != null ? boxPrefabB : fallback;
            case 'C':
                return boxPrefabC != null ? boxPrefabC : fallback;
            default:
                return fallback;
        }
    }

    /// <summary>
    /// Non-visual setter: copy PalletData into this Pallet without creating any Box GameObjects.
    /// Use this when you only need the underlying data (e.g., for logic and TruckReceiver) and don't want visuals.
    /// </summary>
    public void SetPalletData(PalletData data)
    {
        if (data == null)
            return;
        typeOfBox = data.typeOfBox;
        amtOfPallet = data.amtOfPallet;
        palletBoxes = new List<BoxData>();
        if (data.boxDataList != null)
            palletBoxes.AddRange(data.boxDataList);

        // reset baseline detection when re-assigning data
        initialLocalRotation = transform.localRotation;
        canDetectFall = false;
        StartCoroutine(EnableFallDetectionAfterDelay());
    }

    /// <summary>
    /// Ensure every zone collider has a Box GameObject. For each zone, if no Box is detected within the zone bounds,
    /// spawn the correct box prefab (using the pallet's per-type prefabs) at the zone center and parent it to this pallet.
    /// Selection of the box type follows this priority:
    /// 1) If there is a corresponding entry in `palletBoxes` at the same index as the zone, use that BoxData.typeOfBox.
    /// 2) Otherwise, use this pallet's `typeOfBox` as a fallback.
    /// If no pallet-local prefab exists for the selected type, the zone is left empty.
    /// </summary>
    public void FillEmptyZonesWithBoxes()
    {
        if (zones == null || zones.Count == 0)
            return;

        for (int i = 0; i < zones.Count; i++)
        {
            Collider zone = zones[i];
            if (zone == null) continue;

            // Use the zone's world AABB to check for any Box components inside it.
            Vector3 center = zone.bounds.center;
            Vector3 halfExtents = zone.bounds.extents;

            // Slightly shrink the extents to avoid catching objects that are exactly touching the boundary.
            Vector3 eps = Vector3.one * 0.01f;
            Vector3 overlapExtents = Vector3.Max(Vector3.zero, halfExtents - eps);

            Collider[] hits = Physics.OverlapBox(center, overlapExtents, zone.transform.rotation, (int)boxLayerMask);
            bool occupied = false;
            foreach (Collider hit in hits)
            {
                if (hit == null) continue;
                Box boxComp = hit.GetComponentInParent<Box>();
                if (boxComp != null)
                {
                    occupied = true;
                    break;
                }
            }

            if (occupied) continue;

            // Determine the desired box type for this zone: prefer the palletBoxes item at same index, else use pallet.typeOfBox
            string desiredType = this.typeOfBox;
            if (i >= 0 && i < palletBoxes.Count && palletBoxes[i] != null && !string.IsNullOrEmpty(palletBoxes[i].typeOfBox))
            {
                desiredType = palletBoxes[i].typeOfBox;
            }

            GameObject prefabToUse = GetPrefabForType(desiredType, null);
            if (prefabToUse == null)
            {
                // No pallet-local prefab for this type; skip spawning so we don't instantiate unexpected visuals.
                continue;
            }

            // Spawn the prefab at the center of the zone and parent it under this pallet so it moves with the pallet.
            GameObject boxObj = Object.Instantiate(prefabToUse, center, Quaternion.identity);
            boxObj.transform.SetParent(this.transform);
            // Make the spawned box kinematic so it doesn't fall off the pallet
            MakeBoxKinematic(boxObj);
            // Rotate the spawned box 90 degrees on Y (local rotation relative to the pallet)
            boxObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            Box boxScript = boxObj.GetComponent<Box>();
            if (boxScript == null)
                boxScript = boxObj.AddComponent<Box>();
            boxScript.typeOfBox = desiredType;
        }
    }

    // dont know if work, will leave here for further testing
    public void deleteBoxFromZone()
    {
        if (zones == null || zones.Count == 0) return;

        int currIndex = 0;

        // iterate zones from 0 upwards until we find a box to delete or run out of zones
        while (currIndex < zones.Count)
        {
            Collider zone = zones[currIndex];
            if (zone == null)
            {
                currIndex++;
                continue;
            }

            Vector3 center = zone.bounds.center;
            Vector3 halfExtents = zone.bounds.extents;
            // small epsilon to avoid boundary catches
            Vector3 eps = Vector3.one * 0.01f;
            Vector3 overlapExtents = Vector3.Max(Vector3.zero, halfExtents - eps);

            // check for any colliders inside this zone
            Collider[] hits = Physics.OverlapBox(center, overlapExtents, zone.transform.rotation);
            bool deletedOne = false;
            foreach (Collider hit in hits)
            {
                if (hit == null) continue;
                Box boxComp = hit.GetComponentInParent<Box>();
                if (boxComp != null)
                {
                    // destroy the GameObject that holds the Box component (visual)
                    Destroy(boxComp.gameObject);
                    deletedOne = true;
                    break;
                }
            }

            if (deletedOne)
            {
                // found & deleted the first occupied zone — done
                return;
            }

            // otherwise check the next zone
            currIndex++;
        }
    }

    IEnumerator WatchTiltAndRelease()
    {
        while (true)
        {
            if (!hasFallen && canDetectFall)
            {
                bool shouldTrigger = false;

                // compute tilt using local rotation so parent rotation doesn't affect detection
                Vector3 currentLocalUp = transform.localRotation * Vector3.up;
                Vector3 initialLocalUp = initialLocalRotation * Vector3.up;
                float tiltAngle = Vector3.Angle(currentLocalUp, initialLocalUp);

                // If pallet hasn't moved much, count time
                if (tiltAngle < 1f) // small jitter threshold for tilt
                {
                    tiltStableTimer += Time.deltaTime;

                    // Optional extra check: if this GameObject has a Rigidbody, consider angular velocity
                    // to ensure we don't rebaseline while still rotating slowly.
                    bool angularlyStable = true;
                    var rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        angularlyStable = rb.angularVelocity.sqrMagnitude < (0.01f * 0.01f);
                    }

                    tiltStableTimer = Mathf.Min(tiltStableTimer, stableResetTime);

                    if (tiltStableTimer >= stableResetTime && angularlyStable)
                    {
                        // Rebaseline full rotation while stable (keeps new upright reference)
                        initialLocalRotation = transform.localRotation;
                        tiltStableTimer = 0f;
                        // Debug.Log($"Pallet '{name}': rebaselined initialLocalRotation due to stability.");
                    }
                }
                else
                {
                    // Reset timer if tilt changes noticeably
                    tiltStableTimer = 0f;
                }

                // Tilt-only detection
                // Debug.Log($"Pallet '{name}' local-tilt={tiltAngle} (threshold={fallThreshold})");
                if (tiltAngle > fallThreshold) shouldTrigger = true;

                if (shouldTrigger)
                {
                    // Always only trigger the pallet that actually tipped.
                    // Debug.Log($"Pallet '{name}' detected tip; triggering local release.");

                    if (canDetectFall)
                    {
                        TriggerFall();
                    }

                    // mark as handled so we don't repeatedly release
                    hasFallen = true;
                }
            }

            yield return new WaitForSeconds(fallCheckInterval);
        }
    }

    /// <summary>
    /// Public trigger to make this pallet fall (idempotent).
    /// </summary>
    public void TriggerFall()
    {
        if (hasFallen) return;
        // safety: only allow falling if this pallet has finished its startup delay
        if (!canDetectFall) return;
        ReleaseAllBoxesSafely();
        hasFallen = true;
    }

    /// <summary>
    /// Reset the fallen state so the pallet can trigger again (useful for testing).
    /// </summary>
    public void ResetFall()
    {
        hasFallen = false;
    }

    /// <summary>
    /// Unparents every visible Box that is a child of this pallet, enables physics safely (avoids overlap explosions)
    /// and applies a small impulse so boxes visually separate from the pallet.
    /// Clears the palletBoxes data list after releasing.
    /// </summary>
    public void ReleaseAllBoxesSafely()
    {
        // Debug.Log($"Pallet '{name}' ReleaseAllBoxesSafely called — palletBoxes count={ (palletBoxes != null ? palletBoxes.Count : 0) }");
        // find all Box components that are children of this pallet
        Box[] boxes = GetComponentsInChildren<Box>(true);
        // Debug.Log($"Pallet '{name}' found {boxes.Length} Box components under this GameObject (GetComponentsInChildren).");
 		// clear pallet data list since boxes are no longer on the pallet
        if (palletBoxes != null)
            palletBoxes.Clear();

        foreach (Box boxComp in boxes)
        {
            if (boxComp == null) continue;
            GameObject boxObj = boxComp.gameObject;

            // log parent info for diagnostics
            string parentName = boxObj.transform.parent != null ? boxObj.transform.parent.name : "(null)";
            // Debug.Log($"Pallet '{name}' considering Box '{boxObj.name}' parent='{parentName}'");

            // only operate on boxes that are currently parented to (or under) this pallet
            if (!boxObj.transform.IsChildOf(this.transform))
            {
                // Debug.Log($"Pallet '{name}' skipping Box '{boxObj.name}' because it's not a child of this pallet.");
                continue;
            }

            // unparent
            // Debug.Log($"Pallet '{name}' releasing Box '{boxObj.name}' (was parent='{parentName}').");
            boxObj.transform.SetParent(null);

            // ensure it has a collider for overlap checking
            Collider col = boxObj.GetComponent<Collider>();
            Rigidbody rb = boxObj.GetComponent<Rigidbody>();
            if (rb == null) rb = boxObj.AddComponent<Rigidbody>();

            // stop any kinematic behaviour and reset velocity
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // check for overlap with world geometry; if overlapping, nudge up a little until clear (bounded attempts)
            if (col != null)
            {
                int attempts = 0;
                while (attempts < 10)
                {
                    Collider[] overlaps = Physics.OverlapBox(col.bounds.center, col.bounds.extents * 0.9f, boxObj.transform.rotation);
                    bool hasOverlap = false;
                    foreach (var o in overlaps)
                    {
                        if (o == null) continue;
                        if (o.transform.IsChildOf(boxObj.transform)) continue; // ignore self
                        // if overlapping anything else, mark
                        hasOverlap = true;
                        break;
                    }

                    if (!hasOverlap) break;

                    // nudge upward slightly to reduce initial penetration before enabling dynamics
                    boxObj.transform.position += Vector3.up * overlapNudge;
                    attempts++;
                }
            }

            // enable physics interactions and gravity
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;

            // apply a small upward/forward impulse so boxes separate nicely from the pallet
            Vector3 impulse = (Vector3.up * 0.5f + transform.forward * 0.2f).normalized * releaseImpulse;
            rb.AddForce(impulse, ForceMode.Impulse);
        }
        
    }
}

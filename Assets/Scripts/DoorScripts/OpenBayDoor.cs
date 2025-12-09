using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OpenBayDoor : MonoBehaviour, Interactable
{
    [Header("Door Animation Settings")]
    [Tooltip("Animator component controlling the door.")]
    [SerializeField] private Animator doorAnimator;

    // Name of the animation states
    private const string openState = "DoorOpen";
    private const string closeState = "DoorClose";

    // Tracks whether the door is currently open
    private bool isOpen = false;
    public bool IsOpen => isOpen;

    [SerializeField] private GameObject SpawnerZone;
    [SerializeField] private MissionBehavior missionBehavior; // assign in inspector or will be found at runtime
	[SerializeField] private GameObject truckSpawner; // just to decrement the int

    // Public storage for the most recently-obtained pallets/boxes from a receiver truck
    [HideInInspector] public List<PalletData> lastReceivedPallets = new List<PalletData>();
    [HideInInspector] public List<BoxData> lastReceivedBoxes = new List<BoxData>();

    // --- New monitoring fields ---
    [Header("Receiver Monitor")]
    [Tooltip("How often (seconds) to check the SpawnerZone for a receiver truck.")]
    [SerializeField] private float receiverCheckInterval = 0.5f;
    private Coroutine receiverMonitorCoroutine = null;
    // -1 means none found
    private int lastDetectedMissionId = -1;
    public int LastDetectedMissionId => lastDetectedMissionId;

    // Call this method to open the door
    public void OpenDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("Animator not set on OpenBayDoor.");
            return;
        }
        Collider zoneCollider = SpawnerZone.GetComponent<Collider>();
        
        // Use an overlap check to reliably find colliders inside the zone (more robust than testing the truck's pivot point)
        Vector3 pad = Vector3.one * 0.5f; // increased pad to be more tolerant of pivot offsets
        Vector3 extents = zoneCollider.bounds.extents + pad;
        Collider[] hits = Physics.OverlapBox(zoneCollider.bounds.center, extents, zoneCollider.transform.rotation, ~0, QueryTriggerInteraction.Collide);
        Debug.Log($"OpenBayDoor: OverlapBox found {hits.Length} colliders in SpawnerZone '{SpawnerZone.name}' (center={zoneCollider.bounds.center}, extents={zoneCollider.bounds.extents}).");
        var trucksFound = new System.Collections.Generic.HashSet<GameObject>();
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            Debug.Log($"OpenBayDoor: Overlap hit: {hit.gameObject.name} (layer={LayerMask.LayerToName(hit.gameObject.layer)})");
            Transform t = hit.transform;
            // walk up the hierarchy to find a parent tagged "Truck"
            while (t != null && !t.CompareTag("Truck")) t = t.parent;
            if (t != null && t.CompareTag("Truck"))
                trucksFound.Add(t.gameObject);
        }
        
        bool empty = false;

        foreach (GameObject truck in trucksFound)
        {
            PalletZoneTracker pzt = truck.GetComponent<PalletZoneTracker>();

            if (pzt != null)
            {
                int total = pzt.GetTotalPalletsInZones();

                if (total == 0)
                {
                    empty = true;
                }
            }
            
        }


        if (!isOpen)
        {
            SFXController.Instance.PlayClip(SFXController.Instance.doorMoved);
            doorAnimator.Play(openState);
            // Only set the close-door display if there is no truck currently occupying the spawner zone
            if (empty)
            {
                SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.SetCloseDoorDisplay();
            }
            isOpen = true;
        }
    }

   

    // Call this method to close the door
    public void CloseDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("Animator not set on OpenBayDoor.");
            return;
        }

        // BEFORE closing: check the spawner zone for a truck that is a receiver
        lastReceivedPallets.Clear();
        lastReceivedBoxes.Clear();

        // If processing finds a blocking condition, do not close
        if (SpawnerZone != null)
        {
            int _processedId;
            if (!TryProcessReceiverInSpawnerZone(out _processedId, false))
                return; // blocked - do not close
        }

        if (isOpen)
        {
            SFXController.Instance.PlayClip(SFXController.Instance.doorMoved);
            doorAnimator.Play(closeState);
            SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
            isOpen = false;
        }
    }

    // New helper: looks for a receiver truck inside the SpawnerZone, enforces checks, copies data, and submits to MissionBehavior.
    // Returns true if it's OK to close the door (either no receiver found, or receiver passed checks and was processed).
    // Returns false if a receiver was found but failed the checks (e.g., no received pallets, or PalletZoneTracker has 0 pallets).
    // If forceSendEvenIfEmpty is true, the method will process receive-mode trucks even if they have 0 received pallets
    // and will still call MissionBehavior.receiveMission with an empty submittedBoxes list, then destroy the truck.
    // processedMissionId will be set to the mission id of the processed receiver (or -1 if none processed).
    private bool TryProcessReceiverInSpawnerZone(out int processedMissionId, bool forceSendEvenIfEmpty = false)
     {
        processedMissionId = -1;
        Collider zoneCollider = SpawnerZone.GetComponent<Collider>();
        if (zoneCollider == null)
        {
            Debug.LogWarning("SpawnerZone does not have a Collider component on OpenBayDoor.");
            return true; // allow close because we can't reliably check
        }

        // Use an overlap check to reliably find colliders inside the zone (more robust than testing the truck's pivot point)
        Vector3 pad = Vector3.one * 0.5f; // increased pad to be more tolerant of pivot offsets
        Vector3 extents = zoneCollider.bounds.extents + pad;
        Collider[] hits = Physics.OverlapBox(zoneCollider.bounds.center, extents, zoneCollider.transform.rotation, ~0, QueryTriggerInteraction.Collide);
        Debug.Log($"OpenBayDoor: OverlapBox found {hits.Length} colliders in SpawnerZone '{SpawnerZone.name}' (center={zoneCollider.bounds.center}, extents={zoneCollider.bounds.extents}).");
        var trucksFound = new System.Collections.Generic.HashSet<GameObject>();
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            Debug.Log($"OpenBayDoor: Overlap hit: {hit.gameObject.name} (layer={LayerMask.LayerToName(hit.gameObject.layer)})");
            Transform t = hit.transform;
            // walk up the hierarchy to find a parent tagged "Truck"
            while (t != null && !t.CompareTag("Truck")) t = t.parent;
            if (t != null && t.CompareTag("Truck"))
                trucksFound.Add(t.gameObject);
        }

        // Fallback: if overlap found nothing, check each tagged Truck's colliders to see if any intersect the zone.
        if (trucksFound.Count == 0)
        {
            GameObject[] allTrucksFallback = GameObject.FindGameObjectsWithTag("Truck");
            Debug.Log($"OpenBayDoor: Overlap found no trucks. Falling back to collider-bounds check over {allTrucksFallback.Length} tagged Truck objects.");
            foreach (var t in allTrucksFallback)
            {
                if (t == null || !t.activeInHierarchy) continue;
                Collider[] childColliders = t.GetComponentsInChildren<Collider>(true);
                bool added = false;
                foreach (var c in childColliders)
                {
                    if (c == null) continue;
                    if (c.bounds.Intersects(zoneCollider.bounds))
                    {
                        Debug.Log($"OpenBayDoor: Fallback detected truck {t.name} via child collider {c.gameObject.name} bounds intersection.");
                        trucksFound.Add(t);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    // Last resort: check renderers as well
                    Renderer r = t.GetComponentInChildren<Renderer>();
                    if (r != null && r.bounds.Intersects(zoneCollider.bounds))
                    {
                        Debug.Log($"OpenBayDoor: Fallback detected truck {t.name} via renderer.bounds intersection.");
                        trucksFound.Add(t);
                    }
                }
            }

            // Additional fallback: search for any TruckReceiver component in the scene (in case tag was missing)
            if (trucksFound.Count == 0)
            {
                var receivers = GameObject.FindObjectsOfType<TruckReceiver>();
                Debug.Log($"OpenBayDoor: No tagged trucks found; checking {receivers.Length} TruckReceiver components in scene.");
                foreach (var rec in receivers)
                {
                    if (rec == null || rec.gameObject == null || !rec.gameObject.activeInHierarchy) continue;
                    Collider[] childColliders = rec.GetComponentsInChildren<Collider>(true);
                    bool added = false;
                    foreach (var c in childColliders)
                    {
                        if (c == null) continue;
                        if (c.bounds.Intersects(zoneCollider.bounds))
                        {
                            Debug.Log($"OpenBayDoor: Fallback detected truck (via TruckReceiver) {rec.gameObject.name} collider {c.gameObject.name} bounds intersection.");
                            trucksFound.Add(rec.gameObject);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        Renderer r = rec.GetComponentInChildren<Renderer>();
                        if (r != null && r.bounds.Intersects(zoneCollider.bounds))
                        {
                            Debug.Log($"OpenBayDoor: Fallback detected truck (via TruckReceiver) {rec.gameObject.name} via renderer.bounds intersection.");
                            trucksFound.Add(rec.gameObject);
                        }
                    }
                }
            }
        }

        Debug.Log($"OpenBayDoor: Trucks found in zone: {trucksFound.Count}.");

        foreach (GameObject truck in trucksFound)
        {
            // Distinguish between send-mode trucks (PalletZoneTracker only) and receive-mode trucks (TruckReceiver)
            PalletZoneTracker pzt = truck.GetComponent<PalletZoneTracker>();
            TruckReceiver receiver = truck.GetComponent<TruckReceiver>();

            // If this is a send-mode truck (has PZT and no receiver), allow close only when availablePallets.Count == 0
            if (pzt != null && receiver == null)
            {
                // Prefer per-zone count so we only consider pallets relevant to this door's zone
                int spawned = pzt.GetPalletCountInZone(SpawnerZone.GetComponent<Collider>());
                int pending = pzt.availablePallets == null ? 0 : pzt.availablePallets.Count;
                Debug.Log($"OpenBayDoor: Processing send-mode truck '{truck.name}': spawnedInZone={spawned}, pendingAvailablePallets={pending}");
                // If tracker reports zero spawned but other systems may not have registered yet, do an overlap check for pallet objects in the zone
                if (spawned == 0 && SpawnerZone != null)
                {
                    Collider zoneCol = SpawnerZone.GetComponent<Collider>();
                    if (zoneCol != null)
                    {
                        Vector3 pad2 = Vector3.one * 0.1f;
                        Vector3 ext2 = zoneCol.bounds.extents + pad2;
                        Collider[] palletHits = Physics.OverlapBox(zoneCol.bounds.center, ext2, zoneCol.transform.rotation, ~0, QueryTriggerInteraction.Collide);
                        int overlapPallets = 0;
                        foreach (var h in palletHits)
                        {
                            if (h == null) continue;
                            if (h.GetComponent<Pallet>() != null) overlapPallets++;
                        }
                        if (overlapPallets > 0)
                        {
                            Debug.Log($"OpenBayDoor: Overlap detected {overlapPallets} pallet objects in SpawnerZone '{SpawnerZone.name}'. Treating as spawned.");
                            spawned = overlapPallets;
                        }
                    }
                }

                // Block closing if there are either spawned pallets present or pending pallets waiting to be spawned
                if (spawned > 0 || pending > 0)
                {
                    if (forceSendEvenIfEmpty)
                    {
                        Debug.Log($"OpenBayDoor: force deleting send-mode truck '{truck.name}' despite spawned({spawned}) or pending({pending}) pallets.");
                        Destroy(truck);
                        // Clear mission display tied to this spawner zone (if assigned)
                        SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
                        return true; // allow close after forced deletion
                    }
                    SFXController.Instance.PlayClip(SFXController.Instance.errorInput);
                    Debug.Log($"OpenBayDoor: cannot close, send-mode truck '{truck.name}' still has spawned({spawned}) or pending({pending}) pallets.");
                    return false; // block closing
                }

                // Nothing left to process on the truck -> despawn and allow close
                Debug.Log($"OpenBayDoor: send-mode truck '{truck.name}' is empty — processing and despawning.");
                Destroy(truck);
                // Clear mission display tied to this spawner zone (if assigned)
                SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
                return true;
            }

            // If this is a receive-mode truck (has receiver component), require receivedPallets.Count > 0
            if (receiver != null)
            {
                // Compute total boxes received across all received pallets (require at least 1 box)
                int totalBoxCount = 0;
                if (receiver.receivedPallets != null)
                {
                    foreach (var p in receiver.receivedPallets)
                    {
                        if (p == null || p.boxDataList == null) continue;
                        totalBoxCount += p.boxDataList.Count;
                    }
                }
                int recCount = receiver.receivedPallets == null ? 0 : receiver.receivedPallets.Count;
                Debug.Log($"OpenBayDoor: Processing receive-mode truck '{truck.name}': receivedPallets={recCount}, totalBoxes={totalBoxCount}, missionId={receiver.missionId}");
                if (totalBoxCount == 0 && !forceSendEvenIfEmpty)
                {
                    SFXController.Instance.PlayClip(SFXController.Instance.errorInput);
                    Debug.Log($"OpenBayDoor: cannot close, receive-mode truck '{truck.name}' has no received boxes.");
                    return false; // block closing
                }

                // Process received pallets: copy and submit mission
                lastReceivedPallets = new List<PalletData>(receiver.receivedPallets);
                foreach (var pal in lastReceivedPallets)
                {
                    if (pal != null && pal.boxDataList != null)
                    {
                        foreach (var b in pal.boxDataList)
                            lastReceivedBoxes.Add(b);
                    }
                }

                Debug.Log($"OpenBayDoor: receive-mode truck '{truck.name}' processed: {lastReceivedPallets.Count} pallets, {lastReceivedBoxes.Count} boxes.");

                // Always send submittedBoxes if forceSendEvenIfEmpty is true, otherwise only send when count > 0
                if (missionBehavior == null)
                    missionBehavior = FindObjectOfType<MissionBehavior>();

                if (missionBehavior != null && receiver.missionId >= 0 && (lastReceivedBoxes.Count > 0 || forceSendEvenIfEmpty))
                {
                    List<BoxData> submittedBoxes = new List<BoxData>(lastReceivedBoxes);
                    // Safely decrement the receiversSent counter via the TruckSpawnerManager component
                    if (truckSpawner != null)
                    {
                        var manager = truckSpawner.GetComponent<TruckSpawnerManager>();
                        if (manager != null)
                        {
                            manager.DecrementReceiversSent();
                        }
                        else
                        {
                            Debug.LogWarning("OpenBayDoor: truckSpawner GameObject does not have a TruckSpawnerManager component.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("OpenBayDoor: truckSpawner reference is null; cannot decrement receiversSent.");
                    }
                    if (GameStats.Instance.gameRound == 0)
                    {
                        missionBehavior.receiveMission(receiver.missionId, submittedBoxes, false);
                    }
                    else
                    {
                        missionBehavior.receiveMission(receiver.missionId, submittedBoxes);
                    }
                    Debug.Log($"OpenBayDoor: sent mission {receiver.missionId} with {submittedBoxes.Count} boxes to MissionBehavior.");
                }

                // Despawn the truck now that we've processed it
                processedMissionId = receiver.missionId;
                Destroy(truck);
                // Clear mission display tied to this spawner zone (if assigned)
                SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();

                return true;
            }

            // Neither PZT nor Receiver found on this truck — skip
            Debug.Log($"OpenBayDoor: Truck '{truck.name}' has neither PalletZoneTracker nor TruckReceiver; skipping.");
        }

        // No blocking condition encountered
        return true;
     }

    // Separate helper that checks PalletZoneTracker conditions for a truck.
    // Returns true if the truck has no PalletZoneTracker OR if the tracker exists and its availablePallets list is EMPTY (== 0).
    // In other words, this method answers: "Does the PalletZoneTracker condition allow closing?"
    private bool IsPalletZoneTrackerValidForClose(GameObject truck)
    {
        PalletZoneTracker pzt = truck.GetComponent<PalletZoneTracker>();
        if (pzt == null)
            return true; // no tracker — nothing to require, so allow close

        // If the tracker exists, allow close only when availablePallets is empty (count == 0)
        // treat null or zero as empty
        return pzt.GetTotalPalletsInZones() == 0;
    }

    // --- New: Coroutine that periodically monitors the spawner zone for a receiver truck and stores the mission id. ---
    private IEnumerator MonitorSpawnerZoneForReceiver()
    {
        while (true)
        {
            yield return new WaitForSeconds(receiverCheckInterval);

            if (SpawnerZone == null)
            {
                lastDetectedMissionId = -1;
                continue;
            }

            Collider zoneCollider = SpawnerZone.GetComponent<Collider>();
            if (zoneCollider == null)
            {
                lastDetectedMissionId = -1;
                continue;
            }

            // Find trucks overlapping the zone
            Vector3 pad = Vector3.one * 0.1f;
            Vector3 extents = zoneCollider.bounds.extents + pad;
            Collider[] hits = Physics.OverlapBox(zoneCollider.bounds.center, extents, zoneCollider.transform.rotation, ~0, QueryTriggerInteraction.Collide);
            var trucksFound = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                Transform t = hit.transform;
                while (t != null && !t.CompareTag("Truck")) t = t.parent;
                if (t != null && t.CompareTag("Truck")) trucksFound.Add(t.gameObject);
            }

            // If none found by tag/colliders, try detecting by TruckReceiver component bounds
            if (trucksFound.Count == 0)
            {
                var receivers = GameObject.FindObjectsOfType<TruckReceiver>();
                foreach (var rec in receivers)
                {
                    if (rec == null || rec.gameObject == null || !rec.gameObject.activeInHierarchy) continue;
                    Collider[] childColliders = rec.GetComponentsInChildren<Collider>(true);
                    bool added = false;
                    foreach (var c in childColliders)
                    {
                        if (c == null) continue;
                        if (c.bounds.Intersects(zoneCollider.bounds))
                        {
                            trucksFound.Add(rec.gameObject);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        Renderer r = rec.GetComponentInChildren<Renderer>();
                        if (r != null && r.bounds.Intersects(zoneCollider.bounds))
                        {
                            trucksFound.Add(rec.gameObject);
                        }
                    }
                }
            }

            // Find first receive-mode truck and store its mission id
            int detectedId = -1;
            foreach (var truck in trucksFound)
            {
                if (truck == null) continue;
                TruckReceiver receiver = truck.GetComponent<TruckReceiver>();
                if (receiver != null)
                {
                    detectedId = receiver.missionId;
                    break;
                }

                PalletZoneTracker sender = truck.GetComponent<PalletZoneTracker>();
                if (sender != null && sender.GetTotalPalletsInZones() == 0)
                {
                    SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.SetCloseDoorDisplay();
                }
            }

            lastDetectedMissionId = detectedId;
            // small debug
            // Debug.Log($"OpenBayDoor: Monitor found missionId={lastDetectedMissionId} in zone '{(SpawnerZone!=null?SpawnerZone.name:"null")}'.");
        }
    }

    // Public method: close the door and return the mission id that was detected by the monitor (or -1 if none)
    public void CloseDoorAndGetMissionId()
    {
        // Force processing: send submittedBoxes even if empty, destroy truck, and return the processed mission id
        int processedId = -1;
        lastReceivedPallets.Clear();
        lastReceivedBoxes.Clear();
        TryProcessReceiverInSpawnerZone(out processedId, true);

        // Now close the door animation (do not call CloseDoor() because it would re-process)
        if (doorAnimator == null)
        {
            Debug.LogWarning("Animator not set on OpenBayDoor.");
        }
        else
        {
                if (isOpen)
                {
                    doorAnimator.Play(closeState);
                    isOpen = false;
                }
      
        }

    }

    // Toggles between open and close
    public void ToggleDoor()
    {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    public void Interact(GameObject interactor)
    {
        ToggleDoor();
        SFXController.Instance.PlayClip(SFXController.Instance.doorClicked);
    }

    void Start()
    {
        // start the background monitor coroutine
        if (receiverMonitorCoroutine == null)
            receiverMonitorCoroutine = StartCoroutine(MonitorSpawnerZoneForReceiver());
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        if (receiverMonitorCoroutine != null)
            StopCoroutine(receiverMonitorCoroutine);
    }

    // Draw the overlap box in the editor when this object is selected to help debug detection
    private void OnDrawGizmosSelected()
    {
        if (SpawnerZone == null) return;
        Collider zoneCollider = SpawnerZone.GetComponent<Collider>();
        if (zoneCollider == null) return;
        Vector3 pad = Vector3.one * 0.1f;
        Vector3 extents = zoneCollider.bounds.extents + pad;
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(zoneCollider.bounds.center, zoneCollider.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, extents * 2f);
    }
}

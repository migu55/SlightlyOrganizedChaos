using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OpenBayDoor1 : MonoBehaviour, Interactable
{
    [Header("Door Animation Settings")]
    [Tooltip("Animator component controlling the door.")]
    [SerializeField] private Animator doorAnimator;

    // Name of the animation states
    private const string openState = "Door1Open";
    private const string closeState = "Door1Close";

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
            Debug.LogWarning("Animator not set on OpenBayDoor1.");
            return;
        }

        if (!isOpen)
        {
            doorAnimator.Play(openState);
            isOpen = true;
        }
    }

    // Call this method to close the door
    public void CloseDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("Animator not set on OpenBayDoor1.");
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
            doorAnimator.Play(closeState);
            isOpen = false;
        }
    }

    // New helper: looks for a receiver truck inside the SpawnerZone, enforces checks, copies data, and submits to MissionBehavior.
    // Returns true if it's OK to close the door (either no receiver found, or receiver passed checks and was processed).
    // Returns false if a receiver was found but failed the checks (e.g., no received pallets, or PalletZoneTracker has pallets).
    // If forceSendEvenIfEmpty is true, the method will process receive-mode trucks even if they have 0 received pallets
    // and will still call MissionBehavior.receiveMission with an empty submittedBoxes list, then destroy the truck.
    // processedMissionId will be set to the mission id of the processed receiver (or -1 if none processed).
    private bool TryProcessReceiverInSpawnerZone(out int processedMissionId, bool forceSendEvenIfEmpty = false)
    {
        processedMissionId = -1;
         Collider zoneCollider = SpawnerZone.GetComponent<Collider>();
         if (zoneCollider == null)
         {
             Debug.LogWarning("SpawnerZone does not have a Collider component on OpenBayDoor1.");
             return true; // allow close because we can't reliably check
         }

        // Use an overlap check to reliably find colliders inside the zone (more robust than testing the truck's pivot point)
        Collider[] hits = Physics.OverlapBox(zoneCollider.bounds.center, zoneCollider.bounds.extents, zoneCollider.transform.rotation);
        var trucksFound = new System.Collections.Generic.HashSet<GameObject>();
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            Transform t = hit.transform;
            // walk up the hierarchy to find a parent tagged "Truck"
            while (t != null && !t.CompareTag("Truck")) t = t.parent;
            if (t != null && t.CompareTag("Truck"))
                trucksFound.Add(t.gameObject);
        }

        foreach (GameObject truck in trucksFound)
        {
            // Distinguish modes: send-mode trucks (PalletZoneTracker only) vs receive-mode (TruckReceiver)
            PalletZoneTracker pzt = truck.GetComponent<PalletZoneTracker>();
            TruckReceiver receiver = truck.GetComponent<TruckReceiver>();

            // Send-mode truck: block if spawned or pending pallets exist
            if (pzt != null && receiver == null)
            {
                int spawned = pzt.GetTotalPalletsInZones();
                int pending = pzt.availablePallets == null ? 0 : pzt.availablePallets.Count;
                Debug.Log($"OpenBayDoor1: Processing send-mode truck '{truck.name}': spawnedInZones={spawned}, pendingAvailablePallets={pending}");
                if (spawned > 0 || pending > 0)
                {
                    if (forceSendEvenIfEmpty)
                    {
                        Debug.Log($"OpenBayDoor1: force deleting send-mode truck '{truck.name}' despite spawned({spawned}) or pending({pending}) pallets.");
                        Destroy(truck);
                        // Clear mission display tied to this spawner zone (if assigned)
                        SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
                        return true; // allow close after forced deletion
                    }

                    Debug.Log($"OpenBayDoor1: cannot close, send-mode truck '{truck.name}' still has spawned({spawned}) or pending({pending}) pallets.");
                    return false;
                }

                Debug.Log($"OpenBayDoor1: send-mode truck '{truck.name}' is empty — processing and despawning.");
                Destroy(truck);
                // Clear mission display tied to this spawner zone (if assigned)
                SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
                return true;
            }

            // Receive-mode truck: require at least one received pallet
            if (receiver != null)
             {
                int recCount = receiver.receivedPallets == null ? 0 : receiver.receivedPallets.Count;
                Debug.Log($"OpenBayDoor1: Processing receive-mode truck '{truck.name}': receivedPallets={recCount}, missionId={receiver.missionId}");
                if (recCount == 0 && !forceSendEvenIfEmpty)
                {
                    Debug.Log($"OpenBayDoor1: cannot close, receive-mode truck '{truck.name}' has no received pallets.");
                    return false;
                }

                // Copy and send
                lastReceivedPallets = new List<PalletData>(receiver.receivedPallets);
                foreach (var pal in lastReceivedPallets)
                 {
                     if (pal != null && pal.boxDataList != null)
                     {
                         foreach (var b in pal.boxDataList)
                             lastReceivedBoxes.Add(b);
                     }
                 }

                Debug.Log($"OpenBayDoor1: receive-mode truck '{truck.name}' processed: {lastReceivedPallets.Count} pallets, {lastReceivedBoxes.Count} boxes.");

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
                            Debug.LogWarning("OpenBayDoor1: truckSpawner GameObject does not have a TruckSpawnerManager component.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("OpenBayDoor1: truckSpawner reference is null; cannot decrement receiversSent.");
                    }
                    if (GameStats.Instance.gameRound == 0)
                    {
                        missionBehavior.receiveMission(receiver.missionId, submittedBoxes, false);
                    }
                    else
                    {
                        missionBehavior.receiveMission(receiver.missionId, submittedBoxes);
                    }
                    Debug.Log($"OpenBayDoor1: sent mission {receiver.missionId} with {submittedBoxes.Count} boxes to MissionBehavior.");
                }

                processedMissionId = receiver.missionId;
                Destroy(truck);
                // Clear mission display tied to this spawner zone (if assigned)
                SpawnerZone?.GetComponent<TruckToDisplay>()?.missionDisplay?.GetComponent<MissionDisplayController>()?.ClearMissionTruck();
                return true;
            }

            Debug.Log($"OpenBayDoor1: Truck '{truck.name}' has neither PalletZoneTracker nor TruckReceiver; skipping.");
        }

        // No blocking condition encountered
        return true;
    }

    // Public method: close the door and return the mission id that was detected by the monitor (now just forces door close)
    public void CloseDoorAndGetMissionId()
    {
        int processedId = -1;
        lastReceivedPallets.Clear();
        lastReceivedBoxes.Clear();
        TryProcessReceiverInSpawnerZone(out processedId, true);

        // Close animation (don't call CloseDoor() to avoid double-processing)
        if (doorAnimator == null)
        {
            Debug.LogWarning("Animator not set on OpenBayDoor1.");
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
    }

    void Start()
    {
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
			}

			lastDetectedMissionId = detectedId;
			// small debug
			// Debug.Log($"OpenBayDoor1: Monitor found missionId={lastDetectedMissionId} in zone '{(SpawnerZone!=null?SpawnerZone.name:"null")}'.");
		}
	}
}

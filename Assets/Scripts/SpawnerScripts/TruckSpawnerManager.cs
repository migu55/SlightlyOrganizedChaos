using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckSpawnerManager : MonoBehaviour
{
    // List of box data (pure data, not GameObjects)
    public List<BoxData> boxes = new List<BoxData>();

    // List of spawn zones (expected size: 4). The manager will choose the first free zone when spawning a truck.
    public List<Collider> spawnZones = new List<Collider>();

    public GameObject truckPrefab;

    // Boolean to track what mode we are in
    public bool isSendMode;

    private int receiversSent = 0;
    private int receiverLimit = 3;

    // Expose receiversSent as a read-only property and provide safe increment/decrement methods
    public int ReceiversSent => receiversSent;

    public void IncrementReceiversSent()
    {
        receiversSent++;
    }

    public void DecrementReceiversSent()
    {
        receiversSent--;
    }

    // Queue for backed up trucks (stores box list, sendMode, and missionId)
    private Queue<(List<BoxData> boxList, bool sendMode, int missionId)> backedUpTrucks =
        new Queue<(List<BoxData>, bool, int)>();

    private void Update()
    {
        // intentionally left empty - queue processing moved to coroutine ProcessQueueCoroutine
    }

    // Coroutine-based queue processor to run when work exists
    private Coroutine queueProcessorCoroutine;
    private IEnumerator ProcessQueueCoroutine()
    {
        // Top-level yield: wait until there is work to do. This avoids waking every 0.5s when the queue is empty.
        var shortWait = new WaitForSeconds(3f);
        var longWait = new WaitForSeconds(3f);

        while (true)
        {
            // Wait until there is at least one backed-up truck to process
            yield return new WaitUntil(() => backedUpTrucks.Count > 0);

            // While there are queued trucks, attempt to process them. Yield between iterations so we don't hog the frame.
            while (backedUpTrucks.Count > 0)
            {
                // Determine occupancy for each configured spawn zone
                bool anyZoneFree = false;
                GameObject[] allTrucks = GameObject.FindGameObjectsWithTag("Truck");
                for (int zi = 0; zi < spawnZones.Count; zi++)
                {
                    Collider z = spawnZones[zi];
                    if (z == null) continue;

                    bool occupied = false;
                    foreach (GameObject truck in allTrucks)
                    {
                        if (truck == null) continue;
                        if (z.bounds.Contains(truck.transform.position))
                        {
                            occupied = true;
                            break;
                        }
                    }

                    if (!occupied)
                    {
                        anyZoneFree = true;
                        break;
                    }
                }

                if (anyZoneFree)
                {
                    // Peek instead of Dequeue so we can decide whether to spawn without changing queue order
                    var next = backedUpTrucks.Peek();
                    var boxList = next.boxList;
                    var sendMode = next.sendMode;
                    var missionId = next.missionId;

                    // Use >= to block when we've already reached or exceeded the limit
                    if (ReceiversSent >= receiverLimit && !sendMode)
                    {
                        // receiver slots are full; do not dequeue — leave it in the queue
                        // wait a short time before trying again to let receivers free up
                        yield return shortWait;
                    }
                    else
                    {
                        // safe to dequeue and spawn
                        backedUpTrucks.Dequeue();
                        spawnTruck(missionId, boxList, sendMode);
                        // allow a short pause so multiple spawns don't all happen in one frame
                        yield return shortWait;
                    }
                }
                else
                {
                    // No free zone right now; wait longer before retrying
                    yield return longWait;
                }
            }

            // When queue is empty we'll go back to waiting until new items are enqueued
        }
    }

    void Start()
    {
        // start the background queue processor coroutine
        if (queueProcessorCoroutine == null)
            queueProcessorCoroutine = StartCoroutine(ProcessQueueCoroutine());
    }

    private void OnDestroy()
    {
        if (queueProcessorCoroutine != null)
            StopCoroutine(queueProcessorCoroutine);
    }

    // ----------------------------------------------------------------------
    // Converts a list of BoxData into a list of PalletData (5 boxes per pallet)
    // Each box in boxDataList can be a different type; typeOfBox is not set.
    // ----------------------------------------------------------------------
    private List<PalletData> ConvertBoxesToPallets(List<BoxData> boxList)
    {
        List<PalletData> pallets = new List<PalletData>();
        for (int i = 0; i < boxList.Count; i += 5)
        {
            PalletData palletData = new PalletData();
            palletData.boxDataList = boxList.GetRange(i, Mathf.Min(5, boxList.Count - i));
            palletData.amtOfPallet = 1;
            pallets.Add(palletData);
        }
        return pallets;
    }

    // ----------------------------------------------------------------------
    // Public spawn entry points
    // ----------------------------------------------------------------------

    // Overload that preserves previous behavior (no missionId)
    public void spawnTruck(List<BoxData> box, bool sendMode)
    {
        spawnTruck(-1, box, sendMode);
    }

    // New overload: accepts a missionId and ensures the spawned truck's TruckReceiver holds it
    public void spawnTruck(int missionId, List<BoxData> box, bool sendMode)
    {
        isSendMode = sendMode;
        List<PalletData> pallets = ConvertBoxesToPallets(box);

        // Choose the first spawn zone that is not currently occupied by a truck
        Collider targetZone = null;
        GameObject[] allTrucks = GameObject.FindGameObjectsWithTag("Truck");
        for (int zi = 0; zi < spawnZones.Count; zi++)
        {
            Collider z = spawnZones[zi];
            if (z == null) continue;

            bool occupied = false;
            foreach (GameObject truck in allTrucks)
            {
                if (truck == null) continue;
                if (z.bounds.Contains(truck.transform.position))
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                targetZone = z;
                break;
            }
        }

        if (targetZone != null)
        {
            // Enforce receiver limit here as well so direct spawns (when target zones are free)
            // don't bypass the queued/spawn-limit logic and create more receive trucks than allowed.
            if (!sendMode && ReceiversSent >= receiverLimit)
            {
                backedUpTrucks.Enqueue((new List<BoxData>(box), sendMode, missionId));
            }
            else
            {
                InstantiateTruckInZone(targetZone, pallets, sendMode, missionId);
            }
        }
        else
        {
            // All configured spawn zones are occupied, enqueue this truck's data (preserve missionId)
            backedUpTrucks.Enqueue((new List<BoxData>(box), sendMode, missionId));
        }
    }

    // New: spawn a truck using only a missionId and sendMode (no box list required).
    public void spawnTruck(int missionId, bool sendMode)
    {
        spawnTruck(missionId, new List<BoxData>(), sendMode);
    }

    // New: spawn a truck without specifying a missionId or a box list.
    public void spawnTruck(bool sendMode)
    {
        spawnTruck(-1, new List<BoxData>(), sendMode);
    }

    // Diagnostic: log incoming spawn requests at the List<BoxData> entry point
    public void spawnTruck_Diagnostic(int missionId, List<BoxData> box, bool sendMode)
    {
        spawnTruck(missionId, box, sendMode);
    }

    // ----------------------------------------------------------------------
    // Instantiates a truck in the specified zone and sets up its components
    // (moved to the end of the file per request)
    // ----------------------------------------------------------------------
    private void InstantiateTruckInZone(Collider targetZone, List<PalletData> pallets, bool sendMode, int missionId = -1)
    {

        if (truckPrefab == null)
        {
            return;
        }

        // Calculate spawn position inside the zone
        Bounds bounds = targetZone.bounds;
        Vector3 size = Vector3.one;
        Renderer rend = truckPrefab.GetComponentInChildren<Renderer>();
        if (rend != null) size = rend.bounds.size;
        float marginX = size.x / 2f;
        float marginY = size.y / 2f;
        float marginZ = size.z / 2f;

        // Spawn at the center of the zone, offset by (0, -4, 0)
        Vector3 spawnPos = bounds.center + new Vector3(0, -4f, 0);
        // Determine zone index to allow special-case orientation for zones 3 and 4 (indices 2 and 3)
        int zoneIndex = -1;
        if (spawnZones != null && targetZone != null) zoneIndex = spawnZones.IndexOf(targetZone);
        float yaw = -90f; // default yaw used previously
        // If spawning in zone 3 or 4, rotate additional 180 degrees so spawned trucks face the opposite direction
        if (zoneIndex >= 2)
        {
            yaw += 180f;
        }
        Quaternion spawnRot = Quaternion.Euler(0, yaw, 0);
        GameObject truckObj = Instantiate(truckPrefab, spawnPos, spawnRot);
        if (truckObj == null)
        {
            return;
        }

        truckObj.tag = "Truck";
        truckObj.transform.localScale = truckObj.transform.localScale + Vector3.one * 2f;

        if (sendMode)
        {
            // Make this truck a "send" truck: remove any TruckReceiver component and use PalletZoneTracker
            TruckReceiver truckReceiver = truckObj.GetComponent<TruckReceiver>();
            if (truckReceiver != null) Destroy(truckReceiver);

            PalletZoneTracker zoneTracker = truckObj.GetComponent<PalletZoneTracker>();
            if (zoneTracker != null)
            {
                zoneTracker.enabled = true;
                zoneTracker.AddPallets(pallets);
                zoneTracker.RefreshZones();
                targetZone.gameObject.GetComponent<TruckToDisplay>()?.missionDisplay
                    .GetComponent<MissionDisplayController>()?.SetRecieveTruck(truckObj);
            }
        }
        else
        {
            // Make this truck a "receive" truck: remove any PalletZoneTracker component and use TruckReceiver
            PalletZoneTracker zoneTracker = truckObj.GetComponent<PalletZoneTracker>();
            if (zoneTracker != null) Destroy(zoneTracker);

            TruckReceiver truckReceiver = truckObj.GetComponent<TruckReceiver>();
            if (truckReceiver != null)
            {
                // Use the public increment method so the counter behavior is centralized and logged
                IncrementReceiversSent();
                truckReceiver.missionId = missionId;
                truckReceiver.enabled = true;
                truckReceiver.SetExpectedPallets(pallets);
                targetZone.gameObject.GetComponent<TruckToDisplay>()?.missionDisplay
                    .GetComponent<MissionDisplayController>()?.SetMissionTruck(truckObj);
            }
        }

        var gotReceiver = truckObj.GetComponent<TruckReceiver>() != null;
        var gotPZT = truckObj.GetComponent<PalletZoneTracker>() != null;
    }

	public void clearQueue(){
		int cleared = backedUpTrucks.Count;
		backedUpTrucks.Clear();
	}

	// Removes any queued truck entries that match the provided missionId, then calls
	// MissionBehavior.receiveMission(missionId, submittedBoxes) with an empty list.
	// This is useful for cancelling a queued receive truck and still informing the
	// mission system that the mission was "received" (with zero submitted boxes).
	public void RemoveQueuedMissionAndSubmit(int missionId)
	{
		int removed = 0;
		// Build a new queue containing only items that do NOT match the missionId
		var newQueue = new Queue<(List<BoxData> boxList, bool sendMode, int missionId)>();
		while (backedUpTrucks.Count > 0)
		{
			var item = backedUpTrucks.Dequeue();
			if (item.missionId == missionId)
			{
				removed++;
				// skip re-enqueueing -> effectively removing it from the queue
			}
			else
			{
				newQueue.Enqueue(item);
			}
		}

		// Replace the queue with the filtered one
		backedUpTrucks = newQueue;

		// Find the MissionBehavior in the scene and report the mission with an empty box list
		var missionBehavior = FindObjectOfType<MissionBehavior>();
		if (missionBehavior == null)
		{
			return;
		}

		missionBehavior.receiveMission(missionId, new List<BoxData>());
	}

}

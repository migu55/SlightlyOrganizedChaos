// Simplified PalletZoneTracker
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PalletZoneTracker : MonoBehaviour
{
    public GameObject palletPrefab;
    // PalletData queue (first-in, first-out)
    public List<PalletData> availablePallets = new List<PalletData>();

    public Collider zoneA;
    public Collider zoneB;

    private List<GameObject> palletsInZoneA = new List<GameObject>();
    private List<GameObject> palletsInZoneB = new List<GameObject>();

    // cooldown to avoid duplicate spawns in short succession
    private const float spawnCooldown = 0.25f;
    private float lastSpawnA = -10f;
    private float lastSpawnB = -10f;

    private void Start()
    {
        if (zoneA != null)
        {
            ZoneAbsorber a = zoneA.GetComponent<ZoneAbsorber>(); if (a != null) a.enabled = false;
        }
        if (zoneB != null)
        {
            ZoneAbsorber b = zoneB.GetComponent<ZoneAbsorber>(); if (b != null) b.enabled = false;
        }

        // start the background coroutine to refresh zones and spawn periodically
        StartCoroutine(ZoneProcessingLoop());
    }

    // Replace per-frame Update() with a coroutine that runs every ~0.5s
    private IEnumerator ZoneProcessingLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (enabled)
        {
            // Keep zone lists up-to-date using overlap fallback, then attempt at most one spawn per zone per iteration (subject to cooldown)
            RefreshZoneFromOverlap(zoneA, palletsInZoneA);
            RefreshZoneFromOverlap(zoneB, palletsInZoneB);

            TrySpawnPalletInZone(zoneA, palletsInZoneA);
            TrySpawnPalletInZone(zoneB, palletsInZoneB);

            yield return wait;
        }
    }

    // Public: add PalletData items to the queue
    public void AddPallets(List<PalletData> palletsToAdd)
    {
        if (palletsToAdd == null || palletsToAdd.Count == 0) return;
        if (availablePallets == null) availablePallets = new List<PalletData>();
        availablePallets.AddRange(palletsToAdd);
    }

    // ------------------------------------------------------------------
    // Spawn logic
    // ------------------------------------------------------------------
    private void TrySpawnPalletInZone(Collider zone, List<GameObject> zoneList)
    {
        if (zone == null || zoneList == null) return;

        // clean stale entries
        CleanZoneList(zoneList, zone);

        if (zoneList.Count > 0) return;            // zone not empty
        if (availablePallets == null || availablePallets.Count == 0) return;

        // per-zone cooldown
        float now = Time.time;
        if (zone == zoneA && now - lastSpawnA < spawnCooldown) return;
        if (zone == zoneB && now - lastSpawnB < spawnCooldown) return;

        // consume next PalletData (FIFO)
        PalletData palletData = availablePallets[0];
        availablePallets.RemoveAt(0);

        Vector3 spawnPos = zone.bounds.center;
        GameObject palletObj = SpawnPalletObject(palletData, spawnPos);
        if (palletObj != null)
        {
            zoneList.Add(palletObj); // immediately mark zone as occupied
            if (zone == zoneA) lastSpawnA = now; else if (zone == zoneB) lastSpawnB = now;
        }
        else
        {
            // on rare failure, push data back so it's not lost
            availablePallets.Insert(0, palletData);
        }
    }

    private GameObject SpawnPalletObject(PalletData palletData, Vector3 spawnPos)
    {
        if (palletPrefab == null)
        {
            Debug.LogError("PalletZoneTracker: palletPrefab not assigned");
            return null;
        }

        GameObject obj = Instantiate(palletPrefab, spawnPos, Quaternion.Euler(0, 90, 0));
        try { obj.tag = "Pallet"; } catch { }
        obj.layer = LayerMask.NameToLayer("Pallet");

        Pallet p = obj.GetComponent<Pallet>();
        if (p == null) p = obj.AddComponent<Pallet>();

        // set data without spawning visual boxes
        if (palletData != null) p.SetPalletData(palletData);

        return obj;
    }

    // ------------------------------------------------------------------
    // Overlap fallback (simple): add any pallets inside the bounds not already tracked
    // ------------------------------------------------------------------
    private void RefreshZoneFromOverlap(Collider zone, List<GameObject> zoneList)
    {
        if (zone == null || zoneList == null) return;

        CleanZoneList(zoneList, zone);

        Vector3 center = zone.bounds.center;
        Vector3 half = zone.bounds.extents;
        Collider[] hits = Physics.OverlapBox(center, half, Quaternion.identity);
        foreach (var h in hits)
        {
            Pallet p = h.GetComponentInParent<Pallet>();
            if (p == null) continue;
            GameObject go = p.gameObject;
            if (!zoneList.Contains(go))
                zoneList.Add(go);
        }
    }

    // ------------------------------------------------------------------
    // Cleanup helpers
    // ------------------------------------------------------------------
    private void CleanZoneList(List<GameObject> zoneList, Collider zone = null)
    {
        if (zoneList == null) return;
        if (zone == null)
        {
            zoneList.RemoveAll(i => i == null);
            return;
        }
        zoneList.RemoveAll(i => i == null || i.transform == null || !zone.bounds.Contains(i.transform.position));
    }

    public int GetPalletCountInZone(Collider zone)
    {
        if (zone == zoneA) return palletsInZoneA.Count;
        if (zone == zoneB) return palletsInZoneB.Count;
        return GetTotalPalletsInZones();
    }

    public int GetTotalPalletsInZones()
    {
        int a = palletsInZoneA == null ? 0 : palletsInZoneA.Count;
        int b = palletsInZoneB == null ? 0 : palletsInZoneB.Count;
        return a + b;
    }

    public void RefreshZones()
    {
        // immediate refresh: update overlap lists and allow Update to spawn next frame
        RefreshZoneFromOverlap(zoneA, palletsInZoneA);
        RefreshZoneFromOverlap(zoneB, palletsInZoneB);
    }
}

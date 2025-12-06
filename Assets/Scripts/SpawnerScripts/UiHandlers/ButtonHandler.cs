using UnityEngine;
using System.Collections.Generic;

public class ButtonHandler : MonoBehaviour
{
    // Reference to the SpawnBox script (assign in Inspector or find in scene)
    public SpawnBox spawnBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method to be called on button click
    public void OnSpawnBoxClicked()
    {
        // Find a Pallet in the scene
        Pallet pallet = FindObjectOfType<Pallet>();
        if (pallet == null)
        {
            Debug.LogError("No Pallet found in the scene.");
            return;
        }
        // If spawnBox is not assigned, try to find it
        if (spawnBox == null)
        {
            spawnBox = FindObjectOfType<SpawnBox>();
            if (spawnBox == null)
            {
                Debug.LogError("No SpawnBox script found in the scene.");
                return;
            }
        }
        // Call SpawnBoxesFromPallet
        spawnBox.SpawnBoxesFromPallet(pallet);
    }

    // Method to add test pallets to the PalletZoneTracker
    public void AddTestPalletsToZoneTracker()
    {
        // Find the PalletZoneTracker in the scene
        PalletZoneTracker zoneTracker = FindObjectOfType<PalletZoneTracker>();
        if (zoneTracker == null)
        {
            Debug.LogError("No PalletZoneTracker found in the scene.");
            return;
        }
        // Create test pallets (as data, not scene objects)
        List<PalletData> testPallets = new List<PalletData>();
        for (int i = 0; i < 3; i++)
        {
            PalletData testPallet = new PalletData();
            testPallet.typeOfBox = $"TestType_{i+1}";
            testPallet.amtOfPallet = Random.Range(1, 4);
            testPallet.boxDataList = new List<BoxData>();
            for (int j = 0; j < Random.Range(1, 4); j++)
            {
                BoxData testBox = new BoxData();
                testBox.typeOfBox = $"BoxType_{j+1}";
                testPallet.boxDataList.Add(testBox);
            }
            testPallets.Add(testPallet);
        }
        // Add to the zone tracker
        zoneTracker.AddPallets(testPallets);
        Debug.Log($"Added {testPallets.Count} test pallets to PalletZoneTracker.");
    }
}

using UnityEngine;
using System.Collections.Generic;

public class OrdersTest : MonoBehaviour, Interactable
{
    // Reference to the TruckSpawnerManager instance
    private TruckSpawnerManager truckSpawnerManager;

    // ----------------------------------------------------------------------
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // ----------------------------------------------------------------------
    void Start()
    {
    }

    // ----------------------------------------------------------------------
    // Update is called once per frame
    // ----------------------------------------------------------------------
    void Update()
    {

    }

    // Extracted logic from Start for reusability
    public void RunOrdersTest()
    {
        // Find an existing TruckSpawnerManager in the scene
        truckSpawnerManager = FindObjectOfType<TruckSpawnerManager>();

        // If not found, log an error and return
        if (truckSpawnerManager == null)
        {
            Debug.LogError("OrdersTest: No TruckSpawnerManager found in the scene.");
            return;
        }

        // Generate a random test list of BoxData (length 10-20)
        int boxCount = Random.Range(10, 21); // 21 is exclusive
        List<BoxData> testBoxes = new List<BoxData>();
        string[] boxTypes = { "A", "B", "C" };
        
        for (int i = 0; i < boxCount; i++)
        {
            int boxType = Random.Range(0, boxTypes.Length);
            BoxData boxData = new BoxData();
            // Assign a random type string for testing
            boxData.typeOfBox = boxTypes[boxType];
            testBoxes.Add(boxData);
        }

        // Call spawnTruck with sendMode=true to test send mode
        truckSpawnerManager.spawnTruck(testBoxes, true);
    }

    public void Interact(GameObject interactor)
    {
        RunOrdersTest();
    }
}

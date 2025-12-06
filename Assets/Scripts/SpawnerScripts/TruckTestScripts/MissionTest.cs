using UnityEngine;
using System.Collections.Generic;

public class MissionTest : MonoBehaviour, Interactable
{
    // Reference to the TruckSpawnerManager instance
    private TruckSpawnerManager truckSpawnerManager;
    private MissionBehavior missionBehavior;

    // ----------------------------------------------------------------------
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // ----------------------------------------------------------------------
    void Start()
    {
        // Intentionally left blank. Call RunMissionTest() manually.
    }

    // ----------------------------------------------------------------------
    // Update is called once per frame
    // ----------------------------------------------------------------------
    void Update()
    {
        
    }

    // Public method to run the mission test logic
    public void RunMissionTest()
    {
        // Find an existing TruckSpawnerManager in the scene
        //truckSpawnerManager = FindObjectOfType<TruckSpawnerManager>();
        missionBehavior = FindObjectOfType<MissionBehavior>();

        // If not found, log an error and return
        //if (truckSpawnerManager == null)
        //{
        //    Debug.LogError("MissionTest: No TruckSpawnerManager found in the scene.");
        //    return;

        //}
        
        if (missionBehavior == null)
        {
            Debug.LogError("MissionTest: No MissionManager found in the scene.");
            return;
        }

        missionBehavior.createMission();

        //// Generate a random test list of BoxData (length 10-20)
        //int boxCount = Random.Range(10, 21); // 21 is exclusive
        //List<BoxData> testBoxes = new List<BoxData>();
        //for (int i = 0; i < boxCount; i++)
        //{
        //    BoxData boxData = new BoxData();
        //    // Assign a random type string for testing
        //    boxData.typeOfBox = "Type_" + Random.Range(1, 6); // Types 1-5
        //    testBoxes.Add(boxData);
        //}

        //// Call spawnTruck with sendMode=false to test receive mode
        //truckSpawnerManager.spawnTruck(testBoxes, false);
    }

    // Interactable implementation
    public void Interact(GameObject interactor)
    {
        RunMissionTest();
    }
}

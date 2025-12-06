using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class DoorFail : MonoBehaviour
{
    [Header("Door Handles")]
    [Tooltip("Assign door components (OpenBayDoor / OpenBayDoor1) here. If empty, the script will auto-populate at Start.")]
    [SerializeField] private List<MonoBehaviour> doorHandles = new List<MonoBehaviour>();

    // Optional reference to the TruckSpawnerManager to allow removing queued missions.
    [Header("Spawner")]
    [Tooltip("Optional reference to TruckSpawnerManager. If left empty the manager will be located at runtime.")]
    [SerializeField] private TruckSpawnerManager truckSpawnerManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (doorHandles == null) doorHandles = new List<MonoBehaviour>();
        if (doorHandles.Count == 0)
            PopulateHandles();
    }

    // Finds candidate door components in the scene and stores them in doorHandles.
    // It looks for components that implement a public method named CloseDoorAndGetMissionId
    // and expose either a public property "LastDetectedMissionId" or a field named "lastDetectedMissionId".
    public void PopulateHandles()
    {
        doorHandles.Clear();
        // Find all MonoBehaviour components in the scene and pick matching door scripts
        MonoBehaviour[] all = FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (mb == null) continue;
            var t = mb.GetType();
            MethodInfo closeMethod = t.GetMethod("CloseDoorAndGetMissionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            PropertyInfo prop = t.GetProperty("LastDetectedMissionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            FieldInfo field = t.GetField("lastDetectedMissionId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            // Accept if it has the close method and either the property or field that stores mission id
            if (closeMethod != null && (prop != null || field != null))
            {
                doorHandles.Add(mb);
                continue;
            }

            // As a fallback, accept by name (e.g., OpenBayDoor, OpenBayDoor1)
            if (t.Name.Contains("OpenBayDoor"))
            {
                doorHandles.Add(mb);
            }
        }
    }

    // Public accessor for the stored handles
    public IReadOnlyList<MonoBehaviour> DoorHandles => doorHandles;

    // Finds the first door whose detected mission id equals missionId and calls its CloseDoorAndGetMissionId method.
    // Returns the value returned by the door's CloseDoorAndGetMissionId() if it returns an int, or missionId if the door method is void.
    // Returns -1 if no matching door was found or if an error occurred while invoking.
    public int CloseDoorForMission(int missionId)
    {
        if (doorHandles == null || doorHandles.Count == 0)
        {
            PopulateHandles();
            if (doorHandles.Count == 0) return -1;
        }

        foreach (var handle in doorHandles)
        {
            if (handle == null) continue;
            var t = handle.GetType();

            // Try public property first
            int detected = -1;
            PropertyInfo prop = t.GetProperty("LastDetectedMissionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null && prop.PropertyType == typeof(int))
            {
                object val = prop.GetValue(handle, null);
                if (val is int) detected = (int)val;
            }
            else
            {
                // Try private field fallback
                FieldInfo field = t.GetField("lastDetectedMissionId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (field != null && field.FieldType == typeof(int))
                {
                    object val = field.GetValue(handle);
                    if (val is int) detected = (int)val;
                }
            }

            // If we couldn't read a mission id, skip
            if (detected != missionId) continue;

            // We found a match; call CloseDoorAndGetMissionId via reflection
            MethodInfo closeMethod = t.GetMethod("CloseDoorAndGetMissionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (closeMethod == null)
            {
                Debug.LogWarning($"DoorFail: matching door '{t.Name}' does not expose CloseDoorAndGetMissionId().");
                return -1;
            }

            object ret = null;
            try
            {
                ret = closeMethod.Invoke(handle, null);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DoorFail: exception invoking CloseDoorAndGetMissionId on '{t.Name}': {ex}");
                return -1;
            }

            if (ret is int) return (int)ret;
            // If the method returned void, just return the missionId we matched
            return missionId;
        }

        // no match found
        return -1;
    }

    public void CloseEverything()
    {
        // Ensure we have handles to operate on
        if (doorHandles == null || doorHandles.Count == 0)
        {
            PopulateHandles();
            if (doorHandles == null || doorHandles.Count == 0)
            {
                Debug.LogWarning("DoorFail: no door handles found to CloseEverything().");
                return;
            }
        }

        foreach (var handle in doorHandles)
        {
            if (handle == null) continue;
            var t = handle.GetType();

            // Try to find the public CloseDoorAndGetMissionId method
            MethodInfo closeMethod = t.GetMethod("CloseDoorAndGetMissionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (closeMethod == null)
            {
                Debug.LogWarning($"DoorFail: handle '{t.Name}' does not expose CloseDoorAndGetMissionId().");
                continue;
            }

            try
            {
                object ret = closeMethod.Invoke(handle, null);
                if (ret is int)
                {
                    Debug.Log($"DoorFail: invoked CloseDoorAndGetMissionId on '{t.Name}', returned missionId {ret}.");
                }
                else
                {
                    // Method returned void or non-int - treat as successful close
                    Debug.Log($"DoorFail: invoked CloseDoorAndGetMissionId on '{t.Name}' (void or non-int return).");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DoorFail: exception invoking CloseDoorAndGetMissionId on '{t.Name}': {ex}");
            }
        }
    }

    // Public helper: request the TruckSpawnerManager to remove any queued truck for the
    // provided missionId and submit an empty box list to the MissionBehavior.
    public void CancelQueuedMissionAndNotify(int missionId)
    {
        // Try serialized reference first, then find in scene as a fallback
        if (truckSpawnerManager == null)
        {
            truckSpawnerManager = FindObjectOfType<TruckSpawnerManager>();
        }

        if (truckSpawnerManager == null)
        {
            Debug.LogWarning($"DoorFail: TruckSpawnerManager not found - cannot remove queued mission {missionId}.");
            return;
        }

        truckSpawnerManager.RemoveQueuedMissionAndSubmit(missionId);
        Debug.Log($"DoorFail: requested TruckSpawnerManager to RemoveQueuedMissionAndSubmit for missionId={missionId}.");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

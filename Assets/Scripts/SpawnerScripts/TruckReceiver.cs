using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles receiving pallets into the truck in receive mode. Absorbs pallets via trigger, stores them, and compares to the expected list.
/// </summary>
public class TruckReceiver : MonoBehaviour
{
    // The expected list of PalletData to compare against (set by TruckSpawnerManager)
    public List<PalletData> expectedPallets = new List<PalletData>();

    // The list of received pallets (populated as pallets are absorbed)
    public List<PalletData> receivedPallets = new List<PalletData>();

    // Optional mission identifier assigned by the TruckSpawnerManager. -1 means no mission.
    public int missionId = -1;

    /// <summary>
    /// Absorbs a pallet, adds its data to receivedPallets, and destroys the pallet GameObject.
    /// Called by ZoneAbsorber when a pallet enters a zone.
    /// </summary>
    public void AbsorbPallet(Pallet pallet)
    {
        if (pallet == null) return;
        // Convert the Pallet to PalletData for comparison
        PalletData data = new PalletData();
        data.typeOfBox = pallet.typeOfBox;
        data.amtOfPallet = pallet.amtOfPallet;
        data.boxDataList = new List<BoxData>();
        foreach (BoxData box in pallet.palletBoxes)
        {
            BoxData boxData = new BoxData();
            boxData.typeOfBox = box.typeOfBox;
            data.boxDataList.Add(boxData);
        }
        receivedPallets.Add(data);
        
        // Call BoxDataToIntArray on the scene's MissionBehavior (if present) to get counts for A/B/C boxes and update mission UI
        MissionBehavior mb = FindObjectOfType<MissionBehavior>();
        int[] boxArray = null;
        if (mb != null)
        {
            boxArray = mb.BoxDataToIntArray(data.boxDataList);
            if (boxArray != null && boxArray.Length >= 3)
                Debug.Log($"TruckReceiver: Absorbed pallet box counts A:{boxArray[0]} B:{boxArray[1]} C:{boxArray[2]}");
            else
                Debug.Log("TruckReceiver: Absorbed pallet box counts: null or unexpected length");

            // update the mission UI with the new counts
            mb.updateMissionQuotaUI(missionId, boxArray);
        }
        else
        {
            Debug.Log("TruckReceiver: No MissionBehavior found to update mission UI.");
        }

        
        // Destroy the absorbed pallet GameObject
        Destroy(pallet.gameObject);

        // // Optionally, check for completion
        // if (IsComplete())
        // {
        //     Debug.Log("All expected pallets received!");
        //     // Add further success logic here
        // }
    }

    /// <summary>
    /// Absorbs a pallet GameObject, adds its data to receivedPallets, and destroys the pallet GameObject.
    /// Called by ZoneAbsorber when a pallet enters a zone.
    /// </summary>
    public void AbsorbPallet(GameObject palletObj)
    {
        if (palletObj == null) return;
        Pallet pallet = palletObj.GetComponent<Pallet>();
        if (pallet == null) {
            Debug.LogWarning("Absorbed object does not have a Pallet component.");
            Destroy(palletObj); // Optionally destroy anyway, or skip
            return;
        }
        AbsorbPallet(pallet);
    }
   

    /// <summary>
    /// Sets the expected pallets for this truck (call this after spawning in receive mode).
    /// </summary>
    public void SetExpectedPallets(List<PalletData> expected)
    {
        expectedPallets = new List<PalletData>(expected);
        receivedPallets.Clear();
    }
}

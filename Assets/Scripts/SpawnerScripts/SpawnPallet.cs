using UnityEngine;
using System.Collections.Generic;

public class SpawnPallet : MonoBehaviour
{
    public GameObject palletPrefab;
    public GameObject boxPrefab; // Reference to Box prefab

    /// <summary>
    /// Spawns a pallet from the given DataBundle at a specific position and rotation.
    /// Uses up to 5 boxes from DataBundle to populate the pallet.
    /// </summary>
    public GameObject SpawnFromBundle(DataBundle dataBundle, Vector3 spawnPos, Quaternion? rotation = null)
    {
        if (palletPrefab == null)
        {
            Debug.LogError("SpawnPallet: Pallet prefab is not assigned!");
            return null;
        }

        Quaternion spawnRot = rotation ?? Quaternion.identity;
        GameObject palletObj = Instantiate(palletPrefab, spawnPos, spawnRot);
        Pallet palletScript = palletObj.GetComponent<Pallet>();

        if (palletScript == null)
        {
            palletScript = palletObj.AddComponent<Pallet>();
        }

        palletScript.typeOfBox = dataBundle.typeOfBox;
        palletScript.amtOfPallet = dataBundle.amtOfPallet;

        // Use up to 5 boxes from DataBundle
        int boxCount = Mathf.Min(dataBundle.boxes.Count, 5);
        for (int i = 0; i < boxCount; i++)
        {
            Box box = dataBundle.boxes[i];
            // Do NOT instantiate visible box GameObjects here; only record BoxData on the pallet
            BoxData bd = new BoxData();
            bd.typeOfBox = box.typeOfBox;
            palletScript.palletBoxes.Add(bd);
        }

        return palletObj;
    }

    // Optional backward-compatible method
    public GameObject SpawnFromBundle(DataBundle dataBundle)
    {
        return SpawnFromBundle(dataBundle, Vector3.zero);
    }

    /// <summary>
    /// [OPTIONAL] Spawns a pallet from a PalletData instance at a specific position and rotation.
    /// </summary>
    public GameObject SpawnFromPalletData(PalletData palletData, Vector3 spawnPos, Quaternion? rotation = null)
    {
        if (palletPrefab == null)
        {
            Debug.LogError("SpawnPallet: Pallet prefab is not assigned!");
            return null;
        }
        Quaternion spawnRot = rotation ?? Quaternion.identity;
        GameObject palletObj = Instantiate(palletPrefab, spawnPos, spawnRot);
        Pallet palletScript = palletObj.GetComponent<Pallet>();
        if (palletScript == null)
            palletScript = palletObj.AddComponent<Pallet>();
        // Use non-visual setter to populate pallet data without creating visible boxes
        palletScript.SetPalletData(palletData);
        return palletObj;
    }
    /// <summary>
    /// [OPTIONAL] Spawns a pallet from a PalletData instance at Vector3.zero.
    /// </summary>
    public GameObject SpawnFromPalletData(PalletData palletData)
    {
        return SpawnFromPalletData(palletData, Vector3.zero);
    }
}

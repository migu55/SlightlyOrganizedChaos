using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [OPTIONAL/LEGACY] DataBundle is a MonoBehaviour for scene-based workflows.
/// Prefer using PalletData and BoxData for data-driven workflows.
/// </summary>
public class DataBundle : MonoBehaviour
{
    // Type of box
    public string typeOfBox;

    // List of boxes in a pallet (max 5)
    // [OPTIONAL] Use BoxData instead of Box for a more data-driven approach.
    // public List<BoxData> boxes = new List<BoxData>();
    // See ToPalletData/FromPalletData for conversion.
    public List<Box> boxes = new List<Box>();

    // Amount of pallets
    public int amtOfPallet;

    /// <summary>
    /// [OPTIONAL/LEGACY] Adds a box to the list, enforcing the max of 5.
    /// </summary>
    public bool AddBox(Box box)
    {
        if (boxes.Count >= 5)
        {
            Debug.LogWarning("Cannot add more than 5 boxes to a pallet.");
            return false;
        }
        boxes.Add(box);
        return true;
    }

    /// <summary>
    /// [OPTIONAL] Convert this DataBundle to a PalletData instance.
    /// </summary>
    public PalletData ToPalletData()
    {
        var pd = new PalletData();
        pd.typeOfBox = this.typeOfBox;
        pd.amtOfPallet = this.amtOfPallet;
        foreach (var box in boxes)
        {
            var bd = new BoxData();
            bd.typeOfBox = box.typeOfBox;
            pd.boxDataList.Add(bd);
        }
        return pd;
    }

    /// <summary>
    /// [OPTIONAL] Create a DataBundle from a PalletData instance.
    /// </summary>
    public static DataBundle FromPalletData(PalletData pd)
    {
        var go = new GameObject("DataBundle");
        var db = go.AddComponent<DataBundle>();
        db.typeOfBox = pd.typeOfBox;
        db.amtOfPallet = pd.amtOfPallet;
        foreach (var bd in pd.boxDataList)
        {
            var box = go.AddComponent<Box>();
            box.typeOfBox = bd.typeOfBox;
            db.boxes.Add(box);
        }
        return db;
    }
}

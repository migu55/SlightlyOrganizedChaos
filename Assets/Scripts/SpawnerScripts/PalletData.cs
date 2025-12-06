using System.Collections.Generic;

/// <summary>
/// Serializable data class representing a pallet and its boxes.
/// </summary>
[System.Serializable]
public class PalletData
{
    /// <summary>
    /// The type of box this pallet contains (optional, can be set from first box).
    /// </summary>
    public string typeOfBox;
    /// <summary>
    /// The number of pallets this data represents (usually 1 for grouping logic).
    /// </summary>
    public int amtOfPallet;
    /// <summary>
    /// The list of box data objects in this pallet.
    /// </summary>
    public List<BoxData> boxDataList = new List<BoxData>();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PalletData() { }

    /// <summary>
    /// Constructor for easy creation from a list of BoxData.
    /// </summary>
    public PalletData(List<BoxData> boxes)
    {
        boxDataList = new List<BoxData>(boxes);
        typeOfBox = (boxes != null && boxes.Count > 0) ? boxes[0].typeOfBox : "";
        amtOfPallet = 1;
    }
}

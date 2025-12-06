using UnityEngine;
using System.Collections.Generic;

public class SpawnBox : MonoBehaviour
{
    // Reference to the Box prefab
    public GameObject boxPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method to spawn boxes from a Pallet
    public void SpawnBoxesFromPallet(Pallet pallet)
    {
        // amtOfBox is not present in Pallet, so this method should be refactored or removed if not needed
    }

    // Spawns a list of boxes for a given type and amount
    public List<Box> SpawnBoxes(string typeOfBox, int amtOfBox)
    {
        List<Box> boxes = new List<Box>();
        for (int i = 0; i < amtOfBox; i++)
        {
            GameObject boxObj = Instantiate(boxPrefab);
            Box boxScript = boxObj.GetComponent<Box>();
            if (boxScript == null)
            {
                boxScript = boxObj.AddComponent<Box>();
            }
            boxScript.typeOfBox = typeOfBox;
            boxes.Add(boxScript);
        }
        return boxes;
    }

    /// <summary>
    /// [OPTIONAL] Spawns a list of boxes from BoxData.
    /// </summary>
    public List<Box> SpawnBoxes(BoxData boxData, int count)
    {
        List<Box> boxes = new List<Box>();
        for (int i = 0; i < count; i++)
        {
            GameObject boxObj = Instantiate(boxPrefab);
            Box boxScript = boxObj.GetComponent<Box>();
            if (boxScript == null)
                boxScript = boxObj.AddComponent<Box>();
            boxScript.typeOfBox = boxData.typeOfBox;
            boxes.Add(boxScript);
        }
        return boxes;
    }
}


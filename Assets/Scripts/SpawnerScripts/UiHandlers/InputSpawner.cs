using UnityEngine;
using TMPro;

public class InputSpawner : MonoBehaviour
{
    public TMP_InputField inputField;
    public SpawnPallet spawnPallet;
    public Collider spawnZone; // Optional — assign one of your box zones

    void Start()
    {
        inputField.onEndEdit.AddListener(OnInputSubmit);
    }

    void OnInputSubmit(string input)
    {
        string[] parts = input.Split(' ');
        if (parts.Length != 3)
        {
            Debug.LogError("Input must be in format: typeOfBox amtOfBox amtOfPallet");
            return;
        }

        string typeOfBox = parts[0];
        if (!int.TryParse(parts[1], out int amtOfBox) || !int.TryParse(parts[2], out int amtOfPallet))
        {
            Debug.LogError("amtOfBox and amtOfPallet must be integers.");
            return;
        }

        // Create DataBundle
        GameObject dataBundleObj = new GameObject("DataBundle");
        DataBundle dataBundle = dataBundleObj.AddComponent<DataBundle>();
        dataBundle.typeOfBox = typeOfBox;
        dataBundle.amtOfPallet = amtOfPallet;

        Vector3 spawnPos = Vector3.zero;

        // If a spawn zone is assigned, spawn inside it
        if (spawnZone != null)
        {
            Bounds bounds = spawnZone.bounds;
            spawnPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        spawnPallet.SpawnFromBundle(dataBundle, spawnPos);
        Destroy(dataBundleObj);
    }
}
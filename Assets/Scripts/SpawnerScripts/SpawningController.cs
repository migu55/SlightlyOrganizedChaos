using UnityEngine;
using System.Collections;
using static SpawnBundle;

public class SpawningController : MonoBehaviour
{
    public GameObject[] objectsToSpawn;
    private SpawnBundle[] spawnBundles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        spawnBundles = GetComponents<SpawnBundle>();
        Debug.Log("Spawning " + objectsToSpawn.Length + " individual objects and " + spawnBundles.Length + " spawn bundles.");
        for (int i = 0; i < objectsToSpawn.Length; i++)
        {
            GameObject objectToSpawn = objectsToSpawn[i];
            Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
        }

        for (int i = 0; i < spawnBundles.Length; i++)
        {
            Debug.Log("Spawning bundle " + i + " at location " + spawnBundles[i].GetSpawnLocation());
            SpawnBundle spawnBundle = spawnBundles[i];
            Instantiate(spawnBundle.prefabToSpawn, spawnBundle.GetSpawnLocation());
        }
    }
}


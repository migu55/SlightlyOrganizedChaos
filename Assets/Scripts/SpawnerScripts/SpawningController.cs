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
        for (int i = 0; i < objectsToSpawn.Length; i++)
        {
            GameObject objectToSpawn = objectsToSpawn[i];
            Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
        }

        for (int i = 0; i < spawnBundles.Length; i++)
        {
            SpawnBundle spawnBundle = spawnBundles[i];
            Instantiate(spawnBundle.prefabToSpawn, spawnBundle.GetSpawnLocation());
        }
    }
}


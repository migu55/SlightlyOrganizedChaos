using UnityEngine;

public class SpawnBundle : MonoBehaviour
{

    public GameObject prefabToSpawn;
    public float X;
    public float Y;
    public float Z;
    private Transform spawnLocation;

    public void Start()
    {
        if (spawnLocation == null)
        {
            spawnLocation = new GameObject("SpawnLocation").transform;
            spawnLocation.position = new Vector3(X, Y, Z);
            spawnLocation.parent = this.transform;
        }
    }

    public Transform GetSpawnLocation()
    {
        return spawnLocation;
    }

}

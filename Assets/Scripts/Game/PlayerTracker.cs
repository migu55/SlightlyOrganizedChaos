using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    // List of all players, used to position them in the spawn points on load, and add them to the ready system
    public List<GameObject> players;

    [SerializeField]
    // List of the 4 spawn points players will be placed when loading for the first time or into a round
    private List<Transform> spawns;
    [SerializeField]
    // Prefab for the ready text box
    private GameObject textPrefab;
    // Reference to the ready system, used for adding new players to it
    private ClockInHandler readySystem;

    // Finds the ready system, and sets up the empty list of players
    void Start()
    {
        readySystem = GameObject.FindGameObjectWithTag("ClockInButton").GetComponent<ClockInHandler>();
        players = new();
    }

    // Checks if any new players have joing, and if so then add them to the list and spawn them where the next
    // available spot is
    void Update()
    {
        var allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in allPlayers)
        {
            if (!players.Contains(p))
            {
                // Add new player to list and set them to spawn point position/rotation, add them to ready system
                players.Add(p);
                SetPlayerToSpawn(p);
                readySystem = GameObject.FindGameObjectWithTag("ClockInButton").GetComponent<ClockInHandler>();
                readySystem.AddNewPlayer(p);

                // Reset the transform of the text holder to ensure it is where it should be
                var textHolder = p.GetComponentInChildren<ReadyTextHolder>();
                textHolder.ResetTransform();
                
                // Instantiate the ready text and set its parent to the text holder, position/rotation as well
                GameObject txt = Instantiate(textPrefab);
                txt.transform.SetParent(textHolder.gameObject.transform);
                txt.transform.SetLocalPositionAndRotation(new(0,0,0), new(0,0,0,0));
            }
        }
    }

    // Sets player to their spawn point
    public void SetPlayerToSpawn(GameObject p)
    {
        p.transform.SetPositionAndRotation(spawns[players.IndexOf(p)].position, spawns[players.IndexOf(p)].rotation);
    }
}

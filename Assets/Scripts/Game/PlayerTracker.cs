using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public List<GameObject> players;
    public List<GameObject> readyTexts;

    [SerializeField]
    private List<Transform> spawns;
    [SerializeField]
    private GameObject textPrefab;
    private ClockInHandler readySystem;

    void Start()
    {
        readySystem = GameObject.FindGameObjectWithTag("ClockInButton").GetComponent<ClockInHandler>();
        players = new();
        readyTexts = new();
    }

    void Update()
    {
        var allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in allPlayers)
        {
            if (!players.Contains(p))
            {
                players.Add(p);
                p.transform.SetPositionAndRotation(spawns[players.IndexOf(p)].position, spawns[players.IndexOf(p)].rotation);
                readySystem = GameObject.FindGameObjectWithTag("ClockInButton").GetComponent<ClockInHandler>();
                readySystem.AddNewPlayer(p);

                var textHolder = p.GetComponentInChildren<ReadyTextHolder>();
                textHolder.ResetTransform();
                
                GameObject txt = Instantiate(textPrefab);
                txt.transform.SetParent(textHolder.gameObject.transform);
                txt.transform.SetLocalPositionAndRotation(new(0,0,0), new(0,0,0,0));
                readyTexts.Add(txt);
            }
        }
    }
}

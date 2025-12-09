using UnityEngine;
using System.Collections.Generic;


// To be placed on the forklift
public class TriggerDetector : MonoBehaviour
{
    public bool triggered = false;
    public List<GameObject> player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new List<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
                triggered = true;
                if(!player.Contains(other.gameObject)) {
                    player.Add(other.gameObject);
                }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggered && other.gameObject.tag == "Player")
        {
            player.Remove(other.gameObject);
            if(player.Count == 0) triggered = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

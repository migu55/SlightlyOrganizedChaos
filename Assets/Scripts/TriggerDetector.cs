using UnityEngine;


// To be placed on the forklift
public class TriggerDetector : MonoBehaviour
{
    public bool triggered = false;
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.gameObject.tag == "Player")
        {
            Debug.Log("Player entered trigger");
            triggered = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggered && other.gameObject.tag == "Player")
        {
            triggered = false;
            player = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

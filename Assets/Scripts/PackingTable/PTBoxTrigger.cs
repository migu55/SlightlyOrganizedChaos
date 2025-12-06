using UnityEngine;
using UnityEngine.UIElements;

public class PTBoxTrigger : MonoBehaviour
{

    [SerializeField]
    GameObject ptCore;

    private PTController ptc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ptc = ptCore.GetComponent<PTController>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (ptc.isLoading)
        {
            if (other.gameObject.tag == "Box")
            {
                ptc.LoadBoxFromBoxTrigger(other.gameObject);
                Destroy(other.gameObject);
            }
        }

    }
}

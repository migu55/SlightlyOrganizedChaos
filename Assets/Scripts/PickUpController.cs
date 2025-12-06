using UnityEngine;

public class PickUpController : MonoBehaviour, Interactable
{
    private bool pickedUp;
    private ForkliftPickup forkliftPickup; // reference to the forklift's pickup system

    void Start()
    {
        // Try to find ForkliftPickup on parent or nearby
        forkliftPickup = GetComponentInParent<ForkliftPickup>();
        if (forkliftPickup == null)
        {
            forkliftPickup = FindObjectOfType<ForkliftPickup>();
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!pickedUp)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.transform.SetParent(interactor.transform.Find("BoxPoint").gameObject.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.rotation = interactor.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            pickedUp = true;
        } else
        {
            gameObject.transform.SetParent(null);
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            pickedUp = false;
        }
    }

    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.InputSystem;


// To be placed on the forklift
public class MountForklift : MonoBehaviour, Interactable
{
    private Transform playerTransform;
    private Transform cameraTransform;
    private Rigidbody forkliftRigidbody;
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    private Camera forkliftCamera;

    private GameObject player;
    public bool mounted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        forkliftCamera = GetComponentInChildren<Camera>();
        forkliftRigidbody = GetComponent<Rigidbody>();
    }

    public void Interact(GameObject interactor)
    {
        MountOrDismount(interactor, interactor.GetComponentInChildren<Camera>());
    }

    public void MountOrDismount(GameObject player, Camera playerCamera)
    {
        if(mounted && this.player != player)
        {
            Debug.Log("Forklift already mounted by another player");
            return;
        }
        this.player = player;
        this.playerCamera = playerCamera;

        Debug.Log("Mounting or Dismounting");
        if (!mounted)
        {
            var pickupController = player.GetComponentInChildren<PickUpController>();
            if (pickupController == null)
            {
                Debug.Log("Getting on forklift");
                Mount();
            }
        }
        else
        {
            Dismount();
        }
    }

    private void Mount()
    {
        Debug.Log("Mounting");

        //Setting the forklift camera to the active camera and turning off the player camera
        forkliftCamera.rect = playerCamera.rect;
        forkliftCamera.depth = playerCamera.depth + 1;
        playerCamera.enabled = false;
        forkliftCamera.enabled = true;
        forkliftRigidbody.isKinematic = false;

        //Setting the player position to the forklift position
        playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.isKinematic = true; //disable physics on player while mounted
        player.GetComponent<CapsuleCollider>().enabled = false; //disable player collider while mounted
        player.transform.position = transform.Find("MountPoint").position;
        player.transform.rotation = transform.Find("MountPoint").rotation;
        player.transform.SetParent(transform);

        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<InputBridge>().enabled = true;
        player.GetComponent<InputBridge>().SetController(GetComponent<ForkliftController>());
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Forklift");

        mounted = true;
    }

    public void Dismount()
    {
        if (!mounted) return;

        //enabling the player camera and disabling the forklift camera
        Debug.Log("Dismounting");
        playerCamera.enabled = true;
        forkliftCamera.enabled = false;
        forkliftRigidbody.isKinematic = true;

        player.transform.SetParent(null);
        player.transform.position = transform.Find("DismountPoint").position;
        player.transform.rotation = transform.Find("DismountPoint").rotation;

        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<InputBridge>().ClearController();
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        player.GetComponent<InputBridge>().enabled = true;

        //setting the player position to the dismount point
        playerRigidbody.isKinematic = false; //enable physics on player when dismounted
        player.GetComponent<CapsuleCollider>().enabled = true; //enable player collider when dis
        playerTransform = null;
        playerRigidbody = null;

        mounted = false;
    }
}

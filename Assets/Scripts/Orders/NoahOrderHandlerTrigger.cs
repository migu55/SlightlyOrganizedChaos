using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class NoahOrderHandlerTrigger : MonoBehaviour, Interactable
{
    public float price;
    public List<GameObject> playersInArea;
    
    [SerializeField]
    private TextMeshPro textBox;
    [SerializeField]
    private UIController uiController;
    [SerializeField]
    private Camera menuCamera;
    private AimConstraint pointer;
    private ProductOrderingMenu menu;
    private InputActionMap uiControls;

    void Start()
    {
        textBox.GetComponent<Renderer>().enabled = false;
        pointer = GetComponentInChildren<AimConstraint>();
        pointer.RemoveSource(0);
        menu = GameObject.FindGameObjectWithTag("Order Menu").GetComponent<ProductOrderingMenu>();
        uiControls = new InputActions().UI;
        playersInArea = new();
    }

    void Update()
    {
        foreach (var p in playersInArea)
        {
            if (p.GetComponent<PlayerInput>().actions.FindAction("Cancel").IsPressed())
            {
                OpenOrCloseMenu(p);
            }
        }
    }

    public void OpenOrCloseMenu(GameObject playerObj)
    {
        if (!playerObj.TryGetComponent<NoahOrderHandlerPlayer>(out var orderHandler))
        {
            Debug.LogWarning("Failed to find player order handler when interacting with laptop");
            return;
        }

        var input = playerObj.GetComponent<PlayerInput>();
        var bridge = playerObj.GetComponent<InputBridge>();

        if (!orderHandler.isMenuOpen)
        {
            OpenMenu(playerObj);
            orderHandler.isMenuOpen = true;
            bridge.SetController(uiController);
            input.SwitchCurrentActionMap("UI");
        } else
        {
            CloseMenu(playerObj);
            orderHandler.isMenuOpen = false;
            bridge.ClearController();
            input.SwitchCurrentActionMap("Player");
        }
    }

    private void OpenMenu(GameObject player)
    {
        Camera playerCamera = player.GetComponent<PlayerController>().GetCamera();
        menuCamera.rect = playerCamera.rect;
        menuCamera.depth = playerCamera.depth + 1;
        playerCamera.enabled = false;
        menuCamera.enabled = true;

        // Activate menu handling
        menu.EnableMenu();
        uiControls.Enable();
    }

    private void CloseMenu(GameObject player)
    {
        Camera playerCamera = player.GetComponent<PlayerController>().GetCamera();
        playerCamera.enabled = true;
        menuCamera.enabled = false;

        // Deactivate menu handling
        menu.DisableMenu();
        uiControls.Disable();
    }

    public void Interact(GameObject interactor)
    {
        OpenOrCloseMenu(interactor);
    }

}

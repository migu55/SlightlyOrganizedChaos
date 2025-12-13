using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductOrderingMenu : MonoBehaviour
{
    [SerializeField]
    // Display for box amount on the UI
    private TextMeshProUGUI counter;
    [SerializeField]
    // Used to track slider value and update it with the custom navigation
    private Slider slider;
    [SerializeField]
    // Title text telling the player what type of box they are about to order
    private TextMeshProUGUI label;
    [SerializeField]
    // List of all button texts, used to update the prices displayed
    private List<TextMeshProUGUI> buttonsText;
    [SerializeField]
    // List of the available box types, defined in child objects
    private List<Box> boxTypes;
    [SerializeField]
    // List of the prices associated with the boxes
    private List<int> prices;
    [SerializeField]
    // Used for setting the starting selected object so that the menu can work
    private GameObject buttonA;
    [SerializeField]
    // Used for displaying the current order list
    private TextMeshProUGUI order;
    [SerializeField]
    // Used for displaying the total price of the order
    private TextMeshProUGUI pricing;
    [SerializeField]
    // Reference to the trigger pad used to open the menu
    private NoahOrderHandlerTrigger menuButton;

    // Name is kinda self explanatory, the box that will next be added to the order
    private Box boxToOrder;
    // Orders are sent here to be spawned into trucks
    private TruckSpawnerManager spawner;
    // Persistent basket so player can add multiple types/quantities before placing the order
    private List<BoxData> orderBasket = new List<BoxData>();
    // Total price value, updated whenever the player adds a box to the order
    private int totalPrice;
    // List of names of boxes, used with UpdateText() to display the names of the A,B,C types set in code
    private string[] names = { "Apple", "Blueberry", "Melon"};

    // Finds box types in child objects, and updates the box type to the default A type, with count 1
    void Start()
    {
        boxTypes.AddRange(GetComponentsInChildren<Box>());
        boxTypes.Remove(GameObject.FindGameObjectWithTag("BoxToOrder").GetComponent<Box>());
        for (int i = 0; i < boxTypes.Count; i++) { prices.Add(i); }
        boxToOrder = GameObject.FindGameObjectWithTag("BoxToOrder").GetComponent<Box>();
        UpdateBoxType(boxTypes[0].typeOfBox);
        UpdateBoxCount(1);
    }

    // Checks if the truck spawner exists and assigns it if not already
    void Update()
    {
        if (spawner == null)
        {
            spawner = GameObject.FindGameObjectWithTag("TruckSpawnHolder").GetComponent<TruckSpawnerManager>();
        }    
    }

    // Converts the box type (A,B,C) into the name of the box type (Apple,Blueberry,Melon)
    public string ConvertIndexToName(string letter)
    {
        return letter switch
        {
            "A" => names[0],
            "B" => names[1],
            "C" => names[2],
            _ => names[0],
        };

    }

    // Enables the menu state by setting the selected object to the first button
    public void EnableMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonA);
        Cursor.lockState = CursorLockMode.None;
    }

    // Disables the menu state by selecting nothing, preventing players from changing or activating the menu by accident 
    public void DisableMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Updates the prices in the prices list and calls UpdateText() to display the new prices to the player
    public void UpdatePrices(List<int> newPrices)
    {
        prices.Clear();
        foreach (int price in newPrices)
        {
            prices.Add(price);
        }
        UpdateText();
    }

    // Updates all text boxes in the menu with new data when it comes in
    private void UpdateText()
    {
        // Buttons texts
        for (int i = 0; i < boxTypes.Count; i++)
        {
            buttonsText[i].text = "Box o' " + names[i] + "\nPrice: $ " + prices[i];
        }
        
        // Title label and current order title
        label.text = "Select Box Type:\n" + ConvertIndexToName(boxToOrder.typeOfBox);
        order.text = "Current Order";

        // Current order details and total price
        foreach (Box b in boxTypes)
        {
            int count = 0;
            foreach (BoxData d in orderBasket)
            {
                if (d.typeOfBox == b.typeOfBox) count++;
            }
            order.text += $"\n{count}x {ConvertIndexToName(b.typeOfBox)}";
        }
        pricing.text = $"Total Price\n$ {totalPrice}";
    }

    // Updates the currently selected box type, and displays to the player
    public void UpdateBoxType(string newBoxType)
    {
        foreach (Box box in boxTypes)
        {

            if (box.typeOfBox == newBoxType)
            {
                boxToOrder.typeOfBox = newBoxType;
                slider.gameObject.GetComponent<UIItem>().boxType = newBoxType;
            }
        }
        UpdateText();
    }

    // Updates the box count and the text for its display
    public void SliderUpdate()
    {
        counter.text = slider.value.ToString();
        UpdateBoxCount((int) slider.value);
    }

    // Sets the amount of boxes to order to the value given by the slider
    public void UpdateBoxCount(int numBoxes)
    {
        boxToOrder.amtOfBox = numBoxes;
    }

    // Adds the selected box type and count of them to the order basket. Updates the text of the menu
    public void AddToOrder()
    {
        // If somehow the players have deleted the box to be ordered, throw error
        if (boxToOrder == null)
        {
            Debug.LogError("ProductOrderingMenu.AddToOrder: boxToOrder is null.");
            return;
        }

        // Default to 1 if can't get the amount of boxes to order, or if amount of boxes to order is 0 somehow
        int count = Mathf.Max(1, boxToOrder.amtOfBox);

        // Add new box(es) to the order basket
        for (int i = 0; i < count; i++)
        {
            orderBasket.Add(new BoxData() { typeOfBox = boxToOrder.typeOfBox });
            int index = 0;
            foreach (Box b in boxTypes)
            {
                if (b.typeOfBox == boxToOrder.typeOfBox) index = boxTypes.IndexOf(b);
            }
            totalPrice += prices[index];
        }

        // Update the text of the menu to display the order details
        UpdateText();
    }

    // Clears the order basket and resets the text display
    public void ClearOrder()
    {
        orderBasket.Clear();
        totalPrice = 0;
        UpdateText();
    }

    // Sends the order to the truck spawner to be sent to one of the truck bays.
    public void PlaceOrder()
    {
        // Throw error if box to be ordered doesn't exist
        if (boxToOrder == null)
        {
            Debug.LogError("ProductOrderingMenu.PlaceOrder: boxToOrder is null. Check that a GameObject with tag 'BoxToOrder' exists and has a Box component.");
            return;
        }

        // Throw error if the truck spawner doesn't exist
        if (spawner == null)
        {
            Debug.LogError("ProductOrderingMenu.PlaceOrder: spawner is null. Check that a GameObject with tag 'TruckSpawnHolder' exists and has TruckSpawnerManager.");
            return;
        }

        // If the player has added items to the basket, send those; otherwise fall back to the single current selection
        List<BoxData> boxesToSend;
        if (orderBasket != null && orderBasket.Count > 0)
        {
            boxesToSend = new List<BoxData>(orderBasket);
        }
        else
        {
            boxesToSend = new List<BoxData>();
            int count = Mathf.Max(1, boxToOrder.amtOfBox);
            for (int i = 0; i < count; i++) boxesToSend.Add(new BoxData() { typeOfBox = boxToOrder.typeOfBox });
        }

        // Use diagnostic entry to get extra logging from the spawner
        spawner.spawnTruck_Diagnostic(-1, boxesToSend, true);

        // Clear the persistent basket after sending, and subtract price from budget.
        orderBasket.Clear();
        GameStats.Instance.gameBalance -= totalPrice; totalPrice = 0;

        // Update the text on the menu to reset its state to what it was upon opening
        UpdateText();

        // Force close the menu on all players in the area, and play the order placed sound effect
        foreach (GameObject p in menuButton.playersInArea)
        {
            if (p.GetComponent<NoahOrderHandlerPlayer>().isMenuOpen)
            {
                menuButton.OpenOrCloseMenu(p);
            }
        }
        SFXController.Instance.PlayClip(SFXController.Instance.orderPlaced);
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductOrderingMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI counter;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private TextMeshProUGUI label;
    [SerializeField]
    private List<TextMeshProUGUI> buttonsText;
    [SerializeField]
    private List<Button> buttons;
    [SerializeField]
    private List<Box> boxTypes;
    [SerializeField]
    private List<int> prices;
    [SerializeField]
    private GameObject buttonA;
    [SerializeField]
    private TextMeshProUGUI order;
    [SerializeField]
    private TextMeshProUGUI pricing;
    [SerializeField]
    private NoahOrderHandlerTrigger menuButton;

    private Box boxToOrder;
    private TruckSpawnerManager spawner;
    // Persistent basket so player can add multiple types/quantities before placing the order
    private List<BoxData> orderBasket = new List<BoxData>();
    private int totalPrice;

    void Start()
    {
        buttons.AddRange(GetComponentsInChildren<Button>());
        boxTypes.AddRange(GetComponentsInChildren<Box>());
        boxTypes.Remove(GameObject.FindGameObjectWithTag("BoxToOrder").GetComponent<Box>());
        for (int i = 0; i < boxTypes.Count; i++) { prices.Add(i); }
        boxToOrder = GameObject.FindGameObjectWithTag("BoxToOrder").GetComponent<Box>();
        UpdateBoxType(boxTypes[0].typeOfBox);
        UpdateBoxCount(1);
    }

    void Update()
    {
        if (spawner == null)
        {
            spawner = GameObject.FindGameObjectWithTag("TruckSpawnHolder").GetComponent<TruckSpawnerManager>();
        }    
    }

    public void EnableMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonA);
        Cursor.lockState = CursorLockMode.None;
    }

    public void DisableMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UpdatePrices(List<int> newPrices)
    {
        prices.Clear();
        foreach (int price in newPrices)
        {
            prices.Add(price);
        }
        UpdateText();
    }

    private void UpdateText()
    {
        for (int i = 0; i < boxTypes.Count; i++)
        {
            buttonsText[i].text = "Box o' " + boxTypes[i].typeOfBox + "\nPrice: $ " + prices[i];
        }
        label.text = "Select Box Type:\n" + boxToOrder.typeOfBox;
        order.text = "Current order:";
        int comma = 0;
        foreach (Box b in boxTypes)
        {
            int count = 0;
            foreach (BoxData d in orderBasket)
            {
                if (d.typeOfBox == b.typeOfBox) count++;
            }
            order.text += $"\n{count}x {b.typeOfBox}";
            if (comma < 2) order.text += ",";
            comma++;
        }
        pricing.text = $"Total Price:\n$ {totalPrice}";
    }

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

    public void SliderUpdate()
    {
        counter.text = slider.value.ToString();
        UpdateBoxCount((int) slider.value);
    }

    public void UpdateBoxCount(int numBoxes)
    {
        boxToOrder.amtOfBox = numBoxes;
    }

    // Call this from a UI "Add to Order" button so multiple selections accumulate
    public void AddToOrder()
    {
        if (boxToOrder == null)
        {
            Debug.LogError("ProductOrderingMenu.AddToOrder: boxToOrder is null.");
            return;
        }

        int count = Mathf.Max(1, boxToOrder.amtOfBox);
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
        UpdateText();
        Debug.Log($"ProductOrderingMenu.AddToOrder: Added {count} x '{boxToOrder.typeOfBox}' to basket (total={orderBasket.Count}).");
    }

    public void ClearOrder()
    {
        Debug.Log("Clear Order");
        orderBasket.Clear();
        UpdateText();
    }

    public void PlaceOrder()
    {
        if (boxToOrder == null)
        {
            Debug.LogError("ProductOrderingMenu.PlaceOrder: boxToOrder is null. Check that a GameObject with tag 'BoxToOrder' exists and has a Box component.");
            return;
        }

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

        Debug.Log($"ProductOrderingMenu.PlaceOrder: Sending {boxesToSend.Count} boxes to spawner. (basketBeforeClear={ (orderBasket==null?0:orderBasket.Count) })");
        // use diagnostic entry to get extra logging from the spawner
        spawner.spawnTruck_Diagnostic(-1, boxesToSend, true);

        // Clear the persistent basket after sending, and subtract price from budget.
        orderBasket.Clear();
        GameStats.Instance.gameBalance -= totalPrice; totalPrice = 0;
        UpdateText();
        foreach (GameObject p in menuButton.playersInArea)
        {
            if (p.GetComponent<NoahOrderHandlerPlayer>().isMenuOpen)
            {
                Debug.Log("Player in menu, closing...");
                menuButton.OpenOrCloseMenu(p);
            }
        }
        SFXController.Instance.PlayClip(SFXController.Instance.orderPlaced);
        Debug.Log("ProductOrderingMenu.PlaceOrder: Cleared order basket after sending.");
    }
}

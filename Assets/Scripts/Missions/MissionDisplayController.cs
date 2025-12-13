using UnityEngine;

/// <summary>
/// Controls the UI display for mission details including truck info and product requirements.
/// Updates text components to show mission ID and required quantities.
/// </summary>
public class MissionDisplayController : MonoBehaviour
{
    // Reference to the truck GameObject currently assigned to this mission display
    public GameObject missionTruck;
    
    // UI element displaying the mission order number
    public GameObject orderNumber;
    
    // Reference to the mission manager that tracks all active missions
    [SerializeField]
    GameObject missionManager;

    // Array of UI GameObjects showing individual product requirements
    public GameObject[] productRequirements;
    
    // UI element that appears when the truck is ready to receive pallets
    public GameObject receiveDisplay;
    public GameObject closeDoorDisplay;

    // Cached mission data for the currently displayed mission
    private MissionData missionData;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find and cache the mission manager in the scene
        missionManager = GameObject.Find("MissionManager");
    }

    /// <summary>
    /// Assigns a truck to this display and updates all UI elements with mission details.
    /// Retrieves mission data and displays order number and product quantities.
    /// </summary>
    /// <param name="truck">The truck GameObject containing TruckReceiver component with mission info</param>
    public void SetMissionTruck(GameObject truck)
    {
        orderNumber.SetActive(true);
        missionTruck = truck;
        
        // Retrieve mission data using the truck's mission ID
        missionData = missionManager.GetComponent<MissionBehavior>().getMissionWithMissionID(truck.GetComponent<TruckReceiver>().missionId);

        // Get the truck receiver component and build order number text
        var receiver = missionTruck.GetComponent<TruckReceiver>();
        string idText = "Order Number: " + (receiver != null ? receiver.missionId.ToString() : "");

        // Try to update standard Unity UI Text component for order number
        var uiText = orderNumber.GetComponent<UnityEngine.UI.Text>();
        if (uiText != null)
        {
            uiText.text = idText;
        }

        // Get TextMeshPro type for potential TMP usage
        var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        
        // Update product requirement displays if mission data is available
        if (missionData != null && productRequirements != null)
        {
            for (int i = 0; i < productRequirements.Length; i++)
            {
                var reqGO = productRequirements[i];
                if (reqGO == null) continue;
                else
                {
                    // Activate the requirement display element
                    reqGO.SetActive(true);
                }

                // Build quantity text for this product
                string qtyText = "Product:        ";
                if (missionData.MissionQuantities != null && i < missionData.MissionQuantities.Length)
                    qtyText += missionData.MissionQuantities[i].ToString();

                // Try standard UI Text first
                var uiReqText = reqGO.GetComponent<UnityEngine.UI.Text>();
                if (uiReqText != null)
                {                   
                    uiReqText.text = qtyText;
                    continue;
                }

                // Try TextMeshPro if standard UI Text wasn't found
                if (tmpType != null)
                {
                    var tmpReqComp = reqGO.GetComponent(tmpType);
                    if (tmpReqComp != null)
                    {
                        var textProp = tmpType.GetProperty("text");
                        textProp?.SetValue(tmpReqComp, qtyText, null);
                        continue;
                    }
                }

            }
        }

        // Try updating order number with TextMeshPro if standard UI Text wasn't available
        if (tmpType != null)
        {
            var tmpComp = orderNumber.GetComponent(tmpType);
            if (tmpComp != null)
            {
                var textProp = tmpType.GetProperty("text");
                if (textProp != null) textProp.SetValue(tmpComp, idText, null);
                return;
            }
        }

<<<<<<< HEAD
        Debug.LogWarning("OrderNumber has no supported text component (UnityEngine.UI.Text or TextMeshProUGUI).");
=======


>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
    }

    /// <summary>
    /// Activates the receive display when a truck is ready to accept pallets.
    /// </summary>
    /// <param name="truck">The truck GameObject ready to receive pallets</param>
    public void SetRecieveTruck(GameObject truck) {
        orderNumber.SetActive(false);
        receiveDisplay.SetActive(true);
    }

<<<<<<< HEAD
    /// <summary>
    /// Clears the mission truck reference and resets all UI displays to default state.
    /// Hides product requirements and receive display.
    /// </summary>
=======
    public void SetCloseDoorDisplay() {
        orderNumber.SetActive(false);
        closeDoorDisplay.SetActive(true);
        receiveDisplay.SetActive(false);
    }


>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
    public void ClearMissionTruck()
    {
        // Clear the truck reference
        missionTruck = null;
        
        // Reset order number text
        var uiText = orderNumber.GetComponent<UnityEngine.UI.Text>();
        if (uiText != null)
        {
            uiText.text = "Order Number: ";
        }

        // Deactivate all product requirement displays
        foreach (var reqGO in productRequirements)
        {
            if (reqGO != null)
            {
                reqGO.SetActive(false);
            }
        }
        
        // Hide the receive display
        receiveDisplay.SetActive(false);
        closeDoorDisplay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

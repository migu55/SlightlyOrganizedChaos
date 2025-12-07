using UnityEngine;

public class MissionDisplayController : MonoBehaviour
{
    public GameObject missionTruck;
    public GameObject orderNumber;
    
    [SerializeField]
    GameObject missionManager;

    public GameObject[] productRequirements;
    public GameObject receiveDisplay;

    private MissionData missionData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        missionManager = GameObject.Find("MissionManager");
    }

    public void SetMissionTruck(GameObject truck)
    {
        missionTruck = truck;
        missionData = missionManager.GetComponent<MissionBehavior>().getMissionWithMissionID(truck.GetComponent<TruckReceiver>().missionId);
        //Debug.Log("Mission truck set to: " + truck.GetComponent<TruckReceiver>().missionId + " Expected pallets: " + missionData.MissionQuantities[0] + ", " + missionData.MissionQuantities[1] + ", " + missionData.MissionQuantities[2]);

        var receiver = missionTruck.GetComponent<TruckReceiver>();
        string idText = "Order Number: " + (receiver != null ? receiver.missionId.ToString() : "");

        var uiText = orderNumber.GetComponent<UnityEngine.UI.Text>();
        if (uiText != null)
        {
            uiText.text = idText;
        }


        var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (missionData != null && productRequirements != null)
        {
            for (int i = 0; i < productRequirements.Length; i++)
            {
                var reqGO = productRequirements[i];
                if (reqGO == null) continue;
                else
                {
                    reqGO.SetActive(true);
                }

                string qtyText = "Product:        ";
                if (missionData.MissionQuantities != null && i < missionData.MissionQuantities.Length)
                    qtyText += missionData.MissionQuantities[i].ToString();

                
                var uiReqText = reqGO.GetComponent<UnityEngine.UI.Text>();
                if (uiReqText != null)
                {                   
                    uiReqText.text = qtyText;
                    continue;
                }

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

                Debug.LogWarning("Product requirement GameObject '" + reqGO.name + "' has no supported text component.");
            }
        }

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



        Debug.LogWarning("OrderNumber has no supported text component (UnityEngine.UI.Text or TextMeshProUGUI).");
    }

    public void SetRecieveTruck(GameObject truck) {
        receiveDisplay.SetActive(true);
    }


    public void ClearMissionTruck()
    {
        missionTruck = null;
        var uiText = orderNumber.GetComponent<UnityEngine.UI.Text>();
        if (uiText != null)
        {
            uiText.text = "Order Number: ";
        }

        foreach (var reqGO in productRequirements)
        {
            if (reqGO != null)
            {
                reqGO.SetActive(false);
            }
        }
        receiveDisplay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

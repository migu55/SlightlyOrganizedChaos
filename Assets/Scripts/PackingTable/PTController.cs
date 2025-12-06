using System.Collections;
using UnityEngine;

public class PTController : MonoBehaviour
{

    [SerializeField]
    GameObject boxEndpoint;
    [SerializeField]
    GameObject palletEndpoint;

    [SerializeField]
    GameObject palletPrefab;
    [SerializeField]
    GameObject boxPrefabA;
    [SerializeField]
    GameObject boxPrefabB;
    [SerializeField]
    GameObject boxPrefabC;

    private GameObject currentBoxType;

    public GameObject currentPallet;
    Pallet palletData;

    public bool isLoading, hasPallet;
    private bool flag;

    public void LoadBoxFromBoxTrigger(GameObject boxGameObject)
    {
        Box b = boxGameObject.GetComponent<Box>();
        if (flag || b == null) return;
        flag = true;

        if (!hasPallet)
        {
            SpawnPallet();
        }

        //make sure pallet cannot be picked up to avoid errors
        if (palletData.palletBoxes.Count < 5)
        {
            BoxData box = new BoxData();
            box.typeOfBox = b.typeOfBox;
            palletData.palletBoxes.Add(box);
            //for later: update prefab visually
            currentPallet.GetComponent<Pallet>().FillEmptyZonesWithBoxes();
        }
        else
        {
            DetermineBoxType(b.typeOfBox);
            
            

            Vector3 spawningVector = Vector3.up * (boxEndpoint.transform.localScale.y + currentBoxType.transform.localScale.y) / 2f;
            Vector3 coreOffset = new Vector3(-1, 5, 0);


            GameObject spawnedBox = Instantiate(currentBoxType, boxEndpoint.transform.position
            + spawningVector + coreOffset, Quaternion.identity); //spawn box at trigger

            Box spawningBoxData = spawnedBox.GetComponent<Box>();
            if (spawningBoxData == null)
            {
                spawningBoxData = spawnedBox.AddComponent<Box>();
            }

            Rigidbody rb = spawnedBox.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = new Vector3(-1, 0.5f, 0).normalized;

                rb.AddForce(launchDirection * 50f, ForceMode.Impulse);
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);

                Vector3 randomTorque = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized * 3f;

                rb.AddTorque(randomTorque, ForceMode.Impulse);
            }
            // for later: play error noise? show error feedback?
            currentPallet.GetComponent<Pallet>().FillEmptyZonesWithBoxes();
        }
        StartCoroutine(DelayBoxProcessing());
        flag = false;
    }

    void DetermineBoxType(string type)
    {
        switch (type)
        {
            case "A":
                currentBoxType = boxPrefabA;
                break;
            case "B":
                currentBoxType = boxPrefabB;
                break;
            case "C":
                currentBoxType = boxPrefabC;
                break;
            default:
                Debug.Log("Incorrect Type Detected: Swapping from " + type + " to A.");
                currentBoxType = boxPrefabA;
                break;
        }
    }

    public void UnloadBoxFromPallet()
    {
        if (!currentPallet || !palletData || flag) return;

        flag = true;

        BoxData boxToSpawn = palletData.palletBoxes[0];
        DetermineBoxType(boxToSpawn.typeOfBox);
        palletData.palletBoxes.RemoveAt(0);
        
        GameObject spawnedBox = Instantiate(currentBoxType, boxEndpoint.transform.position 
            + Vector3.up * (boxEndpoint.transform.localScale.y + currentBoxType.transform.localScale.y) / 2f, Quaternion.identity); //spawn box at trigger
        Box spawningBoxData = spawnedBox.GetComponent<Box>();
        if (spawningBoxData == null)
        {
            spawningBoxData = spawnedBox.AddComponent<Box>();
        }

        spawningBoxData.typeOfBox = boxToSpawn.typeOfBox;

        if (palletData.palletBoxes.Count <= 0)
        {
            Destroy(currentPallet);
            RemovePallet();
        } 
        
        StartCoroutine(DelayBoxProcessing()); //delays so all of the boxes dont immediately expel themselves from the pallet
        flag = false;
    }

    IEnumerator DelayBoxProcessing()
    {
        yield return new WaitForSecondsRealtime(1);
    }

    void SpawnPallet()
    {
        currentPallet = Instantiate(palletPrefab, 
            palletEndpoint.transform.position + Vector3.up * (palletEndpoint.transform.localScale.y + palletPrefab.transform.localScale.y) / 2f, 
            Quaternion.identity);
        palletData = currentPallet.GetComponent<Pallet>();
        hasPallet = true;
    }

    public void LoadPallet(GameObject pallet)
    {
        currentPallet = pallet;
        palletData = pallet.GetComponent<Pallet>();
        hasPallet = true;
    }

    public void RemovePallet()
    {
        currentPallet = null;
        palletData = null;
        hasPallet = false;
    }

    private void Start()
    {
        StartCoroutine(UnloadRoutine());
    }

    IEnumerator UnloadRoutine()
    {
        while (true)
        {
            if (!isLoading && hasPallet && palletData.palletBoxes.Count > 0)
            {
                UnloadBoxFromPallet();
            }

            yield return new WaitForSeconds(1);

        }
    }
}

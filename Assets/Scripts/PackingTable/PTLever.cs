using UnityEngine;

public class PTLever : MonoBehaviour, Interactable
{

    [SerializeField]
    GameObject ptCore;

    [SerializeField]
    Material loadingMat;

    [SerializeField]
    Material unloadingMat;

    private Renderer objR;
    private PTController ptc;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objR = GetComponent<Renderer>();
        ptc = ptCore.GetComponent<PTController>();
    }

    public void Interact(GameObject interactor)
    {
        SFXController.Instance.PlayClip(SFXController.Instance.doorClicked);
        ptc.isLoading = !ptc.isLoading;

        if (ptc.isLoading)
        {
            objR.material = loadingMat;
        } else
        {
            objR.material = unloadingMat;
        }

    }
}

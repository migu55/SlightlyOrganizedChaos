using System.Collections;
using UnityEngine;

public class Box : MonoBehaviour
{
    // Type of box
    public string typeOfBox;

    // amount of boxes in a pallet
    public int amtOfBox;


    private bool audioTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {

        if (!audioTriggered && gameObject.tag == "Box" && collision.gameObject.tag != "Pallet")
        {
            SFXController.Instance.PlayClip(SFXController.Instance.boxCollision, true);
            audioTriggered = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        audioTriggered = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (gameObject.tag == "Box")
        StartCoroutine(ResetTrigger());
    }

    IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(1f);
        audioTriggered = false;
    }
}

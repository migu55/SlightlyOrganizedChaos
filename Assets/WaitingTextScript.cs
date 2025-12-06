using TMPro;
using UnityEngine;

public class WaitingTextScript : MonoBehaviour
{

    [SerializeField]
    GameObject waitingText;
    [SerializeField]
    GameObject missionList;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waitingText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStats.Instance.gameStarted && GameStats.Instance.gameTime < 300 && GameStats.Instance.gameTime > 90)
        {
            if (missionList.transform.childCount > 0)
            {
                waitingText.SetActive(false);
            }
            else
            {
                waitingText.SetActive(true);
            }
        }
        
    }
}

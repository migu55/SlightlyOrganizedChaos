using TMPro;
using UnityEngine;

public class WaitingTextScript : MonoBehaviour
{

    [SerializeField]
    GameObject waitingText;
    [SerializeField]
    GameObject missionList;

    bool showText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waitingText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        showText = true;

        if (!GameStats.Instance.allPlayersReady)
        {
            showText = false;
        }

        if (GameStats.Instance.inRoundScreen)
        {
            showText = false;
        }

        if (missionList.transform.childCount > 0)
        {
            showText = false;
        }

        if (GameStats.Instance.gameTime > 299 || GameStats.Instance.gameTime < 90)
        {
            showText = false;
        }

        waitingText.SetActive(showText);

    }
}

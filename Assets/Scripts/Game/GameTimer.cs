using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject roundHandlerGO;
    private RoundGameToResults roundHandler;

    // Update is called once per frame
    private void Awake()
    {
        roundHandler = roundHandlerGO.GetComponent<RoundGameToResults>();
    }

    void Update()
    {

        if (GameStats.Instance.gameTime > 0 && GameStats.Instance.allPlayersReady)
        {
            GameStats.Instance.gameTime -= Time.deltaTime;
        }
        else if (GameStats.Instance.gameTime < 0)
        {
            GameStats.Instance.gameTime = 0;
        }

        if (GameStats.Instance.gameTime < 31)
        {
            timerText.color = Color.red;
        } else
        {
            timerText.color = Color.white;
        }

        if (GameStats.Instance.gameTime == 0 && !GameStats.Instance.inRoundScreen)
        {
            roundHandler.TransitionToResults();
            GameStats.Instance.inRoundScreen = true;
        }

        
        int min = Mathf.FloorToInt(GameStats.Instance.gameTime / 60);
        int sec = Mathf.FloorToInt(GameStats.Instance.gameTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", min, sec);
    }
}

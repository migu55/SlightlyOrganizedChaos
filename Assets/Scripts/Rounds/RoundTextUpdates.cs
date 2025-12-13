using TMPro;
using UnityEngine;

public class RoundTextUpdates : MonoBehaviour //For post round stats
{
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] TextMeshProUGUI totalText;
    [SerializeField] TextMeshProUGUI passText;
    [SerializeField] TextMeshProUGUI failText;
    [SerializeField] TextMeshProUGUI budgetText;
    [SerializeField] TextMeshProUGUI profitText;

    // Update is called once per frame
    void Update()
    {
        dayText.text = "Day " + GameStats.Instance.gameRound;
        totalText.text = "Total Orders: " + GameStats.Instance.roundNumMissions;
        passText.text = "Orders Completed: " + GameStats.Instance.roundMissionPasses;
        failText.text = "Orders Failed: " + (GameStats.Instance.roundNumMissions - GameStats.Instance.roundMissionPasses);
        budgetText.text = "Current Budget: " + GameStats.Instance.gameBalance;
        int difference = GameStats.Instance.gameBalance - GameStats.Instance.gamePreviousRoundBalance;
        profitText.text = "Profit: " + (difference >= 0 ? "+ " : "- ") + difference;
    }
}

using TMPro;
using UnityEngine;

public class GameBalance : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI balanceText;

    // Update is called once per frame
    void Update()
    {
        if (GameStats.Instance.gameBalance < 0)
        {
            balanceText.color = Color.red;
        } else
        {
            balanceText.color = Color.white;
        }


        balanceText.text = "$" + GameStats.Instance.gameBalance + " / $" + GameStats.Instance.gameQuota;
    }
}

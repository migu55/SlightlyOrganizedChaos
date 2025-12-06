using TMPro;
using UnityEngine;

public class GameBalance : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI balanceText;

    // Update is called once per frame
    void Update()
    {
        balanceText.text = "$" + GameStats.Instance.gameBalance + " / $" + GameStats.Instance.gameQuota;
    }
}

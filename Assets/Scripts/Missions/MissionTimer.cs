using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Image clock;
    [SerializeField] Image timerBar;
    [SerializeField] TextMeshProUGUI boxAText;
    [SerializeField] TextMeshProUGUI boxBText;
    [SerializeField] TextMeshProUGUI boxCText;
    public int mID = -1;
    public float maxTime = 30f;
    public float time = 30f;
    public int[] current = { 0, 0, 0 };
    public int[] mQuotas = { 0, 0, 0 };
    public bool flag;

    private DoorFail doorFailScript;
    //find instance

    private void Awake()
    {
        flag = false;
        doorFailScript = FindObjectOfType<DoorFail>();
    }

    public void UpdateQuotas(int[] incoming)
    {
        for (int i = 0; i < current.Length; i++)
        {
            current[i] += incoming[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 0 && !flag)
        {
            time -= Time.deltaTime;
            timerBar.fillAmount = Mathf.Lerp(timerBar.fillAmount, time / maxTime, Time.deltaTime);
            Color timerColor = Color.Lerp(Color.red, Color.green, time / maxTime);
            timerBar.color = timerColor;
        }
        else if (time < 0)
        {
            time = 0;
            timerBar.fillAmount = 0;
        }

        if (time < 31)
        {
            clock.color = Color.red;
            timerText.color = Color.red;
        }

        if (time == 0 && !flag)
        {
            if(doorFailScript.CloseDoorForMission(mID) == -1)
            {
                doorFailScript.CancelQueuedMissionAndNotify(mID);
            }
            flag = true;
        }

        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", min, sec);

        boxAText.text = current[0] + " / " + mQuotas[0];
        boxBText.text = current[1] + " / " + mQuotas[1];
        boxCText.text = current[2] + " / " + mQuotas[2];

    }
}

using System.Linq;
using UnityEngine;
public class MissionData
{
    public int id;
    public int[] MissionQuantities;
    public float time;
    public int reward;
    private int boxRangeIncrement = 3;

    public MissionData()
    {
        this.id = 0;

        int[] test = new int[3] { 
            Random.Range(GameStats.Instance.gameRound - boxRangeIncrement, GameStats.Instance.gameRound + boxRangeIncrement),
            Random.Range(GameStats.Instance.gameRound - boxRangeIncrement, GameStats.Instance.gameRound + boxRangeIncrement),
            Random.Range(GameStats.Instance.gameRound - boxRangeIncrement, GameStats.Instance.gameRound + boxRangeIncrement) };

        bool hasBox = false;
        for (int i = 0; i < test.Length; i++)
        {
            if (test[i] > 0)
            {
                hasBox = true;
            }

            if (test[i] < 0)
            {
                test[i] = 1;
            }
        }
        if (!hasBox)
        {
            test[0] = 1;
        }

        this.MissionQuantities = test;
        this.time = (60 + (MissionQuantities.Sum() * 5)); //1 min + quantity boost

        if (GameStats.Instance.gameRound == 1)
        {
            this.time += 30; //Extra 30 seconds for orders on the first round
        }

        this.reward = 500 + (MissionQuantities.Sum() * 20); //500 dollars + quantity boost
    }
}
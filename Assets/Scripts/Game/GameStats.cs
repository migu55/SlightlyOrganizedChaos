using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance;

    public float gameTime = 360;
    public int gameBalance = 500;
    public int gameQuota = 1500;
    public int gameRound = 1;

    public int roundNumMissions = 0;
    public int roundMissionPasses = 0;
    public int roundMissionFails = 0;

    public int gamePreviousRoundBalance = 500;

    public bool inRoundScreen = false;
    public bool allPlayersReady = false;
    public bool gameStarted = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClockInHandler : MonoBehaviour, Interactable
{
    public List<bool> playersReady;
    public bool tutorialEnabled;

    [SerializeField]
    private PlayerInputManager manager;
    [SerializeField]
    private MissionBehavior missionsController;
    [SerializeField]
    private List<Transform> spawnLocations;
    [SerializeField]
    private PlayerTracker tracker;
    [SerializeField]
    private TextBoxShowHide tutorial;

    private List<int> playerIndexes;
    private List<TextMeshPro> readyTexts;
    private MissionData mData;
    private List<MountForklift> forklifts;

    [SerializeField]
    GameObject roundHandlerGO;
    RoundGameToResults rgtr;

    [SerializeField]
    GameObject priceHandlerGO;
    private NoahPricesHandler ph;

    void Start()
    {
        StartCoroutine(SetupTutorial());
    }

    IEnumerator SetupTutorial()
    {
        yield return new WaitUntil(() => GameStats.Instance != null);

        mData = new()
        {
            id = 0,
            reward = 1000,
            time = 60000,
            MissionQuantities = new int[3] { Random.Range(1, 3), Random.Range(1, 3), Random.Range(1, 3) }
        };

        GameStats.Instance.gameBalance = 999999;
        GameStats.Instance.gameQuota = 0;
        GameStats.Instance.gameRound = 0;
        GameStats.Instance.gameTime = 60000;

        missionsController.createMission(mData);
    }

    private void Awake()
    {
        tutorialEnabled = true;
        playersReady = new();
        playerIndexes = new();
        forklifts = new();
        readyTexts = new();

        rgtr = roundHandlerGO.GetComponent<RoundGameToResults>();
        ph = priceHandlerGO.GetComponent<NoahPricesHandler>();

        StartCoroutine(SetupTutorial());
    }

    public void AddNewPlayer(GameObject player)
    {
        Debug.Log($"Detected player {player.name}, adding to lists...");
        PlayerInput input = player.GetComponent<PlayerInput>();
        if (GameStats.Instance.allPlayersReady)
        {
            playersReady.Add(true);
        } else
        {
            playersReady.Add(false);
        }
        playerIndexes.Add(input.playerIndex);
        Debug.Log($"Player {input.name} Joined: GameObject = ${input.gameObject.name}.\nAdded to index {playerIndexes.IndexOf(input.playerIndex)} of playerIndexes");
    }

    public void Interact(GameObject interactor)
    {
        PlayerInput player = interactor.GetComponent<PlayerInput>();
        int index = playerIndexes.IndexOf(player.playerIndex);
        if (tutorialEnabled) {
            playersReady[index] = !playersReady[index];
            StopAllCoroutines();
            StartCoroutine(ReadyUnreadyCoroutine(index));
        }
        if (playersReady[index])
        {
            Debug.Log("Player Ready");
        } else
        {
            Debug.Log("Player Unready");
        }
    }

    public void UnreadyAll()
    {
        if (playersReady != null && playersReady.Count > 0)
        {
            for (int i = 0; i < playersReady.Count; i++)
            {
                playersReady[i] = false;
            }
        }
        GameStats.Instance.allPlayersReady = false;
    }

    void Update()
    {
        var allForklifts = FindObjectsByType<MountForklift>(sortMode: FindObjectsSortMode.None);
        foreach (MountForklift f in allForklifts)
        {
            if (!forklifts.Contains(f))
            {
                forklifts.Add(f);
            }
        }
        
        var allReadyTexts = GameObject.FindGameObjectsWithTag("ReadyUnready");
        foreach (GameObject t in allReadyTexts)
        {
            if (!readyTexts.Contains(t.GetComponent<TextMeshPro>()))
            {
                Debug.Log($"Found text mesh {t.name}, adding to list...");
                readyTexts.Add(t.GetComponent<TextMeshPro>());
            }
        }

        tutorial = tutorial != null ? tutorial : FindFirstObjectByType<TextBoxShowHide>();

        
        bool ready = true;
        foreach (bool r in playersReady)
        {
            if (!r) ready = false;
        }
        if (playersReady.Count == 0) ready = false;
        
        if (ready && !GameStats.Instance.allPlayersReady && tutorial != null)
        {
            AllPlayersReady();
        }
        if (!ready && tutorial != null)
        {
            PlayerUnready();
        }
    }

    private void AllPlayersReady()
    {
        tutorialEnabled = false;
        GameStats.Instance.allPlayersReady = true;
        SFXController.Instance.PlayClip(SFXController.Instance.clockInWhistle);
        ph.ResetRoundTimer();
        rgtr.RoundStatusPreRound();

        missionsController.removeMission(0);
        
        foreach (MountForklift f in forklifts)
        {
            f.Dismount();
        }
        
        for (int i = 0; i < tracker.players.Count; i++)
        {
            tracker.players[i].transform.SetPositionAndRotation(spawnLocations[i].position, spawnLocations[i].rotation);
        }

        tutorial.HideTutorialBoxes();
    }

    private void PlayerUnready()
    {
        tutorial.ShowTutorialBoxes();
    }

    public void ResetTutorial()
    {
        UnreadyAll();
        tutorialEnabled = true;
        GameStats.Instance.allPlayersReady = false;
    }

    private List<float> i = new();
    void FixedUpdate()
    {
        if (i.Count < playerIndexes.Count) {
            i.Clear();
            for (int a = 0; a < playerIndexes.Count; a++)
            {
                i.Add(0);
            }
        }
        for (int a = 0; a < readyTexts.Count; a++)
        {
            if (readyTexts[a].text != "")
            {
                i[a] += Time.fixedDeltaTime;
            } else
            {
                i[a] = 0;
            }
            if (i[a] >= 3.5)
            {
                readyTexts[a].text = "";
            }
        }
    }

    private IEnumerator ReadyUnreadyCoroutine(int index)
    {
        var textObj = readyTexts[index];
        if (playersReady[index])
        {
            SFXController.Instance.PlayClip(SFXController.Instance.playerReady);
            textObj.text = "Ready";
            textObj.color = Color.green;
        } else
        {
            SFXController.Instance.PlayClip(SFXController.Instance.playerUnready);
            textObj.text = "Unready";
            textObj.color = Color.red;
        }

        Vector3 target = new(0, 5, 0);
        float elapsed = 0f;
        Vector3 start = new(0, 0, 0);
        textObj.transform.position = start;

        yield return new WaitForSeconds(0.1f);
        
        while (elapsed < 2f)
        {
            float t = elapsed/2;

            textObj.transform.localPosition = Vector3.Lerp(start, target, t);
            elapsed += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        textObj.transform.localPosition = target;
        yield return new WaitForSeconds(1);
        textObj.text = "";
    }
}

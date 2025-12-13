using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClockInHandler : MonoBehaviour, Interactable
{
    // List of true/false of players ready
    public List<bool> playersReady;
    // Whether the tutorial mode is enabled
    public bool tutorialEnabled;
    // Whether the tutorial we are in is the first (it's the only one that players can place orders in)
    public bool firstTutorial = true;

    [SerializeField]
    // Reference to the mission controller system, used for adding the tutorial mission for the players to practise,
    // and to then remove it when the tutorial is disabled
    private MissionBehavior missionsController;
    [SerializeField]
    // Reference to the player tracker, used for setting players to their spawn points when a round starts
    private PlayerTracker tracker;
    [SerializeField]
    // Reference to the tutorial text box show/hide system, used to show or hide the tutorial text boxes at the appropriate times
    private TextBoxShowHide tutorial;

    // List of player indexes within the PlayerInput system
    private List<int> playerIndexes;
    // List of all player ready text boxes
    private List<TextMeshPro> readyTexts;
    // Mission data used for tutorial mission
    private MissionData mData;
    // List of all forklifts, used to call dismount on when all players have been readied up
    private List<MountForklift> forklifts;

    [SerializeField]
    // Reference to the round handler object, used for getting reference to the results system
    GameObject roundHandlerGO;
    // Reference to the round results
    RoundGameToResults rgtr;

    [SerializeField]
    // Reference to the price handler object, used for getting reference to the price handler system
    GameObject priceHandlerGO;
    // Reference to the price handler
    private NoahPricesHandler ph;

    // Sets up the tutorial with the pre-round music, and the practise mission with extended duration
    IEnumerator SetupTutorial()
    {
        yield return new WaitUntil(() => GameStats.Instance != null);

        MusicManager.Instance.PlayMusic(MusicManager.Instance.preroundMusic, 0f);

        // Mission data for the tutorial mission, over the top and very lenient
        mData = new()
        {
            id = 0,
            reward = 1000000,
            time = 60000,
            MissionQuantities = new int[3] { Random.Range(1, 3), Random.Range(1, 3), Random.Range(1, 3) }
        };

        // Sets the available balance to a very large number, and the required amount to 0, and plenty of time so
        // players have the freedom to learn and explore
        GameStats.Instance.gameBalance = 999999;
        GameStats.Instance.gameQuota = 0;
        GameStats.Instance.gameRound = 0;
        GameStats.Instance.gameTime = 60000;

        missionsController.createMission(mData);
    }

    // Initializes start variables, and starts the tutorial setup coroutine
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

    // Adds a new player to the list of players
    public void AddNewPlayer(GameObject player)
    {
        PlayerInput input = player.GetComponent<PlayerInput>();

        // If the game has already started, initialize this player as ready
        if (GameStats.Instance.allPlayersReady)
        {
            playersReady.Add(true);
        } else
        {
            playersReady.Add(false);
        }

        // Find their index in the player input system and add it to the list
        playerIndexes.Add(input.playerIndex);
    }

    // Implementation of the Interactable interface, sets the player that interacted with it to ready/unready
    public void Interact(GameObject interactor)
    {
        PlayerInput player = interactor.GetComponent<PlayerInput>();
        int index = playerIndexes.IndexOf(player.playerIndex);

        // Only allows them to ready/unready if the game hasn't started yet
        if (tutorialEnabled) {
            playersReady[index] = !playersReady[index];
            StopAllCoroutines();
            StartCoroutine(ReadyUnreadyCoroutine(index));
        }
    }

    // Unreadies all players
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

    // Ensures all forklifts, ready texts, tutorial text box controller, and player ready states are tracked
    void Update()
    {
        // Adds any missing forklifts to the list from the world, since some are added at runtime
        var allForklifts = FindObjectsByType<MountForklift>(sortMode: FindObjectsSortMode.None);
        foreach (MountForklift f in allForklifts)
        {
            if (!forklifts.Contains(f))
            {
                forklifts.Add(f);
            }
        }
        
        // Adds any new ready texts that have been instantiated by a player spawning in
        var allReadyTexts = GameObject.FindGameObjectsWithTag("ReadyUnready");
        foreach (GameObject t in allReadyTexts)
        {
            if (!readyTexts.Contains(t.GetComponent<TextMeshPro>()))
            {
                readyTexts.Add(t.GetComponent<TextMeshPro>());
            }
        }

        // Ensures the tutorial text box controller exists and has a reference
        tutorial = tutorial != null ? tutorial : FindFirstObjectByType<TextBoxShowHide>();

        // If any players are not ready, cancel the AllPlayersReady() call
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

    // When all players are ready, disable the tutorial mode and start the next round
    private void AllPlayersReady()
    {
        tutorialEnabled = false;
        firstTutorial = false;
        GameStats.Instance.allPlayersReady = true;
        SFXController.Instance.PlayClip(SFXController.Instance.clockInWhistle);
        ph.ResetRoundTimer();
        rgtr.RoundStatusPreRound();

        // Removes the mission from the tutorial, and also helps to ensure that between rounds are clear
        missionsController.removeMission(0);
        
        // Forces forklift dismount on all players in them
        foreach (MountForklift f in forklifts)
        {
            f.Dismount();
        }
        
        // Positions all players on their spawn points
        for (int i = 0; i < tracker.players.Count; i++)
        {
            tracker.SetPlayerToSpawn(tracker.players[i]);
        }

        // Hides all tutorial boxes
        tutorial.HideTutorialBoxes();
    }

    // If any player is unready during the tutorial mode, ensure the tutorial text boxes are being displayed
    private void PlayerUnready()
    {
        tutorial.ShowTutorialBoxes();
    }

    // Re-enables the tutorial mode
    public void ResetTutorial()
    {
        UnreadyAll();
        tutorialEnabled = true;
        GameStats.Instance.allPlayersReady = false;
    }

    // List of timers used for auto disappearing the ready texts if they persist for too long
    private List<float> autoClearTimers = new();

    // Used just for incrementing the list of auto clearing timers, and doing the clearing
    void FixedUpdate()
    {
        if (autoClearTimers.Count < playerIndexes.Count) {
            autoClearTimers.Clear();
            for (int a = 0; a < playerIndexes.Count; a++)
            {
                autoClearTimers.Add(0);
            }
        }
        for (int a = 0; a < readyTexts.Count; a++)
        {
            if (readyTexts[a].text != "")
            {
                autoClearTimers[a] += Time.fixedDeltaTime;
            } else
            {
                autoClearTimers[a] = 0;
            }
            if (autoClearTimers[a] >= 3.5)
            {
                readyTexts[a].text = "";
            }
        }
    }

    // Sets the text strings and colours, then moves the text in the floaty animation
    private IEnumerator ReadyUnreadyCoroutine(int index)
    {
        // Text object reference for the player being called on
        var textObj = readyTexts[index];

        // If that player has just readied up, display as such and set the colour to green
        if (playersReady[index])
        {
            SFXController.Instance.PlayClip(SFXController.Instance.playerReady);
            textObj.text = "Ready";
            textObj.color = Color.green;
        }
        // Otherwise display unready and set the colour to red
        else
        {
            SFXController.Instance.PlayClip(SFXController.Instance.playerUnready);
            textObj.text = "Unready";
            textObj.color = Color.red;
        }

        // Sets the target position, time elapsed so far, and start position. Start the object at the start position
        Vector3 target = new(0, 5, 0);
        float elapsed = 0f;
        Vector3 start = new(0, 0, 0);
        textObj.transform.position = start;

        // Wait just a moment before continuing
        yield return new WaitForSeconds(0.1f);
        
        // While the time passed has not surpassed 2 seconds, move the text box by the distance needed to reach the target
        // after 2 seconds have passed
        while (elapsed < 2f)
        {
            float t = elapsed/2;

            // Move the text object a little bit further, and increment the elapsed time
            textObj.transform.localPosition = Vector3.Lerp(start, target, t);
            elapsed += Time.fixedDeltaTime;

            // Wait for the next FixedUpdate
            yield return new WaitForFixedUpdate();
        }

        // Ensure the text box ends at the target position every time, wait a moment, then hide the text
        textObj.transform.localPosition = target;
        yield return new WaitForSeconds(1);
        textObj.text = "";
    }
}

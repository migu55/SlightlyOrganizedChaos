using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundGameToResults : MonoBehaviour
{

    [SerializeField]
    GameObject roundStatusText;
    TextMeshProUGUI rst;
    [SerializeField]
    GameObject statusBar;
    [SerializeField]
    GameObject whiteScreenParent;
    Image whiteScreen;
    [SerializeField]
    GameObject roundResultsCanvas;
    [SerializeField]
    GameObject roundResultsCamera;
    [SerializeField]
    GameObject nextRoundText;
    [SerializeField]
    GameObject clockInGO;
    ClockInHandler clockInHandler;
    private DoorFail doorFailScript;
    private TruckSpawnerManager truckSpawnerManagerScript;

    public GameObject prefabA, prefabB, prefabC;
    public GameObject leftBox, rightBox;
    public Vector3 leftR, rightR;
    private bool flag = false;

    private bool inputDetected = true;

    private void Awake()
    {
        whiteScreen = whiteScreenParent.GetComponent<Image>();
        clockInHandler = clockInGO.GetComponent<ClockInHandler>();
        rst = roundStatusText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        StartCoroutine(RotationManager());
    }

    public void TransitionToResults()
    {
        StartCoroutine(TransitionToResultsCoroutine());
    }

    IEnumerator TransitionToResultsCoroutine()
    {
        roundStatusText.SetActive(true);
        rst.text = "Round\nOver!";
        //SFX.Play(Whistle)
        MusicManager.Instance.StopMusic();
        List<MountForklift> forklifts = FindObjectsOfType<MountForklift>().ToList();
        foreach (MountForklift f in forklifts)
        {
            f.Dismount();
        }
        List<GameObject> players = FindObjectsOfType<PlayerController>().Select(x => x.gameObject).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].GetComponent<PlayerController>().enabled = false;
        }
        statusBar.SetActive(false);
        yield return new WaitForSeconds(1);
        //whiteScreenParent.SetActive(true);
        SetAlpha(0);
        yield return StartCoroutine(FadeScreen(true));
        SpawnBoxes();
        roundStatusText.SetActive(false);
        roundResultsCanvas.SetActive(true);
        roundResultsCamera.SetActive(true);
        nextRoundText.SetActive(false);
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = new(55 + (i * 5), 1, -10);
        }
        yield return StartCoroutine(FadeScreen(false));
        yield return new WaitForSeconds(1);
        nextRoundText.SetActive(true);
        inputDetected = false;
    }

    void SpawnBoxes()
    {
        GameObject[] boxes = { prefabA, prefabB, prefabC };
        int random = UnityEngine.Random.Range(1, 3);
        int random2 = UnityEngine.Random.Range(1, 3);

        leftBox = Instantiate(boxes[random], new Vector3(-12.5f, -116, -1), Quaternion.identity);
        rightBox = Instantiate(boxes[random2], new Vector3(12.5f, -116, -1), Quaternion.identity);

        leftBox.GetComponent<Rigidbody>().isKinematic = true;
        rightBox.GetComponent<Rigidbody>().isKinematic = true;
    }

    void DestroyBoxes()
    {
        Destroy(leftBox);
        Destroy(rightBox);
    }

    void SetAlpha(int alpha)
    {
        Color c = whiteScreen.color;
        c.a = alpha;
        whiteScreen.color = c;
    }

    IEnumerator FadeScreen(bool fadeToWhite)
    {
        Color c = whiteScreen.color;
        float startAlpha = c.a;
        float time = 0f;

        while (time < 2f)
        {
            float t = time / 2f;
            c.a = Mathf.Lerp(startAlpha, fadeToWhite ? 1 : 0, t);
            whiteScreen.color = c;

            time += Time.deltaTime;
            yield return null;
        }

        c.a = fadeToWhite ? 1 : 0;
        whiteScreen.color = c;
    }

    public void TransitionToGame()
    {
        StartCoroutine(TransitionToGameCoroutine());
    }

    IEnumerator TransitionToGameCoroutine()
    {
        yield return StartCoroutine(FadeScreen(true));

        if (GameStats.Instance.gameBalance < GameStats.Instance.gameQuota)
        {
            GameStats.Instance.inRoundScreen = false;
            GameStats.Instance.gameStarted = false;
            SceneManager.LoadScene("MainMenu");
        }

        DestroyBoxes();

        GameStats.Instance.gamePreviousRoundBalance = GameStats.Instance.gameBalance;
        GameStats.Instance.gameTime = 360;
        GameStats.Instance.gameRound++;

        GameStats.Instance.gameQuota = GameStats.Instance.gameQuota + (int) (1500 * (1 * (0.5 * GameStats.Instance.gameRound))); //needs testing
        GameStats.Instance.roundMissionFails = 0;
        GameStats.Instance.roundMissionPasses = 0;
        GameStats.Instance.roundNumMissions = 0;
        List<GameObject> players = FindObjectsOfType<PlayerController>().Select(x => x.gameObject).ToList();
        clockInHandler.UnreadyAll();
        clockInHandler.tutorialEnabled = true;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].GetComponent<PlayerController>().enabled = true;
        }
        MusicManager.Instance.PlayMusic(MusicManager.Instance.preroundMusic);
        roundResultsCanvas.SetActive(false);
        roundResultsCamera.SetActive(false);
        statusBar.SetActive(true);
        yield return StartCoroutine(FadeScreen(false));
        GameStats.Instance.inRoundScreen = false;
        yield return null;
    }

    public void FirstRound()
    {
        //Clear Everything
        //Boxes
        List<GameObject> boxes = GameObject.FindGameObjectsWithTag("Box").ToList();
        foreach (GameObject box in boxes)
        {
            Destroy(box);
        }
        //Pallets
        List<GameObject> pallets = FindObjectsOfType<Pallet>().Select(x => x.gameObject).ToList();
        foreach (GameObject pallet in pallets)
        {
            Destroy(pallet);
        }
        //Close Doors / Clear Trucks
        truckSpawnerManagerScript = FindObjectOfType<TruckSpawnerManager>();
        truckSpawnerManagerScript.clearQueue();

        doorFailScript = FindObjectOfType<DoorFail>();
        doorFailScript.CloseEverything();
        

        GameStats.Instance.gameStarted = true;
        GameStats.Instance.gamePreviousRoundBalance = 500;
        GameStats.Instance.gameTime = 390;
        GameStats.Instance.gameRound++;
        GameStats.Instance.gameBalance = 500;
        GameStats.Instance.gameQuota = 1500;
        GameStats.Instance.roundMissionFails = 0;
        GameStats.Instance.roundMissionPasses = 0;
        GameStats.Instance.roundNumMissions = 0;
    }


    public void RoundStatusPreRound()
    {
        StartCoroutine(RoundStatusPreRoundCoroutine());
    }

    public IEnumerator RoundStatusPreRoundCoroutine()
    {
        roundStatusText.SetActive(true);
        rst.text = "Shift\nStart!";
        yield return new WaitForSeconds(2f);
        roundStatusText.SetActive(false);
    }

    public void RoundStatusRoundStart()
    {
        StartCoroutine(RoundStatusRoundStartCoroutine());
    }

    public IEnumerator RoundStatusRoundStartCoroutine()
    {
        roundStatusText.SetActive(true);
        rst.text = "Incoming\nOrders!";
        yield return new WaitForSeconds(2f);
        roundStatusText.SetActive(false);
    }

    private void Update()
    {
        if (!inputDetected)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad == null) continue;

                if (gamepad.allControls.Any(control => control is ButtonControl button && button.wasPressedThisFrame))
                {
                    inputDetected = true;
                    TransitionToGame();
                }
            }
        }

        if (GameStats.Instance.gameRound == 0 && GameStats.Instance.allPlayersReady && !GameStats.Instance.gameStarted)
        {
            FirstRound();
        }
    }

    IEnumerator RotationManager()
    {
        while (true)
        {
            // Wait until BOTH boxes are available
            yield return new WaitUntil(() => leftBox != null && rightBox != null);

            // Run smooth rotation until one is destroyed
            yield return StartCoroutine(SmoothRotationsLoop());
        }
    }

    IEnumerator SmoothRotationsLoop()
    {
        while (leftBox != null && rightBox != null)
        {
            Quaternion leftStart = leftBox.transform.rotation;
            Quaternion rightStart = rightBox.transform.rotation;

            Quaternion leftTarget = UnityEngine.Random.rotation;
            Quaternion rightTarget = UnityEngine.Random.rotation;

            float duration = 1.5f;
            float t = 0f;

            while (t < duration)
            {
                // Stop immediately if either box was destroyed
                if (leftBox == null || rightBox == null)
                    yield break;

                t += Time.deltaTime;
                float progress = t / duration;

                leftBox.transform.rotation = Quaternion.Slerp(leftStart, leftTarget, progress);
                rightBox.transform.rotation = Quaternion.Slerp(rightStart, rightTarget, progress);

                yield return null;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MissionBehavior : MonoBehaviour
{
    public GameObject MissionUIPrefab;
    public Transform missionUIParentTransform;
    int requiredAmtOfMissions, randomAmtOfMissions, nextRequiredMissionInterval;
    bool roundActive;
    int currentMissionID = 1;
    List<int> missionIntervals;
    List<MissionData> activeMissions;

    [SerializeField]
    GameObject roundHandlerGO;
    private RoundGameToResults rgtr;

    public TruckSpawnerManager truckSpawnerManager;

    public GameObject spawnMission(MissionData m)
    {
        GameObject baseUI = Instantiate(MissionUIPrefab, missionUIParentTransform, false);

        MissionTimer timeValue = baseUI.GetComponent<MissionTimer>();
        timeValue.mQuotas = m.MissionQuantities;
        timeValue.mID = m.id;
        timeValue.time = m.time;
        timeValue.maxTime = m.time;

        GameObject baseMission = baseUI.transform.GetChild(0).gameObject; // MissionBackground

        string idUI = (m.id > 9 ? m.id.ToString() : "0" + m.id);

        baseMission.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().SetText(idUI);

        for (int i = 0; i < m.MissionQuantities.Count(); i++)
        {
            GameObject quota = baseMission.transform.GetChild(i + 1).gameObject; //get reference to the quota group, +1 to avoid portrait
            if (m.MissionQuantities.ElementAt(i) == 0)
            {
                quota.SetActive(false);
            }
        }

        GameObject timer = baseMission.transform.GetChild(4).gameObject; //get reference to the timer section
        TextMeshProUGUI timeRemaining = timer.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        timeRemaining.SetText("" + (m.time / 60) + " : " + (m.time % 60));
        GameObject reward = baseMission.transform.GetChild(5).gameObject; //get reference to the reward section
        TextMeshProUGUI missionReward = reward.GetComponent<TextMeshProUGUI>();
        missionReward.SetText("+$" + m.reward);


        Mission baseMissionClass = baseUI.AddComponent<Mission>(); //attach mission to UI
        baseMissionClass.id = m.id;
        baseMissionClass.MissionQuantities = m.MissionQuantities;
        baseMissionClass.time = m.time;
        baseMissionClass.reward = m.reward;

        return baseUI;
    }

    public void createMission()
    {
        createMission(null);
    }

    public void createMission(MissionData mDetails) //overload for tutorial mission
    {
        if (mDetails != null && activeMissions.FirstOrDefault(m => m.id == mDetails.id) != null) return;

        MissionData mData;

        if (mDetails == null)
        {
            mData = new();
            mData.id = currentMissionID;
            currentMissionID++;
        } else
        {
            mData = mDetails;
        }

        if (mData.time > GameStats.Instance.gameTime) //prevents time going over the total mission time
        {
            mData.time = GameStats.Instance.gameTime;
        }        

        spawnMission(mData); //missionUI
        activeMissions.Add(mData);
        GameStats.Instance.roundNumMissions++;
        SFXController.Instance.PlayClip(SFXController.Instance.missionSpawned);
        truckSpawnerManager.spawnTruck(mData.id, false); //send mission information to truck
    }

    public void removeMission(int MissionID)
    {
        MissionData m = activeMissions.FirstOrDefault(o => o.id == MissionID);
        if (m != null)
        {
            activeMissions.Remove(m);
        }
 
        GameObject found = FindObjectsOfType<Mission>().FirstOrDefault(o => o.id == MissionID)?.gameObject; //destroy UI element
        if (found != null)
        {
            Destroy(found);
        }

    }

    public void updateMissionQuotaUI(int missionID, int[] qty)
    {
        GameObject found = FindObjectsOfType<Mission>().FirstOrDefault(o => o.id == missionID)?.gameObject;
        if (found != null)
        {
            MissionTimer core = found.GetComponent<MissionTimer>();
            core.UpdateQuotas(qty); //send info to missionUI
        }
    }


    public void receiveMission(int missionID, List<BoxData> submitted)
    {
        receiveMission(missionID, submitted, true);
    }

    public int[] BoxDataToIntArray(List<BoxData> data) //global converter helper
    {
        if (data == null) return null;

        int a = 0, b = 0, c = 0;
        for (int i = 0; i < data.Count; i++)
        {
            switch (data[i].typeOfBox)
            {
                case "A":
                    a++;
                    break;
                case "B":
                    b++;
                    break;
                case "C":
                    c++;
                    break;
                default:
                    break;
            }
        }
        int[] submittedArray = { a, b, c };
        return submittedArray;
    }

    public void receiveMission(int missionID, List<BoxData> submitted, bool playAnimation)
    {
        MissionData mission = activeMissions.FirstOrDefault(i => i.id == missionID);
        if (mission != null)
        {
            int[] submittedArray = BoxDataToIntArray(submitted);

            bool failure = false;
            if (submittedArray != null)
            {
                for (int i = 0; i < submittedArray.Length; i++)
                {
                    if (mission.MissionQuantities[i] > submittedArray[i])
                    {
                        failure = true;
                    }
                }
            }

            if (!failure || submittedArray == null)
            {
                GameStats.Instance.gameBalance += mission.reward;
                GameStats.Instance.roundMissionPasses++;
                SFXController.Instance.PlayClip(SFXController.Instance.missionComplete);
            }
            else
            {
                if (playAnimation)
                {
                    SFXController.Instance.PlayClip(SFXController.Instance.missionFailed);
                }               
            }

            if (playAnimation)
            {
                playCompleteAnimation(missionID, failure);
            }
            else
            {
                removeMission(missionID);
            }
        }
    }

    void playCompleteAnimation(int missionID, bool failure)
    {
        StartCoroutine(completeAnimationCoroutine(missionID, failure));
    }

    IEnumerator completeAnimationCoroutine(int missionID, bool failure)
    {

        GameObject found = FindObjectsOfType<Mission>().FirstOrDefault(o => o.id == missionID)?.gameObject;
        MissionData mission = activeMissions.FirstOrDefault(i => i.id == missionID);

        //clear and then play so that success state doesnt flip
        activeMissions.Remove(mission);

        if (found != null && mission != null)
        {
            GameObject completeBG = found.transform.GetChild(2).gameObject; //Mission Completion BG
            completeBG.SetActive(true);
            Image cBGimage = completeBG.GetComponent<Image>();
            GameObject completeTextTransform = completeBG.transform.GetChild(0).gameObject; //Mission Completion Text
            TextMeshProUGUI completeText = completeTextTransform.GetComponent<TextMeshProUGUI>();

            if (!failure) //succeed quota
            {
                cBGimage.color = Color.green;
                completeText.text = "Order Complete! +$" + mission.reward;
            }
            else //fail quota
            {
                cBGimage.color = Color.red;
                completeText.text = "Order Failed! :(";
            }

            float displayTime = 5f;
            float totalTime = 0f;
            RectTransform r = completeBG.GetComponent<RectTransform>();
            float totalWidth = r.rect.width;
            float totalHeight = r.rect.height;
            RectTransform rText = completeTextTransform.GetComponent<RectTransform>();
            Vector3 startPos = new Vector3(totalWidth, -25, 0);
            Vector3 endPos = new Vector3(-1 * totalWidth, -25, 0);

            // Fix anchors & pivot so localPosition actually moves correctly
            rText.anchorMin = new Vector2(0.5f, 0.5f);
            rText.anchorMax = new Vector2(0.5f, 0.5f);
            rText.pivot = new Vector2(0.5f, 0.5f);


            while (displayTime > totalTime)
            {
                totalTime += Time.deltaTime;
                float lerp = totalTime / displayTime;
                rText.localPosition = Vector3.Lerp(startPos, endPos, lerp);
                yield return null;
            }

            removeMission(missionID);

        }
    }

    public MissionData getMissionWithMissionID(int missionID)
    {
        return activeMissions.FirstOrDefault(m => m.id == missionID);
    }

    void BeginRound()
    {
        roundActive = true;
        rgtr.RoundStatusRoundStart();
        SFXController.Instance.PlayClip(SFXController.Instance.roundStartJingle);
        MusicManager.Instance.PlayMusic(MusicManager.Instance.roundMusic);
        currentMissionID = 1; //set current Mission ID for this round
        requiredAmtOfMissions = (GameStats.Instance.gameRound / 2) + 3; //set required number of Missions for this round
        randomAmtOfMissions = GameStats.Instance.gameRound / 3; //every 3 rounds, add one random Mission

        for (int i = 0; i < requiredAmtOfMissions - 1; i++) //divide required number of Missions equally, buffer is prep phase and 90 before finish
        {
            missionIntervals.Add((300 / requiredAmtOfMissions) * i + 90);
        }

        for (int j = 0; j < randomAmtOfMissions; j++)
        {
            if (Random.value > 0.75) //can adjust later to add more likeliness as the game progresses
            {
                missionIntervals.Add(Random.Range(100, 270));
            }
        }

        missionIntervals.Sort();
        missionIntervals.Reverse();

        nextRequiredMissionInterval = 299;

        StartCoroutine(ActiveRound());
    } 

    IEnumerator EndRound()
    {
        roundActive = false;
        SFXController.Instance.PlayClip(SFXController.Instance.roundEndWhistle);
        yield return new WaitForSeconds(1);
    }

    void Awake()
    {
        activeMissions = new();
        missionIntervals = new();
        rgtr = roundHandlerGO.GetComponent<RoundGameToResults>();
    }

    IEnumerator ActiveRound()
    {
        while (roundActive && GameStats.Instance.gameTime <= 300 && GameStats.Instance.gameTime >= 0)
        {
            if (nextRequiredMissionInterval == Mathf.FloorToInt(GameStats.Instance.gameTime)) //if required Mission not empty and upcomming required Mission matched
            {
                createMission();
                if (missionIntervals.Count > 0)
                {
                    nextRequiredMissionInterval = missionIntervals[0]; //move to next required interval
                    missionIntervals.RemoveAt(0);
                } else
                {
                    nextRequiredMissionInterval = -1; //no more
                }
            }
            yield return null;
        }
        
    }

        
    // Update is called once per frame
    void Update()
    {
        if(truckSpawnerManager == null)
        {
            truckSpawnerManager = FindObjectOfType<TruckSpawnerManager>();
        }

        if (GameStats.Instance.gameTime < 300 && GameStats.Instance.gameTime > 0 && !roundActive)
        {
            BeginRound();
        }

        if (GameStats.Instance.gameTime <= 0 && roundActive)
        {
            StartCoroutine(EndRound());
        }
    }
}


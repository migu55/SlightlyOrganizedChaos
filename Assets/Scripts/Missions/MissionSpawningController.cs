using System.Collections.Generic;
using UnityEngine;

public class MissionSpawningController : MonoBehaviour
{

    public List<GameObject> missions;
    public Transform missionSpawnLocation;

    // Update is called once per frame
    void Update()
    {
        if (missions.Count > 0)
        {

            for (int i = 0; i < missions.Count; i++)
            {
                GameObject newMission = Instantiate(missions[i]);


                newMission.transform.SetParent(missionSpawnLocation, false);


                RectTransform rt = newMission.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                }

            }
            missions.Clear();
        }
    }
}

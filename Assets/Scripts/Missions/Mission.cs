using System.Linq;
using UnityEngine;

public class Mission : MonoBehaviour
{

    public int id;
    public int[] MissionQuantities;
    public float time;
    public int reward;
    
    private void Awake()
    {
        id = 0;
        MissionQuantities = new int[3] { Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10) };
        time = (60 + (MissionQuantities.Sum() * 5)); //1 min + quantity boost
        reward = 500 + (MissionQuantities.Sum() * 20); //500 dollars + quantity boost
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;

public class NoahPricesHandler : MonoBehaviour
{
    private GameObject menuObj;
    private ProductOrderingMenu menu;
    private List<int> newPrices = new();
    private float time = 0;
    private float roundTime = 0;
    private float roundEnd;

    void Start()
    {
        menuObj = GameObject.FindGameObjectWithTag("Order Menu");
        menu = menuObj.GetComponent<ProductOrderingMenu>();
        for (int i = 0; i < 3; i++) { newPrices.Add(UnityEngine.Random.Range(5, 10)); }
        menu.UpdatePrices(newPrices);
    }

    void Update()
    {
        if (GameStats.Instance.gameTime >= roundEnd) roundEnd = GameStats.Instance.gameTime;

        bool menuOpen = false;

        var playerList = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in playerList)
        {
            if (p.GetComponent<NoahOrderHandlerPlayer>().isMenuOpen)
            {
                menuOpen = true;
            }
        }

        if (time >= 30)
        {
            UpdatePricesLocally();
        } else if (!menuOpen)
        {
            time += Time.deltaTime;
        }

        if (roundTime < roundEnd) roundTime += Time.deltaTime;
    }

    public void UpdatePricesLocally()
    {
            // update prices
            time = 0;
            int min, max;
            min = (int)Math.Min(Math.Floor(roundTime / 60) * 20, 60);
            max = (int)Math.Min((Math.Floor(roundTime / 60) * 20) + 20, 100);
            if (min < 5) min = 5;
            for (int i = 0; i < newPrices.Count; i++)
            {
            newPrices[i] = UnityEngine.Random.Range(min, max + 1);
            }
            menu.UpdatePrices(newPrices);
    }

    // Call after the round has been incremented and a new gameTime has been set.
    public void ResetRoundTimer()
    {
        roundTime = 0;
        roundEnd = GameStats.Instance.gameTime;
        UpdatePricesLocally();
    }
}

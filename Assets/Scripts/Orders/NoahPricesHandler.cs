using System;
using System.Collections.Generic;
using UnityEngine;

public class NoahPricesHandler : MonoBehaviour
{
    // ProductOrderingMenu object reference used to get the class reference
    private GameObject menuObj;
    // Class reference to the menu, to call UpdatePrices()
    private ProductOrderingMenu menu;
    // List of newly generated prices to be sent to the order menu
    private List<int> newPrices = new();
    // Used to track when to update the prices; only increases while no player is in the menu
    private float time = 0;
    // Tracks how long the round has been going on for, changes the range of prices
    private float roundTime = 0;
    // Used to set how high roundTime will get to, to balance out the price ranges based on the length of individual rounds
    private float roundEnd = 0;

    // Sets menu references and initializes first prices
    void Start()
    {
        menuObj = GameObject.FindGameObjectWithTag("Order Menu");
        menu = menuObj.GetComponent<ProductOrderingMenu>();
        for (int i = 0; i < 3; i++) { newPrices.Add(UnityEngine.Random.Range(5, 10)); }
        menu.UpdatePrices(newPrices);
    }

    // Updates round duration and menu open status. If unopened time has exceeded 20 seconds, update the prices
    void Update()
    {
        if (GameStats.Instance.gameTime >= roundEnd) roundEnd = GameStats.Instance.gameTime;

        bool menuOpen = false;

        // Uses a list of all player game objects to find if anyone has the menu open
        var playerList = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in playerList)
        {
            if (p.GetComponent<NoahOrderHandlerPlayer>().isMenuOpen)
            {
                menuOpen = true;
            }
        }

        if (time >= 20)
        {
            UpdatePricesLocally();
        } else if (!menuOpen)
        {
            time += Time.deltaTime;
        }

        // Increments round time if the round has started
        if (roundTime < roundEnd && GameStats.Instance.gameTime < 300) roundTime += Time.deltaTime;
    }

    // Updates the prices and sends the new ones to the ordering menu, resetting the time tracker
    public void UpdatePricesLocally()
    {
            // Resets time tracker to 0
            time = 0;

            // Sets min and max price values
            int min, max;
            min = (int)Math.Min(Math.Floor(roundTime / 60) * 20, GameStats.Instance.gameTime < 300 ? 60 : 10);
            max = (int)Math.Min((Math.Floor(roundTime / 60) * 20) + 20, GameStats.Instance.gameTime < 300 ? 100 : 10);
            if (min < 5) min = 5;

            // Updates to new price values
            for (int i = 0; i < newPrices.Count; i++)
            {
                newPrices[i] = UnityEngine.Random.Range(min, max + 1);
            }

            // Sends new prices to the order menu update
            menu.UpdatePrices(newPrices);
    }

    // Resets the round timer to 0 and gets the new round end time, and resets prices to the start of round values
    public void ResetRoundTimer()
    {
        roundTime = 0;
        roundEnd = GameStats.Instance.gameTime;
        UpdatePricesLocally();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI startText;
    //private bool gameStarted = false;
    public float flashSpeed = 0.8f; // How quickly the text flashes

    void Start()
    {
        // Ensure the game doesn't start until a key is pressed
        //startText.gameObject.SetActive(true); // Make sure the message is visible
        startText = GetComponent<TextMeshProUGUI>(); 
        StartCoroutine(BlinkText());
    }

    // void Update()
    // {
    //     if (!gameStarted && Input.anyKeyDown)
    //     {
    //         // Start the game when any key is pressed
    //         gameStarted = true;
    //
    //         // Hide the "Press any button" text
    //         startText.gameObject.SetActive(false);
    //         StartGame();
    //     }
    // }
    
    IEnumerator BlinkText()
    {
        while (true)
        {
            startText.enabled = !startText.enabled; // Toggle visibility
            // Or: flashingText.color = new Color(flashingText.color.r, flashingText.color.g, flashingText.color.b, flashingText.color.a == 1 ? 0 : 1); // Toggle alpha
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    // void StartGame()
    // {
    //     Debug.Log("Starting game. . .");
    //     SceneManager.LoadScene("Default");
    // }
}

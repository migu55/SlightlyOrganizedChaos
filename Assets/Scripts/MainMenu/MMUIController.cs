using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMUIController : MonoBehaviour
{
    bool buttonPressed;

    public GameObject anyButtonScreen, homeScreen, controlsScreen, creditsScreen;

    public GameObject homeFirstButton, controlsFirstButton, creditsFirstButton, homeClosedButton;

    public Sprite playerControlsImg, forkliftControlsImg, UIControlsImg;

    public GameObject imageHolder;
    private Image controlsImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonPressed = false;
        controlsImage = imageHolder.GetComponent<Image>();
        MusicManager.Instance.PlayMusic(MusicManager.Instance.menuMusic, 0f);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Default");
    }

    public void OpenControls()
    {
        homeScreen.SetActive(false);
        controlsScreen.SetActive(true);
        homeClosedButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsFirstButton);
    }

    public void SetPlayerControls()
    {
        controlsImage.sprite = playerControlsImg;
    }
    
    public void SetForkliftControls()
    {
        controlsImage.sprite = forkliftControlsImg;
    }
    
    public void SetUIControls()
    {
        controlsImage.sprite = UIControlsImg;
    }
    
    public void CloseControls()
    {
        controlsScreen.SetActive(false);
        homeScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(homeClosedButton);
    }
    
    public void OpenCredits()
    {
        homeScreen.SetActive(false);
        creditsScreen.SetActive(true);
        homeClosedButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(creditsFirstButton);
    }
    
    public void CloseCredits()
    {
        creditsScreen.SetActive(false);
        homeScreen.SetActive(true);       
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(homeClosedButton);
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (!buttonPressed)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad == null) continue;

                if (gamepad.allControls.Any(control => control is ButtonControl button && button.wasPressedThisFrame))
                {
                    Debug.Log("pressed");

                    buttonPressed = true;
                    anyButtonScreen.SetActive(false);
                    homeScreen.SetActive(true);

                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(homeFirstButton);
                    Debug.Log(EventSystem.current.currentSelectedGameObject);
                }
            }
        }
        
    }
}

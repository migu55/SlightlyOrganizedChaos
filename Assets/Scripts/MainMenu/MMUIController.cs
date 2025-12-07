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

    GameObject lastSelected = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonPressed = false;
        controlsImage = imageHolder.GetComponent<Image>();
        MusicManager.Instance.PlayMusic(MusicManager.Instance.menuMusic, 0f);
    }

    public void PlayGame()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        SceneManager.LoadScene("Default");
    }

    public void OpenControls()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        homeScreen.SetActive(false);
        controlsScreen.SetActive(true);
        homeClosedButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsFirstButton);
    }

    public void SetPlayerControls()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        controlsImage.sprite = playerControlsImg;
    }
    
    public void SetForkliftControls()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        controlsImage.sprite = forkliftControlsImg;
    }
    
    public void SetUIControls()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        controlsImage.sprite = UIControlsImg;
    }
    
    public void CloseControls()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        controlsScreen.SetActive(false);
        homeScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(homeClosedButton);
    }
    
    public void OpenCredits()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        homeScreen.SetActive(false);
        creditsScreen.SetActive(true);
        homeClosedButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(creditsFirstButton);
    }
    
    public void CloseCredits()
    {
        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
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

    private bool IsNavigationInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return false;

        return gamepad.dpad.up.wasPressedThisFrame
            || gamepad.dpad.down.wasPressedThisFrame
            || gamepad.dpad.left.wasPressedThisFrame
            || gamepad.dpad.right.wasPressedThisFrame
            || gamepad.leftStick.up.wasPressedThisFrame
            || gamepad.leftStick.down.wasPressedThisFrame
            || gamepad.leftStick.left.wasPressedThisFrame
            || gamepad.leftStick.right.wasPressedThisFrame;
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
                    SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);

                    buttonPressed = true;
                    anyButtonScreen.SetActive(false);
                    homeScreen.SetActive(true);

                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(homeFirstButton);
                }
            }
        }

        var selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null) return;
        
        if (selected != lastSelected)
        {
            if(IsNavigationInput())
            {
                SFXController.Instance.PlayClip(SFXController.Instance.uiInput, true);
            }
           
        }
      
        lastSelected = selected;
        
    }
}

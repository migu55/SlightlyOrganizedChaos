using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{

    public GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Gather device lists (Input System devices) and log/display them
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Detected input devices:");
        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
            sb.AppendLine($"  [{d.deviceId}] {d.displayName} ({d.layout})");
        Debug.Log(sb.ToString());

        // Pick the first suitable device (Gamepad or Joystick)
        UnityEngine.InputSystem.InputDevice chosenDevice = null;
        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
        {
            if (d is UnityEngine.InputSystem.Gamepad || d is UnityEngine.InputSystem.Joystick)
            {
                chosenDevice = d;
                break;
            }
        }

        if (chosenDevice != null)
        {
            var pim = GetComponent<PlayerInputManager>();
            pim.JoinPlayer(-1, -1 , "Player", chosenDevice);
            Debug.Log($"Joined player with device: {chosenDevice.displayName}");
        }
        else
        {
            Debug.Log("No suitable input device found to join player.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

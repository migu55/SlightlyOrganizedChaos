using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Manages automatic player spawning for multiplayer setup.
/// Detects connected input devices (gamepads/joysticks) and automatically spawns players using PlayerInputManager.
/// Useful for testing multiplayer scenarios without manual player joining.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    // Player prefab to spawn (set in inspector)
    public GameObject playerPrefab;
    
    /// <summary>
    /// Called on first frame. Detects input devices and automatically spawns the first player
    /// with the first available gamepad or joystick device.
    /// </summary>
    void Start()
    {
        // Build and log a list of all detected input devices for debugging
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Detected input devices:");
        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
            sb.AppendLine($"  [{d.deviceId}] {d.displayName} ({d.layout})");

        // Find the first suitable controller device (gamepad or joystick)
        UnityEngine.InputSystem.InputDevice chosenDevice = null;
        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
        {
            if (d is UnityEngine.InputSystem.Gamepad || d is UnityEngine.InputSystem.Joystick)
            {
                chosenDevice = d;
                break;
            }
        }

        // If a suitable device was found, automatically spawn a player with it
        if (chosenDevice != null)
        {
            var pim = GetComponent<PlayerInputManager>();
<<<<<<< HEAD
            
            // Join player: -1 = auto-assign split screen, -1 = auto control scheme, "Player" = action map
            pim.JoinPlayer(-1, -1 , "Player", chosenDevice);
            Debug.Log($"Joined player with device: {chosenDevice.displayName}");
        }
        else
        {
            Debug.Log("No suitable input device found to join player.");
=======
            pim.JoinPlayer(-1, -1 , null, chosenDevice);
>>>>>>> 63de0e21a8ff66ceb7fdc9ebe55d94d4da0ee360
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

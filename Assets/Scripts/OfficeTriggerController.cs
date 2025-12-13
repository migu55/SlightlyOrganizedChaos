using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class OfficeTriggerController : MonoBehaviour
{
    // Canvas object of the laptop and its aim constraint.
    [SerializeField]
    private GameObject canvas;
    private AimConstraint canvasConstraint;
    // Bool that controls when the ParticleSystems should be running.
    private bool blockVision = true;
    // Float used to track how long the ParticleSystem has been running, since the built-in PlaybackTime
    // variable is inaccessible.
    private float emissionDuration = 0;

    // Initializes the canvas constraint from the canvas itself.
    void Start()
    {
        canvasConstraint = canvas.GetComponent<AimConstraint>();
    }

    // Handles when a player enters the trigger area, adds them to the list of AimConstraint sources.
    public void AddToAimConstraint(GameObject other)
    {
        // Find the camera of the player and set the aim constraint target to that
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            ConstraintSource cameraSource = new() { sourceTransform = player.GetCamera().transform, weight = 1 };
            canvasConstraint.AddSource(cameraSource);
            blockVision = false;
        }

        // Whenever a player enters the office, ensure that the canvas and its children are all visible.
        canvas.SetActive(true);
        Renderer[] renderers = canvas.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = true;
        }
    }

    // Handles when a player leaves the trigger area, removes them from the list and shrinks the list if they were
    // the player at the end of the list.
    public void RemoveFromAimConstraint(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            ConstraintSource cameraSource = new() { sourceTransform = player.GetCamera().transform, weight = 1 };
            for (int i = 0; i < canvasConstraint.sourceCount; i++)
            {
                if (canvasConstraint.GetSource(i).Equals(cameraSource))
                {
                    canvasConstraint.RemoveSource(i);
                }
            }
        }

        // If all players have left, then hide the canvas
        bool allPlayersLeft = true;
        ConstraintSource empty = new() { sourceTransform = null, weight = 0 };
        for (int i = 0; i < canvasConstraint.sourceCount; i++)
        {
            if (canvasConstraint.GetSource(i).Equals(empty)) {}
            // If even a single player is in the office, this forces the first code block below to execute,
            // rather than the one below it.
            else
            {
                allPlayersLeft = false;
            }
        }

        // Array of all renderers in the canvas
        Renderer[] renderers = canvas.GetComponentsInChildren<Renderer>();

        // When all players leave then set the canvas and its children to be invisible
        if (allPlayersLeft)
        {
            canvas.SetActive(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
            blockVision = true;
        }
        // Otherwise ensure the canvas and its children are visible
        else
        {
            canvas.SetActive(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }
            blockVision = false;
        }
    }
}

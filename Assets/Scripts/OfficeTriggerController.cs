using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class OfficeTriggerController : MonoBehaviour
{
    // List of the ParticleSystems that obscure player vision of the truck areas.
    [SerializeField]
    private List<ParticleSystem> barriers;
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
        var psList = GameObject.FindGameObjectsWithTag("VisionBlocker");
        barriers.Clear();
        foreach (GameObject ps in psList) { barriers.Add(ps.GetComponent<ParticleSystem>()); }
        foreach (ParticleSystem system in barriers) { system.Play(); }
    }

    void FixedUpdate() // Now works correctly, should try refactoring if time is available to use a function instead?
    {
        if (emissionDuration < 4 && blockVision) emissionDuration += Time.fixedDeltaTime;
        for (int i = 0; i < barriers.Count; i++)
        {
            if (barriers[i].isStopped && blockVision)
            {
                barriers[i].Play();
                emissionDuration = 0;
            }
            else if (barriers[i].isPlaying && emissionDuration >= 4)
            {
                barriers[i].Pause();
            }
            else if (!blockVision)
            {
                emissionDuration = 0;
                if (barriers[i].isPaused) barriers[i].Play();
            }
        }
    }

    // Handles when a player enters the trigger area, adds them to the list of AimConstraint sources.
    public void AddToAimConstraint(GameObject other)
    {
        if (other.CompareTag("Player"))
        { // Bugged, doesn't work more than the first time, need to make sure that the AimConstraint sources list
        // doesn't cause problems when it's empty
            PlayerController player = other.GetComponent<PlayerController>();
            ConstraintSource cameraSource = new() { sourceTransform = player.GetCamera().transform, weight = 1 };
            canvasConstraint.AddSource(cameraSource);
            blockVision = false;
        }
        // Whenever a player enters the office, ensure that the canvas and its children are all visible. Also ensure the
        // particles blocking view to the trucks are disabled.
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
        bool allPlayersLeft = true;
        ConstraintSource test = new() { sourceTransform = null, weight = 0 };
        for (int i = 0; i < canvasConstraint.sourceCount; i++)
        {
            if (canvasConstraint.GetSource(i).Equals(test))
            {}
            else
            { // If even a single player is in the office, this forces the first code block below to execute,
            // rather than the one below it.
                allPlayersLeft = false;
            }
        }
        Renderer[] renderers = canvas.GetComponentsInChildren<Renderer>();
        if (allPlayersLeft)
        { // When all players leave then set the canvas and its children to be invisible. Also re-enable the particles. 
            canvas.SetActive(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
            blockVision = true;
        } else
        { // Otherwise ensure the canvas and its children are visible, and the particles are off.
            canvas.SetActive(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }
            blockVision = false;
        }
    }
}

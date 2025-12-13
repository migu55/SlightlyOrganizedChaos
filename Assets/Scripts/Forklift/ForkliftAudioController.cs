using UnityEngine;

/// <summary>
/// Manages all audio playback for the forklift including engine sounds and fork lift/lower sounds.
/// Handles smooth transitions between engine states (start, idle, stop) and pitch modulation based on speed.
/// </summary>
public class ForkliftAudioController : MonoBehaviour
{
    // Main engine audio source (currently unused but available for future expansion)
    public AudioSource engineAudioSource;
    
    // Audio source for the looping engine idle sound
    public AudioSource engineIdleClip;
    
    // Audio source for the engine start sound (one-shot)
    public AudioSource engineStartClip;
    
    // Audio source for the engine stop sound (one-shot)
    public AudioSource engineStopClip;
    
    // Audio source for the fork hydraulic lift/lower sound
    public AudioSource forkLiftClip;

    // Coroutine reference to manage delayed idle playback after engine start
    private Coroutine engineStartRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    /// <summary>
    /// Plays the engine start sound, then automatically transitions to idle after the start sound completes.
    /// Stops any previous idle sound and cancels pending transitions to prevent overlapping audio.
    /// </summary>
    public void PlayEngineStart()
    {
        // Stop any pending idle start coroutine before restarting
        if (engineStartRoutine != null)
        {
            StopCoroutine(engineStartRoutine);
        }

        // Stop idle so it doesn't overlap with the start sound
        if (engineIdleClip != null && engineIdleClip.isPlaying)
        {
            engineIdleClip.Stop();
        }

        if (engineStartClip != null)
        {
            // Play start sound and schedule idle to begin after it finishes
            engineStartClip.Play();
            float delay = engineStartClip.clip != null ? engineStartClip.clip.length : 0f;
            engineStartRoutine = StartCoroutine(PlayIdleAfterDelay(delay));
        }
        else
        {
            // No start clip assigned, go straight to idle
            PlayEngineIdle();
        }
    }

    /// <summary>
    /// Coroutine that waits for the specified delay then starts the idle engine sound.
    /// Used to create smooth audio transition from engine start to idle.
    /// </summary>
    /// <param name="delay">Time in seconds to wait before playing idle sound</param>
    private System.Collections.IEnumerator PlayIdleAfterDelay(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        PlayEngineIdle();
        engineStartRoutine = null;
    }

    /// <summary>
    /// Plays the engine stop sound (one-shot).
    /// Typically called when the player dismounts from the forklift.
    /// </summary>
    public void PlayEngineStop()
    {
        if (engineStopClip != null)
        {
            engineStopClip.Play();
        }
    }

    /// <summary>
    /// Plays the looping engine idle sound if it's not already playing.
    /// Used for continuous engine running audio while the forklift is mounted.
    /// </summary>
    public void PlayEngineIdle()
    {
        if (engineIdleClip != null && !engineIdleClip.isPlaying)
        {
            engineIdleClip.Play();
        }
    }

    /// <summary>
    /// Adjusts the pitch of the idle engine sound to simulate RPM changes.
    /// Higher pitch indicates faster movement/acceleration.
    /// </summary>
    /// <param name="pitch">Pitch multiplier (typically 0.5 to 1.5)</param>
    public void ChangeEngineIdlePitch(float pitch)
    {
        if (engineIdleClip != null)
        {
            engineIdleClip.pitch = pitch;
        }
    }

    /// <summary>
    /// Stops the engine idle loop.
    /// Called when the engine is turned off or player dismounts.
    /// </summary>
    public void StopEngineIdle()
    {
        if (engineIdleClip != null && engineIdleClip.isPlaying)
        {
            engineIdleClip.Stop();
        }
    }

    /// <summary>
    /// Plays the fork lift hydraulic sound if it's not already playing.
    /// Used when the forks are being raised or lowered.
    /// </summary>
    public void PlayForkLiftSound()
    {
        if (forkLiftClip != null && !forkLiftClip.isPlaying)
        {
            forkLiftClip.Play();
        }
    }

    /// <summary>
    /// Plays the fork lift sound in reverse (negative pitch) to simulate lowering.
    /// Provides audio feedback for downward fork movement.
    /// </summary>
    public void PlayForkLiftSoundReverse()
    {
        PlayForkLiftSound();
        forkLiftClip.pitch = -1f;
    }

    /// <summary>
    /// Stops the fork lift hydraulic sound and resets pitch to normal.
    /// Called when fork movement input stops.
    /// </summary>
    public void StopForkLiftSound()
    {
        if (forkLiftClip != null && forkLiftClip.isPlaying)
        {
            forkLiftClip.Stop();
            forkLiftClip.pitch = 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

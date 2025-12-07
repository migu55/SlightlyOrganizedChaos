using UnityEngine;

public class ForkliftAudioController : MonoBehaviour
{

    public AudioSource engineAudioSource;
    public AudioSource engineIdleClip;
    public AudioSource engineStartClip;
    public AudioSource engineStopClip;
    public AudioSource forkLiftClip;

    private Coroutine engineStartRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PlayEngineStart()
    {
        // stop any pending idle start coroutine before restarting
        if (engineStartRoutine != null)
        {
            StopCoroutine(engineStartRoutine);
        }

        // stop idle so it doesn't overlap with the start sound
        if (engineIdleClip != null && engineIdleClip.isPlaying)
        {
            engineIdleClip.Stop();
        }

        if (engineStartClip != null)
        {
            engineStartClip.Play();
            float delay = engineStartClip.clip != null ? engineStartClip.clip.length : 0f;
            engineStartRoutine = StartCoroutine(PlayIdleAfterDelay(delay));
        }
        else
        {
            // no start clip, go straight to idle
            PlayEngineIdle();
        }
    }

    private System.Collections.IEnumerator PlayIdleAfterDelay(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        PlayEngineIdle();
        engineStartRoutine = null;
    }

    public void PlayEngineStop()
    {
        if (engineStopClip != null)
        {
            engineStopClip.Play();
        }
    }

    public void PlayEngineIdle()
    {
        if (engineIdleClip != null && !engineIdleClip.isPlaying)
        {
            engineIdleClip.Play();
        }
    }

    public void ChangeEngineIdlePitch(float pitch)
    {
        if (engineIdleClip != null)
        {
            engineIdleClip.pitch = pitch;
        }
    }

    public void StopEngineIdle()
    {
        if (engineIdleClip != null && engineIdleClip.isPlaying)
        {
            engineIdleClip.Stop();
        }
    }

    public void PlayForkLiftSound()
    {
        if (forkLiftClip != null && !forkLiftClip.isPlaying)
        {
            forkLiftClip.Play();
        }
    }

    public void PlayForkLiftSoundReverse()
    {
        PlayForkLiftSound();
        forkLiftClip.pitch = -1f;
        Debug.Log(forkLiftClip.pitch);
    }

    public void StopForkLiftSound()
    {
        if (forkLiftClip != null && forkLiftClip.isPlaying)
        {
            forkLiftClip.Stop();
            forkLiftClip.pitch = 1f;
            Debug.Log("Stopped: " + forkLiftClip.pitch);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

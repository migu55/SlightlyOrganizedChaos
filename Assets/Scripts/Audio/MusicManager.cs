using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource source;

    [Header("Tracks")]
    public AudioClip menuMusic;
    public AudioClip preroundMusic;
    public AudioClip roundMusic;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip clip, float fadeTime = 1f)
    {
        if (source.clip == clip) return; // already playing

        StartCoroutine(FadeMusic(clip, fadeTime));
    }

    public void StopMusic()
    {
        source.Stop();
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float time)
    {
        float startVol = source.volume;

        // Fade out
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVol, 0, t / time);
            yield return null;
        }

        source.clip = newClip;
        source.Play();

        // Fade in
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(0, startVol, t / time);
            yield return null;
        }

        source.volume = startVol;
    }
}

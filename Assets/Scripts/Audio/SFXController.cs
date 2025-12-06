using UnityEngine;

public class SFXController : MonoBehaviour
{
    public static SFXController Instance;

    [Header("Default Audio Settings")]
    public bool randomizePitch = false;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("SFX Tracks")]
    public AudioClip roundWhistle;
    public AudioClip clockInWhistle;
    public AudioClip missionComplete;
    public AudioClip missionFailed;
    public AudioClip orderPlaced;
    public AudioClip palletRecieved;
    public AudioClip uiInput;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Play SFX in 2D
    public void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        if (randomizePitch)
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
        else
            audioSource.pitch = 1f;

        audioSource.PlayOneShot(clip);
    }

    // Play SFX in 3D at a world position
    public void PlayAt(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, pos, volume);
    }
}

using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    public AudioSource audioSource;
    private AudioClip hoverSound;
    private AudioClip clickSound;

    private static SoundEffectsManager instance;
    public static SoundEffectsManager Instance => instance;

    private float volume = 1.0f; // 0.0f - 1.0f

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            hoverSound = Resources.Load<AudioClip>("Sounds/Hover Sound Effect");
            clickSound = Resources.Load<AudioClip>("Sounds/Click Sound Effect");
            volume = PlayerPrefs.GetFloat("SoundEffectsVolume", volume);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayHoverSound()
    {
        audioSource.PlayOneShot(hoverSound, volume);
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound, volume);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", volume);
    }

    public float GetVolume()
    {
        return volume * 100f;
    }
}

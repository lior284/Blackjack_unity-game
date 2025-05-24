using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundEffect : MonoBehaviour
{
    private SoundEffectsManager soundManager;

    void Start()
    {
        // Get the SoundManager (make sure to set it in the inspector or find it in the scene)
        soundManager = FindFirstObjectByType<SoundEffectsManager>();
    }

    public void OnHover()
    {
        if (soundManager != null)
        {
            soundManager.PlayHoverSound();
        }
    }

    public void OnClick()
    {
        if (soundManager != null)
        {
            soundManager.PlayClickSound();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public GameObject settingsMenu;

    public Slider musicSlider;
    public Slider soundEffectsSlider;
    public Dropdown colorDropdown;

    public Text backBtnText;

    private BackgroundMusicManager backgroundMusicManager;
    private SoundEffectsManager soundEffectsManager;
    private Cardback cardback;

    public void OpenSettings()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        backBtnText.color = Color.black;
        settingsMenu.SetActive(false);
    }

    private void Start()
    {
        if(backgroundMusicManager == null)
        {
            backgroundMusicManager = FindFirstObjectByType<BackgroundMusicManager>();
        }
        musicSlider.value = backgroundMusicManager.GetVolume();

        if(soundEffectsManager == null)
        {
            soundEffectsManager = FindFirstObjectByType<SoundEffectsManager>();
        }
        soundEffectsSlider.value = soundEffectsManager.GetVolume();

        if(cardback == null)
        {
            cardback = FindFirstObjectByType<Cardback>();
        }
        colorDropdown.value = cardback.GetSelectedCardBackIndex();
    }

    public void ResetSettings()
    {
        backgroundMusicManager.SetVolume(1.0f);
        musicSlider.value = 100;

        soundEffectsManager.SetVolume(1.0f);
        soundEffectsSlider.value = 100;

        cardback.SetSelectedCardBack(0);
        colorDropdown.value = 0;

        PlayerPrefs.Save();
    }

    public void UpdateMusicVolume()
    {
        backgroundMusicManager.SetVolume(musicSlider.value / 100f);
    }
    public void UpdateSoundEffectsVolume()
    {
        soundEffectsManager.SetVolume(soundEffectsSlider.value / 100f);
    }

    public void UpdateBackOfCard()
    {
        Cardback.Instance.SetSelectedCardBack(colorDropdown.value);
    }
}

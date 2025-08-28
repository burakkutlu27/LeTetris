using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform mainMenu, settingsMenu;
    [SerializeField] private Slider MusicSlider, SoundSlider;
    
    private void Start()
    {
        InitializeSliders();
        SetInitialVolumes();
    }
    
    private void InitializeSliders()
    {
        // Initialize Music Slider
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.2f);
        }
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        
        // Initialize Sound Slider
        if (!PlayerPrefs.HasKey("SoundVolume"))
        {
            PlayerPrefs.SetFloat("SoundVolume", 0.5f);
        }
        SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
    }
    
    private void SetInitialVolumes()
    {
        // Set initial music volume using SoundManager
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
            SoundManager.instance.SetSFXVolume(PlayerPrefs.GetFloat("SoundVolume"));
            // MainMenu'de müziği başlat
            SoundManager.instance.StartGameMusic();
        }
    }
    
    public void OpenSettingsMenu()
    {
        mainMenu.GetComponent<RectTransform>().DOLocalMoveX(-1200, .5f);
        settingsMenu.GetComponent<RectTransform>().DOLocalMoveX(0, .5f);
    }
    
    public void CloseSettingsMenu()
    {
        mainMenu.GetComponent<RectTransform>().DOLocalMoveX(0, .5f);
        settingsMenu.GetComponent<RectTransform>().DOLocalMoveX(1200, .5f);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GamePlay");
    }
    
    public void ChangeMusicVolume()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMusicVolume(MusicSlider.value);
            PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("SoundManager is not available!");
        }
    }
    
    public void ChangeSoundVolume()
    {
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.Save();
        
        // SoundManager'daki SFX volume'unu da güncelle
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetSFXVolume(SoundSlider.value);
        }
    }
}

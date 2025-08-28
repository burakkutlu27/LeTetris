using NUnit.Framework;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioClip[] MusicClips;
    [SerializeField] private AudioSource musicSource;
    public bool IsMusicPlaying = true;

    [SerializeField] private AudioClip[] SFXClips;
    [SerializeField] private AudioSource[] VocalClips;
    public bool IsSFXPlaying = true;
    private AudioClip randomClip;

    public IconManager fxicon;
    public IconManager musicicon;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        randomClip = GetRandomMusicClip(MusicClips);
        // Müziği otomatik başlatma - sadece gerektiğinde başlatılacak
        // PlayBackgroundMusic(randomClip);
        
        // PlayerPrefs'ten ses ayarlarını yükle
        LoadVolumeSettings();
    }
    
    private void LoadVolumeSettings()
    {
        if (musicSource != null)
        {
            // Music volume'u PlayerPrefs'ten yükle
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                musicSource.volume = PlayerPrefs.GetFloat("MusicVolume");
            }
            else
            {
                // Varsayılan değer
                musicSource.volume = 0.2f;
            }
        }
    }
    
    private void Update()
    {
        // Müzik döngüsünü kontrol et
        UpdateMusic();
    }

    public void PlaySFX(int clipIndex)
    {
        if (IsSFXPlaying && clipIndex >= 0 && clipIndex < SFXClips.Length)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.clip = SFXClips[clipIndex];
            // SFX volume'unu PlayerPrefs'ten al
            if (PlayerPrefs.HasKey("SoundVolume"))
            {
                sfxSource.volume = PlayerPrefs.GetFloat("SoundVolume");
            }
            else
            {
                sfxSource.volume = 0.5f; // Varsayılan değer
            }
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning("SFX playback is disabled or invalid clip index.");
        }
    }

    public void PlayRandomVocal()
    {
        if (IsSFXPlaying && VocalClips != null && VocalClips.Length > 0)
        {
            int randomSourceIndex = Random.Range(0, VocalClips.Length);
            VocalClips[randomSourceIndex].clip = VocalClips[randomSourceIndex].clip;
            // Vocal sesleri için de volume ayarı
            if (PlayerPrefs.HasKey("SoundVolume"))
            {
                VocalClips[randomSourceIndex].volume = PlayerPrefs.GetFloat("SoundVolume");
            }
            VocalClips[randomSourceIndex].Play();
        }
    }

    AudioClip GetRandomMusicClip(AudioClip[] clips)
    {
        AudioClip rastgeleClip = clips[Random.Range(0, clips.Length)];
        return rastgeleClip;
    }

    public void PlayBackgroundMusic(AudioClip musicClip)
    {
        if (!musicClip || !musicSource || !IsMusicPlaying)
        {
            return;
        }
        musicSource.clip = musicClip;
        musicSource.Play();
    }
    
    void UpdateMusic()
    {
        if (IsMusicPlaying)
        {
            if (!musicSource.isPlaying)
            {
                randomClip = GetRandomMusicClip(MusicClips);
                PlayBackgroundMusic(randomClip);
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
    }
    
    public void ToggleMusic()
    {
        IsMusicPlaying = !IsMusicPlaying;
        UpdateMusic();
        Debug.Log($"Music toggle → {(IsMusicPlaying ? "ON" : "OFF")}");

        if (musicicon != null)
        {   
            Debug.Log("MusicIcon referansı dolu, ToggleIcon çağrılıyor...");
            musicicon.ToggleIcon(IsMusicPlaying);
        }
        else
        {
            Debug.LogWarning("MusicIcon referansı NULL!");
        }
    }   

    public void ToggleSFX()
    {
        IsSFXPlaying = !IsSFXPlaying;
        if (!IsSFXPlaying)
        {
            foreach (AudioSource sfxSource in GetComponents<AudioSource>())
            {
                Destroy(sfxSource);
            }
        }
        if (fxicon != null)
            fxicon.ToggleIcon(IsSFXPlaying);
    }
    
    // Public method to set music volume
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    // Public method to get music volume
    public float GetMusicVolume()
    {
        if (musicSource != null)
        {
            return musicSource.volume;
        }
        return 0f;
    }
    
    // Public method to set SFX volume
    public void SetSFXVolume(float volume)
    {
        // Mevcut SFX AudioSource'larının volume'unu güncelle
        AudioSource[] allAudioSources = GetComponents<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != musicSource) // Müzik değil, SFX ise
            {
                audioSource.volume = volume;
            }
        }
    }
    
    // Oyun başladığında müziği başlat
    public void StartGameMusic()
    {
        if (randomClip == null)
        {
            randomClip = GetRandomMusicClip(MusicClips);
        }
        PlayBackgroundMusic(randomClip);
        Debug.Log("Game music started!");
    }
}

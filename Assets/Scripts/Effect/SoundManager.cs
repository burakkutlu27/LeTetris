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
        PlayBackgroundMusic(randomClip);
    }

    public void PlaySFX(int clipIndex)
    {
        if (IsSFXPlaying && clipIndex >= 0 && clipIndex < SFXClips.Length)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.clip = SFXClips[clipIndex];
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
}

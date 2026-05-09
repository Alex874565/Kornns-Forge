using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    ButtonClick
}

public enum MusicType
{
    BackgroundMusic
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("---------- Audio Source ----------")]
    [SerializeField] private AudioSource musicSource;

    [Header("---------- Pool Settings ----------")]
    [SerializeField] private AudioSource sfxPrefab;
    [SerializeField] private int initialPoolSize = 10;

    [Header("---------- Audio Data ----------")]
    [SerializeField] private SoundData[] sounds;
    [SerializeField] private MusicData[] musicTracks;

    private Dictionary<SoundType, SoundData> soundDictionary;
    private Dictionary<MusicType, MusicData> musicDictionary;

    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeDictionaries();
        InitializePool();
    }

    private void InitializeDictionaries()
    {
        soundDictionary = new Dictionary<SoundType, SoundData>();

        foreach (SoundData sound in sounds)
        {
            soundDictionary[sound.type] = sound;
        }

        musicDictionary = new Dictionary<MusicType, MusicData>();

        foreach (MusicData music in musicTracks)
        {
            musicDictionary[music.type] = music;
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource source = Instantiate(sfxPrefab, transform);

        source.gameObject.SetActive(false);

        sfxPool.Enqueue(source);

        return source;
    }

    private AudioSource GetAudioSource()
    {
        if (sfxPool.Count == 0)
        {
            CreateNewAudioSource();
        }

        AudioSource source = sfxPool.Dequeue();

        source.gameObject.SetActive(true);

        return source;
    }

    private IEnumerator ReturnToPool(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        source.clip = null;
        source.loop = false;

        source.gameObject.SetActive(false);

        sfxPool.Enqueue(source);
    }

    /* SFX */
    public static void PlaySound(SoundType type)
    {
        if (Instance.soundDictionary.TryGetValue(type, out SoundData soundData))
        {
            AudioSource source = Instance.GetAudioSource();

            source.clip = soundData.clip;
            source.volume = soundData.volume;
            source.loop = false;

            source.Play();

            Instance.StartCoroutine(
                Instance.ReturnToPool(source)
            );
        }
        else
        {
            Debug.LogWarning($"Sound not found: {type}");
        }
    }

    /* Music */

    public static void PlayMusic(MusicType type)
    {
        if (Instance.musicDictionary.TryGetValue(type, out MusicData musicData))
        {
            Instance.musicSource.clip = musicData.clip;
            Instance.musicSource.volume = musicData.volume;
            Instance.musicSource.loop = true;

            Instance.musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music not found: {type}");
        }
    }

    public static void StopMusic()
    {
        Instance.musicSource.Stop();
    }

    public static void PauseMusic()
    {
        Instance.musicSource.Pause();
    }

    public static void ResumeMusic()
    {
        Instance.musicSource.UnPause();
    }
}
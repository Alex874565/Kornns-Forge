using UnityEngine;

[System.Serializable]
public class MusicData
{
    public MusicType type;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}

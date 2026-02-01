using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private AudioSource audioSource;

    public AudioSource baseMusicAudioSource;
    public AudioSource greenMusicAudioSource;
    public AudioSource redMusicAudioSource;
    public AudioSource blueMusicAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();

        baseMusicAudioSource.volume = 1f;
        greenMusicAudioSource.volume = 0f;
        redMusicAudioSource.volume = 0f;
        blueMusicAudioSource.volume = 0f;
    }

    private void Update()
    {
        PlayerController[] player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        if (player.Length == 0)
        {
            greenMusicAudioSource.volume = 0f;
            redMusicAudioSource.volume = 0f;
            blueMusicAudioSource.volume = 0f;
        }

        MaskType currentMask = player[0].CurrentMask;

        switch (currentMask)
        {
            case MaskType.Green:
                greenMusicAudioSource.volume = 1f;
                redMusicAudioSource.volume = 0f;
                blueMusicAudioSource.volume = 0f;
                break;
            case MaskType.Red:
                greenMusicAudioSource.volume = 0f;
                redMusicAudioSource.volume = 1f;
                blueMusicAudioSource.volume = 0f;
                break;
            case MaskType.Blue:
                greenMusicAudioSource.volume = 0f;
                redMusicAudioSource.volume = 0f;
                blueMusicAudioSource.volume = 1f;
                break;
        }
    }

    public void PlaySFX(AudioClipPlus audioClip)
    {
        audioSource.pitch = audioClip.Pitch;
        if (audioClip.PitchVariance > 0)
        {
            audioSource.pitch += UnityEngine.Random.Range(-audioClip.PitchVariance, audioClip.PitchVariance);
        }
        audioSource.PlayOneShot(audioClip.AudioClip, audioClip.Volume);
    }

    public void PlayRandomSFX(List<AudioClipPlus> audioClips)
    {
        if (audioClips.Count == 0) return;
        int randomIndex = UnityEngine.Random.Range(0, audioClips.Count);
        PlaySFX(audioClips[randomIndex]);
    }
}

[Serializable]
public class AudioClipPlus
{
    public AudioClip AudioClip;
    public float Volume = 1.0f;
    public float Pitch = 1.0f;
    public float PitchVariance = 0.0f;

    public AudioClipPlus(AudioClip clip, float vol = 1.0f, float pit = 1.0f, float pitchVariance = 0)
    {
        AudioClip = clip;
        Volume = vol;
        Pitch = pit;
        this.PitchVariance = pitchVariance;
    }
}

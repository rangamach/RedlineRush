using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class SoundService
{
    private Dictionary<SoundTypes, AudioClip> soundLookUp;
    private AudioSource bgAudioSource;
    private AudioSource sfxAudioSource;
    private AudioSource carAudioSource;

    //Car AudioSource Variables:
    private float minPitch = 0.85f;
    private float maxPitch = 1.9f;
    private float pitchMultiplier = 0.02f;

    private float idleVolume = 0.6f;
    private float maxVolume = 1.0f;
    private float volumeMultiplier = 0.01f;
    public SoundService(SoundSO SO, AudioSource bg, AudioSource sfx, AudioSource car)
    {
        this.bgAudioSource = bg;
        this.sfxAudioSource = sfx;
        this.carAudioSource = car;

        soundLookUp = new Dictionary<SoundTypes, AudioClip>();

        foreach(var snd in SO.Sounds)
        {
            if(!soundLookUp.ContainsKey(snd.type) && snd.clip != null)
                soundLookUp[snd.type] = snd.clip;
        }

        PlayBackgroundMusic();
    }
    private void PlayBackgroundMusic()
    {
        soundLookUp.TryGetValue(SoundTypes.Background, out AudioClip clip);
        bgAudioSource.clip = clip;
        bgAudioSource.loop = true;
        bgAudioSource.Play();
    }
    public void PlaySFXMusic(SoundTypes type)
    {
        if (soundLookUp.TryGetValue(type, out AudioClip clip))
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
    public void StartCarEngine()
    {
        if (soundLookUp.TryGetValue(SoundTypes.CarStart, out AudioClip clip))
        {
            carAudioSource.loop = false;
            carAudioSource.clip = clip;
            carAudioSource.Play();
        }
        else
        {
            Debug.Log(SoundTypes.CarStart + " Sound not found!!!");
        }
    }
    public void StartCarEngineLoop()
    {
        if(soundLookUp.TryGetValue(SoundTypes.CarEngine,out AudioClip clip))
        {
            carAudioSource.loop = true;
            carAudioSource.clip = clip;
            carAudioSource.Play();
        }
        else
        {
            Debug.Log(SoundTypes.CarEngine + " Sound not found!!!");
        }
    }
    public float GetAudioClipLength(SoundTypes type)
    {
        if(soundLookUp.TryGetValue(type,out AudioClip clip))
        {
            return clip.length;
        }
        return 0f;
    }
    public void StopCarEngine()
    {
        if(carAudioSource.isPlaying)
        {
            carAudioSource.Stop();
        }
    }
    public void UpdateCarEnginePitch(float speed)
    {
        carAudioSource.pitch = Mathf.Clamp(minPitch + speed * pitchMultiplier,minPitch,maxPitch);
        carAudioSource.volume = Mathf.Clamp(idleVolume + speed * volumeMultiplier, idleVolume,maxVolume);
    }
}
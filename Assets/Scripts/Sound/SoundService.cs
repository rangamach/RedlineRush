using System.Collections.Generic;
using UnityEngine;

public class SoundService
{
    private Dictionary<SoundTypes, AudioClip> soundLookUp;
    private AudioSource bgAudioSource;
    private AudioSource sfxAudioSource;
    public SoundService(SoundSO SO, AudioSource bg, AudioSource sfx)
    {
        this.bgAudioSource = bg;
        this.sfxAudioSource = sfx;

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
}
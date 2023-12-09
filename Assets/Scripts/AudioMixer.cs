using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffects
{
    land,
    swap,
    resolve,
    upgrade,
    powerup,
    score
}


[RequireComponent(typeof(AudioSource))]
public class AudioMixer : Singleton<AudioMixer>
{
    [SerializeField] private AudioSource music, soundEffects;

    [SerializeField] private AudioClip[] sounds;

    protected override void Init()
    {
        soundEffects = GetComponent<AudioSource>();
    }

    public void PlayMusic()
    {
        music.Play();
    }

    public void StopMusic()
    {
        music.Pause();
    }

    public void PlaySound(SoundEffects effect)
    {
        soundEffects.PlayOneShot(sounds[(int)effect]);
    }

    public IEnumerator PlayDelayedSound(SoundEffects effect, float t)
    {
        yield return new WaitForSeconds(t);
        PlaySound(effect);
    }
}

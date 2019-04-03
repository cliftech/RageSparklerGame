using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Soundmanager : MonoBehaviour
{
    public AudioSource oneShotAudioSource;
    public AudioSource loopAudioSource;

    public void PlayOneShot(AudioClip clip, float delay = 0)
    {
        StartCoroutine(PlayOneShotSource(clip, delay));
    }

    IEnumerator PlayOneShotSource(AudioClip clip, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        oneShotAudioSource.PlayOneShot(clip);
    }

    public void PlayFor(AudioClip clip, float time, float delay = 0)
    {
        StopCoroutine("TurnOffLoopSource");
        loopAudioSource.clip = clip;
        StartCoroutine(TurnOffLoopSource(time, delay));
    }

    IEnumerator TurnOffLoopSource(float time, float delay)
    {
        if(delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        loopAudioSource.Play();
        loopAudioSource.volume = 1;
        yield return new WaitForSecondsRealtime(time);
        while (loopAudioSource.volume > 0.01)
        {
            loopAudioSource.volume -= Time.fixedDeltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        loopAudioSource.volume = 0;
        loopAudioSource.Stop();
    }
}

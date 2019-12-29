using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public List<AudioClip> jumpClips;
    public List<AudioClip> landClips;
    public List<AudioClip> gripClips;
    public List<AudioClip> createCubeClips;
    public List<AudioClip> getLumiereClips;
    public List<AudioClip> failActionClips;
    public List<AudioClip> hitClips;
    public List<AudioClip> hitTracerClips;
    public List<AudioClip> emissionTracerClips;
    public List<AudioClip> detectionClips;
    public List<AudioClip> timeOutClips;
    public List<AudioClip> receivedMessageClips;
    public List<AudioClip> eventStartClips;

    public List<AudioClip> normalMusics;
    public List<AudioClip> endGameMusics;

    public AudioSource globalSource;
    public AudioSource instantSource; // La source à utiliser pour des clips de courte durée non-spatialisé

    public void Initialize() {
        globalSource.volume = PlayerPrefs.GetFloat(MenuOptions.MUSIC_VOLUME_KEY);
        PlayClipsOnSource(normalMusics, globalSource);
    }

    public void PlayJumpClip(AudioSource source) {
        PlayClipsOnSource(jumpClips, source);
    }
    public void PlayLandClip(AudioSource source) {
        PlayClipsOnSource(landClips, source);
    }
    public void PlayGripClip(AudioSource source) {
        PlayClipsOnSource(gripClips, source);
    }
    public void PlayCreateCubeClip(AudioSource source) {
        PlayClipsOnSource(createCubeClips, source);
    }
    public void PlayFailActionClip() {
        PlayClipsOnSource(failActionClips, instantSource);
    }
    public void PlayTimeOutClip() {
        PlayClipsOnSource(timeOutClips, instantSource);
    }
    public void PlayReceivedMessageClip() {
        PlayClipsOnSource(receivedMessageClips, instantSource);
    }
    public void PlayEventStartClip() {
        PlayClipsOnSource(eventStartClips, instantSource);
    }
    public void PlayEventEndClip() {
        PlayClipsOnSource(eventStartClips, instantSource, bReverse: true);
    }
    public void PlayDetectionClip(AudioSource source) {
        source.spatialBlend = 0.5f;
        PlayClipsOnSource(detectionClips, source);
    }
    public void PlayEmissionTracerClip(AudioSource source, float duree) {
        source.spatialBlend = 1.0f;
        PlayClipsOnSource(emissionTracerClips, source);
        StartCoroutine(StopClipIn(source, duree));
    }
    public void PlayHitClip(AudioSource source, bool priority = false) {
        if(!source.isPlaying || priority)
            PlayClipsOnSource(hitClips, source);
    }
    public void PlayHitTracerClip(AudioSource source) {
        PlayClipsOnSource(hitTracerClips, source);
    }
    public void PlayGetLumiereClip(Vector3 pos) {
        AudioSource source = new GameObject().AddComponent<AudioSource>();
        source.transform.position = pos;
        source.spatialBlend = 1.0f;
        PlayClipsOnSource(getLumiereClips, source);
    }
    public void PlayEndGameMusic() {
        PlayClipsOnSource(endGameMusics, globalSource);
    }

    protected void PlayClipsOnSource(List<AudioClip> clips, AudioSource source, bool bReverse = false) {
        if(source != globalSource)
            source.volume = PlayerPrefs.GetFloat(MenuOptions.SOUND_VOLUME_KEY);
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        source.clip = clip;
        if(bReverse) {
            source.timeSamples = source.clip.samples - 1;
            source.pitch = -1;
        } else {
            source.pitch = 1;
            source.timeSamples = 0;
        }
        source.Play();
    }

    protected IEnumerator StopClipIn(AudioSource source, float duree) {
        yield return new WaitForSeconds(duree);
        source.Stop();
    }

}

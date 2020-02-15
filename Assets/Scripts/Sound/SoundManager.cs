﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioClipParams jumpClips;
    public AudioClipParams landClips;
    public AudioClipParams gripClips;
    public AudioClipParams createCubeClips;
    public AudioClipParams getLumiereClips;
    public AudioClipParams failActionClips;
    public AudioClipParams hitClips;
    public AudioClipParams hitTracerClips;
    public AudioClipParams emissionTracerClips;
    public AudioClipParams detectionClips;
    public AudioClipParams timeOutClips;
    public AudioClipParams receivedMessageClips;
    public AudioClipParams eventStartClips;
    public AudioClipParams eventEndClips;
    public AudioClipParams jumpSuccessClips;
    public AudioClipParams jumpEventStunClips;
    public AudioClipParams jumpEventUnStunClips;
    public AudioClipParams activationPouvoirClips;
    public AudioClipParams deniedPouvoirClips;
    public AudioClipParams notFoundPouvoirClips;

    public AudioClipParams normalMusics;
    public AudioClipParams endGameMusics;

    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;

    public void Initialize() {
        globalSoundsFolder = new GameObject("Sounds").transform;
        //globalSource.volume = PlayerPrefs.GetFloat(MenuOptions.MUSIC_VOLUME_KEY);
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        PlayClipsOnSource(normalMusics);
    }

    public AudioSource GetAvailableSource() {
        // On vérifie si celles déjà utilisées ont finit d'être utilisées
        for(int i = 0; i < usedSources.Count; i++) {
            AudioSource source = usedSources[i];
            if(source == null) {
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
            if (source.isPlaying == false)
                availableSources.Add(source);
        }

        // Si on en a déjà de disponible, c'est cool ! :D
        if(availableSources.Count > 0) {
            AudioSource res = availableSources[0];
            availableSources.RemoveAt(0);
            usedSources.Add(res);
            return res;
        }

        // Sinon on va devoir en créer une ! :)
        AudioSource newSource = new GameObject("Source").AddComponent<AudioSource>();
        usedSources.Add(newSource);
        return newSource;
    }

    public void PlayJumpClip(Vector3 pos) {
        PlayClipsOnSource(jumpClips, pos);
    }
    public void PlayLandClip(Vector3 pos) {
        PlayClipsOnSource(landClips, pos);
    }
    public void PlayGripClip(Vector3 pos) {
        PlayClipsOnSource(gripClips, pos);
    }
    public void PlayCreateCubeClip(Vector3 pos) {
        PlayClipsOnSource(createCubeClips, pos);
    }
    public void PlayFailActionClip() {
        PlayClipsOnSource(failActionClips);
    }
    public void PlayTimeOutClip() {
        PlayClipsOnSource(timeOutClips);
    }
    public void PlayReceivedMessageClip() {
        PlayClipsOnSource(receivedMessageClips);
    }
    public void PlayEventStartClip() {
        PlayClipsOnSource(eventStartClips);
    }
    public void PlayEventEndClip() {
        // reverse !
        PlayClipsOnSource(eventStartClips);
    }
    public void PlayJumpSuccessClip() {
        PlayClipsOnSource(jumpSuccessClips);
    }
    public void PlayDetectionClip(Vector3 pos, Transform parent) {
        //source.spatialBlend = 0.5f;
        PlayClipsOnSource(detectionClips, pos, parent);
    }
    public void PlayEmissionTracerClip(Vector3 pos, Transform parent, float duree) {
        //source.spatialBlend = 1.0f;
        AudioSource source = PlayClipsOnSource(emissionTracerClips, pos, parent);
        StartCoroutine(StopClipIn(source, duree));
    }
    public void PlayHitClip(Vector3 pos) {
        // reverse !
        PlayClipsOnSource(hitClips, pos);
    }
    public void PlayHitTracerClip(Vector3 pos, Transform parent) {
        PlayClipsOnSource(hitTracerClips, pos, parent);
    }
    public void PlayGetLumiereClip(Vector3 pos) {
        //source.spatialBlend = 1.0f;
        PlayClipsOnSource(getLumiereClips, pos);
    }
    public void PlayEndGameMusic() {
        PlayClipsOnSource(endGameMusics);
    }
    public void PlayJumpEventStunClip() {
        PlayClipsOnSource(jumpEventStunClips);
    }
    public void PlayJumpEventUnStunClip() {
        PlayClipsOnSource(jumpEventUnStunClips);
    }
    public void PlayActivationPouvoirClip(AudioClipParams audioClip = null) {
        PlayClipsOnSource((audioClip == null) ? activationPouvoirClips : audioClip);
    }
    public void PlayDeniedPouvoirClip() {
        PlayClipsOnSource(deniedPouvoirClips);
    }
    public void PlayNotFoundPouvoirClip() {
        PlayClipsOnSource(notFoundPouvoirClips);
    }

    //protected void PlayClipsOnSource(List<AudioClip> clips, AudioSource source, bool bReverse = false) {
    protected AudioSource PlayClipsOnSource(AudioClipParams audioClipParams, Vector3 pos = new Vector3(), Transform parent = null) {
        // On get la source
        AudioSource source = GetAvailableSource();

        // On set le volume
        if(audioClipParams.bIsMusic) {
            source.volume = PlayerPrefs.GetFloat(MenuOptions.MUSIC_VOLUME_KEY);
        } else {
            source.volume = PlayerPrefs.GetFloat(MenuOptions.SOUND_VOLUME_KEY);
        }
        source.volume *= AudioClipParams.BASE_VOLUME * audioClipParams.relativeVolume;

        // On positionne la source
        source.transform.position = pos;
        source.transform.SetParent((parent == null) ? globalSoundsFolder: parent);

        // On get le bon clip
        source.clip = audioClipParams.clips[Random.Range(0, audioClipParams.clips.Count)];

        // On vérifie si c'est reverse ou pas
        if(audioClipParams.bReverse) {
            source.timeSamples = source.clip.samples - 1;
            source.pitch = -1;
        } else {
            source.pitch = 1;
            source.timeSamples = 0;
        }

        // On vérifie si ça loop ou pas !
        source.loop = audioClipParams.bLoop;

        // Et enfin on joue la source ! <3
        source.Play();

        return source;
    }

    protected IEnumerator StopClipIn(AudioSource source, float duree) {
        yield return new WaitForSeconds(duree);
        source.Stop();
    }

}

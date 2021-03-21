using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public SoundBankIngame sounds;

    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;
    protected AudioSource normalMusicSource;
    protected float musicVolume;
    protected float soundVolume;

    public void Initialize() {
        GetAudioVolumes();
        StopUIMusic();
        globalSoundsFolder = new GameObject("Sounds").transform;
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        normalMusicSource = PlayClipsOnSource(sounds.normalMusics);
    }

    public void GetAudioVolumes() {
        musicVolume = PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME_KEY, MenuOptions.defaultMusicVolume);
        soundVolume = PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME_KEY, MenuOptions.defaultSoundVolume);
    }

    public void ApplyAudioVolumes() {
        float oldSoundVolume = soundVolume;
        float oldMusicVolume = musicVolume;
        GetAudioVolumes();
        foreach(AudioSource source in usedSources) {
            float sourceRelativeVolume = source.GetComponent<AudioClipParamsHolder>().clipParams.relativeVolume;
            source.volume = sourceRelativeVolume * soundVolume * AudioClipParams.BASE_VOLUME;
        }
        float musicSourceRelativeVolume = normalMusicSource.GetComponent<AudioClipParamsHolder>().clipParams.relativeVolume;
        normalMusicSource.volume = musicSourceRelativeVolume * musicVolume * AudioClipParams.BASE_VOLUME;
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
            if (source.isPlaying == false) {
                availableSources.Add(source);
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
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
        AudioClipParamsHolder clipHolder = newSource.gameObject.AddComponent<AudioClipParamsHolder>();
        usedSources.Add(newSource);
        return newSource;
    }

    public void PlayJumpClip(Vector3 pos) {
        PlayClipsOnSource(sounds.jumpClips, pos);
    }
    public void PlayLandClip(Vector3 pos) {
        PlayClipsOnSource(sounds.landClips, pos);
    }
    public void PlayGripClip(Vector3 pos) {
        PlayClipsOnSource(sounds.gripClips, pos);
    }
    public void PlayCreateCubeClip(Vector3 pos) {
        PlayClipsOnSource(sounds.createCubeClips, pos);
    }
    public void PlayFailActionClip() {
        PlayClipsOnSource(sounds.failActionClips);
    }
    public void PlayTimeOutClip() {
        PlayClipsOnSource(sounds.timeOutClips);
    }
    public void PlayReceivedMessageClip() {
        PlayClipsOnSource(sounds.receivedMessageClips);
    }
    public void PlayNewBlockClip() {
        PlayClipsOnSource(sounds.newBlockClips);
    }
    public void PlayEventStartClip() {
        PlayClipsOnSource(sounds.eventStartClips);
    }
    public void PlayEventEndClip() {
        // reverse !
        PlayClipsOnSource(sounds.eventStartClips);
    }
    public void PlayJumpEventStartClip(float duration) {
        PlayClipsOnSource(sounds.jumpEventStartClips, duration: duration);
    }
    public void PlayJumpSuccessClip() {
        PlayClipsOnSource(sounds.jumpSuccessClips);
    }
    public void PlayDetectionClip(Vector3 pos, Transform parent) {
        //source.spatialBlend = 0.5f;
        PlayClipsOnSource(sounds.detectionClips, pos, parent);
    }
    public void PlayDissimuleClip() {
        PlayClipsOnSource(sounds.dissimuleClips);
    }
    public AudioSource PlayEmissionTracerClip(Vector3 pos, Transform parent) {
        //source.spatialBlend = 1.0f;
        AudioSource source = PlayClipsOnSource(sounds.emissionTracerClips, pos, parent);
        //StartCoroutine(StopClipIn(source, duree));
        return source;
    }
    public void PlayFirstBossPresenceClip(Vector3 pos, Transform parent) {
        PlayClipsOnSource(sounds.firstBossPresenceClips, pos, parent);
    }
    public void PlayHitClip(Vector3 pos) {
        // reverse !
        PlayClipsOnSource(sounds.hitClips, pos);
    }
    public void PlayTracerHitClip(Vector3 pos, Transform parent) {
        PlayClipsOnSource(sounds.tracerHitClips, pos, parent);
    }
    public AudioSource PlayTracerBlastLoadClip(Vector3 pos, float duration) {
        return PlayClipsOnSource(sounds.tracerBlastLoadClips, pos, null, duration);
    }
    public void PlayDecreasingBallFirstBoss(Vector3 pos, float duration) {
        PlayClipsOnSource(sounds.firstBossDecreasingBall, pos, null, duration);
    }
    public void PlayIncreasingBallFirstBoss(Vector3 pos, float duration) {
        PlayClipsOnSource(sounds.firstBossIncreasingBall, pos, null, duration);
    }
    public void PlayGetLumiereClip(Vector3 pos) {
        //source.spatialBlend = 1.0f;
        PlayClipsOnSource(sounds.getLumiereClips, pos);
    }
    public void PlayGetItemClip(Vector3 pos) {
        PlayClipsOnSource(sounds.getItemClips, pos);
    }
    public void PlayEndGameMusic() {
        normalMusicSource.Stop();
        PlayClipsOnSource(sounds.endGameMusics);
    }
    public void PlayJumpEventStunClip() {
        PlayClipsOnSource(sounds.jumpEventStunClips);
    }
    public void PlayJumpEventUnStunClip() {
        PlayClipsOnSource(sounds.jumpEventUnStunClips);
    }
    public void PlayIntersectionEventClip(Vector3 position, float duration, Transform parent) {
        PlayClipsOnSource(sounds.intersectionEventClips, pos: position,  duration: duration, parent: parent);
    }
    public void PlayActivationPouvoirClip(AudioClipParams audioClip = null) {
        PlayClipsOnSource((audioClip == null) ? sounds.activationPouvoirClips : audioClip);
    }
    public void PlayDeniedPouvoirClip() {
        PlayClipsOnSource(sounds.deniedPouvoirClips);
    }
    public void PlayNotFoundPouvoirClip() {
        PlayClipsOnSource(sounds.notFoundPouvoirClips);
    }
    public void PlayPouvoirAvailableClip() {
        PlayClipsOnSource(sounds.pouvoirAvailableClips);
    }
    public void PlayTimeZoneButtonInClip(Vector3 pos) {
        PlayClipsOnSource(sounds.timeZoneButtonInClips, pos);
    }
    public void PlayTimeZoneButtonOutClip(Vector3 pos) {
        PlayClipsOnSource(sounds.timeZoneButtonOutClips, pos);
    }
    public void PlayRewardBestScore() {
        PlayClipsOnSource(sounds.rewardBestScoreClips);
    }
    public void PlayVictoryClip() {
        PlayClipsOnSource(sounds.victoryClips);
    }
    public void PlayDefeatClip() {
        PlayClipsOnSource(sounds.defeatClips);
    }
    public void PlayBounceClip() {
        PlayClipsOnSource(sounds.bounceClips);
    }

    protected AudioSource PlayClipsOnSource(
        AudioClipParams audioClipParams,
        Vector3 pos = new Vector3(),
        Transform parent = null,
        float duration = -1.0f) {

        // On get la source
        AudioSource source = GetAvailableSource();

        // On set le volume
        if(audioClipParams.bIsMusic) {
            source.volume = musicVolume;
        } else {
            source.volume = soundVolume;
        }
        source.volume *= AudioClipParams.BASE_VOLUME * audioClipParams.relativeVolume;

        // On positionne la source
        source.transform.position = pos;
        source.transform.SetParent((parent == null) ? globalSoundsFolder: parent);

        // On get le bon clip
        AudioClip clip = audioClipParams.clips[UnityEngine.Random.Range(0, audioClipParams.clips.Count)];
        source.clip = clip;
        source.GetComponent<AudioClipParamsHolder>().clipParams = audioClipParams;

        // On vérifie si c'est reverse ou pas
        if(audioClipParams.bReverse) {
            source.timeSamples = source.clip.samples - 1;
            source.pitch = -1;
        } else {
            source.timeSamples = 0;
            source.pitch = 1;
        }

        // On ajuste le pitch pour matcher avec la duration si celle-ci est spécifiée ! :)
        if(duration == -1) {
            source.pitch = source.pitch / Mathf.Abs(source.pitch); // Normalize à 1
        } else {
            float acceleration = clip.length / duration;
            source.pitch *= acceleration;
        }

        // On set le spatial blend !
        source.spatialBlend = audioClipParams.spatialBlend;

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

    protected void StopUIMusic() {
        UISoundManager.Instance.StopMusic();
    }
}

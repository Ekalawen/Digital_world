using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SoundManager : MonoBehaviour
{

    public SoundBankIngame sounds;
    public float variationTransitionDuration = 0.3f;
    public float variationTransitionOffset = 0.03f;
    public float endGameTransitionDuration = 0.15f;
    public List<AudioClipParams> levelMusics;

    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;
    protected List<AudioSource> pausedSources;
    protected AudioSource levelMusicSource;
    protected AudioSource finishingLevelMusicSource;
    protected AudioClipParams levelMusic;
    protected float musicVolume;
    protected float soundVolume;
    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;
        GetAudioVolumes();
        StopUIMusic();
        globalSoundsFolder = new GameObject("Sounds").transform;
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        pausedSources = new List<AudioSource>();
        StartLevelMusic();
    }

    public void GetAudioVolumes()
    {
        musicVolume = PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME_KEY, MenuOptions.defaultMusicVolume);
        soundVolume = PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME_KEY, MenuOptions.defaultSoundVolume);
    }

    public void ApplyAudioVolumes()
    {
        GetAudioVolumes();
        foreach (AudioSource source in usedSources)
        {
            float sourceRelativeVolume = source.GetComponent<AudioClipParamsHolder>().clipParams.relativeVolume;
            source.volume = sourceRelativeVolume * soundVolume * AudioClipParams.BASE_VOLUME;
        }
        float musicSourceRelativeVolume = levelMusicSource.GetComponent<AudioClipParamsHolder>().clipParams.relativeVolume;
        levelMusicSource.volume = musicSourceRelativeVolume * musicVolume * AudioClipParams.BASE_VOLUME;
    }

    public AudioSource GetAvailableSource()
    {
        // On vérifie si celles déjà utilisées ont finit d'être utilisées
        for (int i = 0; i < usedSources.Count; i++)
        {
            AudioSource source = usedSources[i];
            if (source == null)
            {
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
            if (source.isPlaying == false)
            {
                availableSources.Add(source);
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
        }

        // Si on en a déjà de disponible, c'est cool ! :D
        if (availableSources.Count > 0)
        {
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

    public void PlayJumpClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.jumpClips, pos);
    }
    public void PlayLandClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.landClips, pos);
    }
    public void PlayGripClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.gripClips, pos);
    }
    public void PlayCreateCubeClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.createCubeClips, pos);
    }
    public void PlayFailActionClip()
    {
        PlayClipsOnSource(sounds.failActionClips);
    }
    public void PlayTimeOutClip()
    {
        PlayClipsOnSource(sounds.timeOutClips);
    }
    public void PlayReceivedMessageClip()
    {
        PlayClipsOnSource(sounds.receivedMessageClips);
    }
    public void PlayNewBlockClip()
    {
        PlayClipsOnSource(sounds.newBlockClips);
    }
    public void PlayEventStartClip()
    {
        PlayClipsOnSource(sounds.eventStartClips);
    }
    public void PlayStartSpikeEventClip(Vector3 pos) {
        PlayClipsOnSource(sounds.randomSpikeEventClips, pos: pos);
    }
    public void PlayEventEndClip()
    {
        // reverse !
        PlayClipsOnSource(sounds.eventStartClips);
    }
    public void PlayJumpEventStartClip(float duration)
    {
        PlayClipsOnSource(sounds.jumpEventStartClips, duration: duration);
    }
    public void PlayJumpSuccessClip()
    {
        PlayClipsOnSource(sounds.jumpSuccessClips);
    }
    public void PlayDetectionClip(Vector3 pos, Transform parent)
    {
        //source.spatialBlend = 0.5f;
        PlayClipsOnSource(sounds.detectionClips, pos, parent);
    }
    public void PlayDissimuleClip()
    {
        PlayClipsOnSource(sounds.dissimuleClips, acceleration: 1.2f);
    }
    public AudioSource PlayEmissionTracerClip(Vector3 pos, Transform parent)
    {
        //source.spatialBlend = 1.0f;
        AudioSource source = PlayClipsOnSource(sounds.emissionTracerClips, pos, parent);
        //StartCoroutine(StopClipIn(source, duree));
        return source;
    }
    public void PlayFirstBossPresenceClip(Vector3 pos, Transform parent, Vector2 volumeRange) {
        AudioSource source = PlayClipsOnSource(sounds.firstBossPresenceClips, pos, parent);
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = volumeRange.x;
        source.maxDistance = volumeRange.y;
    }
    public void PlayHitClip(Vector3 pos)
    {
        // reverse !
        PlayClipsOnSource(sounds.hitClips, pos);
    }
    public void PlayTracerHitClip(Vector3 pos, Transform parent)
    {
        PlayClipsOnSource(sounds.tracerHitClips, pos, parent);
    }
    public void PlayTracerInterruptedClip(Vector3 pos, Transform parent)
    {
        PlayClipsOnSource(sounds.tracerInterruptedClips, pos, parent);
    }
    public AudioSource PlayTracerBlastLoadClip(Vector3 pos, float duration) {
        return PlayClipsOnSource(sounds.tracerBlastLoadClips, pos, null, duration);
    }
    public void PlayDecreasingBallFirstBoss(Vector3 pos, float duration)
    {
        PlayClipsOnSource(sounds.firstBossDecreasingBall, pos, null, duration);
    }
    public void PlayIncreasingBallFirstBoss(Vector3 pos, float duration)
    {
        PlayClipsOnSource(sounds.firstBossIncreasingBall, pos, null, duration);
    }
    public void PlayGetLumiereClip(Vector3 pos)
    {
        //source.spatialBlend = 1.0f;
        PlayClipsOnSource(sounds.getLumiereClips, pos);
    }
    public void PlayEscapeLumiereRecoverLife(Vector3 pos) {
        PlayClipsOnSource(sounds.escapeDataRecoverLifeClips, pos);
    }
    public void PlayGetItemClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.getItemClips, pos);
    }
    public void PlayJumpEventStunClip()
    {
        PlayClipsOnSource(sounds.jumpEventStunClips);
    }
    public void PlayJumpEventUnStunClip()
    {
        PlayClipsOnSource(sounds.jumpEventUnStunClips);
    }
    public void PlayIntersectionEventCreateCubesClip(Vector3 position, float duration, Transform parent)
    {
        PlayClipsOnSource(sounds.intersectionEventCreateCubesClips, pos: position, duration: duration, parent: parent);
    }
    public void PlayIntersectionEventSwapCubesClip(Vector3 position, float duration, Transform parent)
    {
        PlayClipsOnSource(sounds.intersectionEventSwapCubesClips, pos: position, duration: duration, parent: parent);
    }
    public void PlayActivationPouvoirClip(AudioClipParams audioClip = null) {
        AudioClipParams clip = (audioClip == null) ? sounds.activationPouvoirClips : audioClip;
        PlayClipsOnSource(clip);
    }
    public void PlayActivationPouvoirReversedClip(AudioClipParams audioClip = null) {
        AudioClipParams clip = (audioClip == null) ? sounds.activationPouvoirClips : audioClip;
        PlayClipsOnSource(clip);
    }
    public void PlayDeniedPouvoirClip()
    {
        PlayClipsOnSource(sounds.deniedPouvoirClips);
    }
    public void PlayNotFoundPouvoirClip()
    {
        PlayClipsOnSource(sounds.notFoundPouvoirClips);
    }
    public void PlayGainPouvoirClip() {
        PlayClipsOnSource(sounds.gainPouvoir);
    }
    public void PlayPowerDashImpactClip() {
        PlayClipsOnSource(sounds.powerDashImpact);
    }
    public void PlayPouvoirAvailableClip()
    {
        PlayClipsOnSource(sounds.pouvoirAvailableClips);
    }
    public void PlayTimeZoneButtonInClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.timeZoneButtonInClips, pos);
    }
    public void PlayTimeZoneButtonOutClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.timeZoneButtonOutClips, pos);
    }
    public void PlayOrbTriggerActivationClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.orbTriggerActivationClips, pos);
    }
    public void PlayRewardBestScore()
    {
        PlayClipsOnSource(sounds.rewardBestScoreClips);
    }
    public void PlayVictoryClip()
    {
        PlayClipsOnSource(sounds.victoryClips);
    }
    public void PlayDefeatClip()
    {
        PlayClipsOnSource(sounds.defeatClips);
    }
    public void PlayBounceClip()
    {
        PlayClipsOnSource(sounds.bounceClips);
    }
    public void PlayBounceWithJumpClip()
    {
        PlayClipsOnSource(sounds.bounceWithJumpClips);
    }
    public void PlayVoidCubeExplosionClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.voidCubeExplosionClips, pos);
    }
    public void PlayGravityChangeClip()
    {
        PlayClipsOnSource(sounds.gravityChangeClips);
    }
    public void PlayGeneratorGeneratesClip(Vector3 pos) {
        PlayClipsOnSource(sounds.generatorGeneratesClips, pos);
    }
    public void PlayCatchSoulRobberClip(Vector3 pos)
    {
        PlayClipsOnSource(sounds.catchSoulRobber, pos);
    }
    public void PlayTransitionSoundClip() {
        PlayClipsOnSource(sounds.transitionSoundsClips);
    }
    public void PlayLogoutStartedClip() {
        PlayClipsOnSource(sounds.logoutStartedClips);
    }


    protected AudioSource PlayClipsOnSource(
        AudioClipParams audioClipParams,
        Vector3 pos = new Vector3(),
        Transform parent = null,
        float duration = -1.0f,
        float acceleration = 1.0f,
        AudioSource sourceToUse = null,
        int clipIndice = -1,
        float avancementTime = -1)
    {

        // On get la source
        AudioSource source = sourceToUse ?? GetAvailableSource();
        if(source == null) {
            Debug.Log($"SOURCE NUL OMG !!! XD");
        }

        // On set le volume
        if (audioClipParams.bIsMusic)
        {
            source.volume = musicVolume;
        }
        else
        {
            source.volume = soundVolume;
        }
        source.volume *= AudioClipParams.BASE_VOLUME * audioClipParams.relativeVolume;

        // On positionne la source
        source.transform.position = pos;
        source.transform.SetParent((parent == null) ? globalSoundsFolder : parent);

        // On get le bon clip
        AudioClip clip = clipIndice == -1 ? MathTools.ChoseOne(audioClipParams.clips) : audioClipParams.clips[clipIndice];
        source.clip = clip;
        source.GetComponent<AudioClipParamsHolder>().clipParams = audioClipParams;

        // On vérifie si c'est reverse ou pas
        if (audioClipParams.bReverse)
        {
            source.timeSamples = source.clip.samples - 1;
            source.pitch = -1;
        }
        else
        {
            source.timeSamples = 0;
            source.pitch = 1;
        }

        if(avancementTime != -1) {
            source.time = avancementTime;
        }

        // On ajuste le pitch pour matcher avec la duration si celle-ci est spécifiée ! :)
        if (duration == -1) {
            source.pitch = source.pitch / Mathf.Abs(source.pitch); // Normalize à 1
        } else {
            float scaledDuration = duration / Time.timeScale;
            float accel = clip.length / scaledDuration;
            source.pitch *= accel;
        }
        source.pitch *= acceleration;

        // On set le spatial blend !
        source.spatialBlend = audioClipParams.spatialBlend;

        // On vérifie si ça loop ou pas !
        source.loop = audioClipParams.bLoop;

        // Et enfin on joue la source ! <3
        source.Play();

        return source;
    }

    protected IEnumerator StopClipIn(AudioSource source, float duree)
    {
        yield return new WaitForSeconds(duree);
        source.Stop();
    }

    protected void StopUIMusic()
    {
        UISoundManager.Instance.StopMusic();
    }

    public void PauseSounds() {
        foreach (AudioSource source in usedSources) {
            source.Pause();
            pausedSources.Add(source);
        }
        usedSources.Clear();
    }

    public void UnPauseSounds() {
        foreach (AudioSource source in pausedSources) {
            source.UnPause();
            usedSources.Add(source);
        }
        pausedSources.Clear();
    }

    protected void StartLevelMusic() {
        Assert.AreNotEqual(levelMusics.Count, 0);
        levelMusic = MathTools.ChoseOne(levelMusics);
        levelMusicSource = PlayClipsOnSource(levelMusic, clipIndice: 0);
        usedSources.Remove(levelMusicSource);
    }

    public void PlayNewLevelMusicVariation(int variationIndice) {
        PlayTransitionSoundClip();

        int newVariationIndice = Mathf.Min(variationIndice, levelMusic.clips.Count - 1);
        int currentVariationIndice = levelMusic.clips.IndexOf(levelMusicSource.clip);
        if (newVariationIndice == currentVariationIndice || gm.eventManager.IsEndGameStarted()) {
            return;
        }
        float avancementTime = levelMusicSource.time;
        finishingLevelMusicSource = levelMusicSource;
        levelMusicSource = GetAvailableSource();
        usedSources.Remove(levelMusicSource);
        PlayClipsOnSource(levelMusic, sourceToUse: levelMusicSource, clipIndice: newVariationIndice, avancementTime: avancementTime);
        FadeOutSourceIn(finishingLevelMusicSource, variationTransitionDuration, variationTransitionOffset);
        FadeInSourceIn(levelMusicSource, variationTransitionDuration, variationTransitionOffset);
    }

    protected void FadeOutSourceIn(AudioSource source, float duration, float waitDuration = 0) {
        StartCoroutine(CFadeOutSource(source, duration, waitDuration));
    }

    protected IEnumerator CFadeOutSource(AudioSource source, float duration, float waitDuration) {
        yield return new WaitForSeconds(waitDuration);
        float maxVolume = source.volume;
        Timer timer = new Timer(duration);
        while(!timer.IsOver()) {
            float newVolume = maxVolume * (1 - timer.GetAvancement());
            source.volume = newVolume;
            yield return null;
        }
        source.volume = 0.0f;
        source.Stop();
        if (!availableSources.Contains(source)) {
            availableSources.Add(source);
        }
    }

    protected void FadeInSourceIn(AudioSource source, float duration, float waitDuration = 0) {
        StartCoroutine(CFadeInSource(source, duration, waitDuration));
    }

    protected IEnumerator CFadeInSource(AudioSource source, float duration, float waitDuration) {
        yield return new WaitForSeconds(waitDuration);
        float maxVolume = source.volume;
        Timer timer = new Timer(duration);
        while(!timer.IsOver()) {
            float newVolume = maxVolume * timer.GetAvancement();
            source.volume = newVolume;
            yield return null;
        }
        source.volume = maxVolume;
    }

    public void PlayEndGameMusic() {
        finishingLevelMusicSource = levelMusicSource;
        levelMusicSource = GetAvailableSource();
        usedSources.Remove(levelMusicSource);
        PlayClipsOnSource(sounds.endGameMusics, sourceToUse: levelMusicSource);
        FadeOutSourceIn(finishingLevelMusicSource, endGameTransitionDuration);
        FadeInSourceIn(levelMusicSource, endGameTransitionDuration);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UISoundManager : MonoBehaviour {
    static UISoundManager _instance;
    public static UISoundManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<UISoundManager>()); } }

    protected SoundBankUI sounds = null;
    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;
    protected float musicVolume = 1.0f;
    protected float soundVolume = 1.0f;
    protected float archivesMultiplicativVolume = 14.0f;
    protected float archivesMinRelativVolume = 0.1f;
    protected float musicWhenArchivesMultiplicativVolume = 0.1f;
    protected float musicWhenArchivesVolumeTransitionDuration = 1.0f;
    protected AudioSource musicAudioSource;

    protected AudioSource archivesAudioSource;
    protected Coroutine stopArchivesCoroutine = null;
    protected Fluctuator musicVolumeFluctuator;
    protected bool usingInitializationArchives = false;

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "UISoundManager";
        globalSoundsFolder = new GameObject("Sounds").transform;
        globalSoundsFolder.parent = transform;
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        musicVolumeFluctuator = new Fluctuator(this, GetCurrentMusicVolume, SetCurrentMusicVolume);
        GetAudioVolumes();
        DontDestroyOnLoad(this);
        StartCoroutine(CInitialize());
    }

    public void GetAudioVolumes() {
        musicVolume = PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME_KEY, MenuOptions.defaultMusicVolume);
        soundVolume = PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME_KEY, MenuOptions.defaultSoundVolume);
    }

    protected IEnumerator CInitialize() {
        AsyncOperationHandle<SoundBankUI> handle = Addressables.LoadAssetAsync<SoundBankUI>("Assets/Prefabs/Sound/SoundBankUI.asset");
        yield return handle;
        sounds = handle.Result;
    }

    public AudioSource GetAvailableSource() {
        // On vérifie si celles déjà utilisées ont finit d'être utilisées
        for (int i = 0; i < usedSources.Count; i++) {
            AudioSource source = usedSources[i];
            if (source == null) {
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
        if (availableSources.Count > 0) {
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

    protected AudioSource PlayClipsOnSource(
        AudioClipParams audioClipParams,
        Vector3 pos = new Vector3(),
        Transform parent = null,
        float duration = -1.0f) {

        // On get la source
        AudioSource source = GetAvailableSource();

        // On set le volume
        if(audioClipParams.bIsMusic) {
            source.volume = GetMusicVolumeForClipParams(audioClipParams);
        } else {
            source.volume = GetSoundVolumeForClipParams(audioClipParams);
        }

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

    protected float GetMusicVolumeForClipParams(AudioClipParams clipParams) {
        return AudioClipParams.BASE_VOLUME * musicVolume * clipParams.relativeVolume;
    }

    protected float GetSoundVolumeForClipParams(AudioClipParams clipParams) {
        return AudioClipParams.BASE_VOLUME * soundVolume * clipParams.relativeVolume;
    }

    public AudioSource GetAudioSourceFor(AnimationClip clip) {
        return usedSources.Find(s => s.clip == clip);
    }

    public AudioSource GetAudioSourceFor(AudioClipParams clipParams) {
        return usedSources.Find(s => clipParams.clips.Contains(s.clip));
    }

    public void PlayHoverButtonClip() {
        PlayClipsOnSource(sounds.hoverButtonClips);
    }
    public void PlayHoverButtonClipWithVolume(float relativeVolume) {
        AudioSource audioSource = PlayClipsOnSource(sounds.hoverButtonClips);
        audioSource.volume *= relativeVolume;
    }
    public void PlayClickedButtonClip() {
        PlayClipsOnSource(sounds.clickedButtonClips);
    }
    public void PlayUnlockPathClip() {
        PlayClipsOnSource(sounds.unlockPathClips);
    }
    public void AccelerateUnlockPathClip(float acceleration) {
        AudioSource source = GetAudioSourceFor(sounds.unlockPathClips);
        if(source != null) {
            source.pitch *= acceleration;
        }
    }
    public void PlayNewBestScoreClip() {
        PlayClipsOnSource(sounds.newBestScoreClips);
    }
    public void PlayNewBestScoreClickedClip() {
        PlayClipsOnSource(sounds.newBestScoreClickedClips);
    }
    public void PlayBlockPassedClip() {
        PlayClipsOnSource(sounds.blockPassedClips);
    }

    public void StartMusic() {
        StartCoroutine(CStartMusic());
    }
    public IEnumerator CStartMusic() {
        yield return new WaitUntil(() => sounds != null);
        if (musicAudioSource == null) {
            musicAudioSource = PlayClipsOnSource(sounds.menuMusicClips);
            availableSources.Remove(musicAudioSource);
            usedSources.Remove(musicAudioSource);
        } else if (!musicAudioSource.isPlaying) {
            musicAudioSource.Play();
        } else if (musicAudioSource.isPlaying && musicAudioSource.clip != sounds.menuMusicClips.clips[0]) {
            musicAudioSource.Stop();
            //usedSources.Remove(musicAudioSource);
            //availableSources.Remove(musicAudioSource); // Just au cas où x)
            musicAudioSource = PlayClipsOnSource(sounds.menuMusicClips);
            availableSources.Remove(musicAudioSource);
            usedSources.Remove(musicAudioSource);
        }
    }

    public void PlayCreditsMusic() {
        StartCoroutine(CPlayCreditsMusic());
    }
    protected IEnumerator CPlayCreditsMusic() {
        yield return new WaitUntil(() => sounds != null);
        if (musicAudioSource != null && musicAudioSource.isPlaying) {
            musicAudioSource.Stop();
            usedSources.Remove(musicAudioSource);
            availableSources.Remove(musicAudioSource); // Just au cas où x)
        }
        musicAudioSource = PlayClipsOnSource(sounds.creditsMusicClips);
        availableSources.Remove(musicAudioSource);
        usedSources.Remove(musicAudioSource);
    }

    public void StopMusic() {
        if (musicAudioSource != null) {
            musicAudioSource.Stop();
        }
    }

    public void UpdateMusicVolume() {
        if (musicAudioSource != null) {
            float musicSourceRelativeVolume = musicAudioSource.GetComponent<AudioClipParamsHolder>().clipParams.relativeVolume;
            musicAudioSource.volume = musicSourceRelativeVolume * musicVolume * AudioClipParams.BASE_VOLUME;
        }
    }

    public float GetMusicVolume() {
        return musicVolume;
    }

    public void SetMusicVolume(float value) {
        musicVolume = value;
        UpdateMusicVolume();
    }

    public void PlayArchivesClip(AudioClipParams clip, bool usingInitializationArchives) {
        this.usingInitializationArchives = usingInitializationArchives;
        StopArchivesClip();
        archivesAudioSource = PlayClipsOnSource(clip);
        usedSources.Remove(archivesAudioSource);
        AdjustArchivesMusicVolume(clip);
        DecreaseMusicVolume();

        StopPlayArchivesIn(archivesAudioSource.clip.length);
    }

    protected void DecreaseMusicVolume() {
        float newMusicVolume = GetMusicVolumeForClipParams(musicAudioSource.GetComponent<AudioClipParamsHolder>().clipParams) * musicWhenArchivesMultiplicativVolume;
        musicVolumeFluctuator.GoTo(newMusicVolume, musicWhenArchivesVolumeTransitionDuration);
    }

    protected void IncreaseMusicVolume() {
        float newMusicVolume = GetMusicVolumeForClipParams(musicAudioSource.GetComponent<AudioClipParamsHolder>().clipParams);
        musicVolumeFluctuator.GoTo(newMusicVolume, musicWhenArchivesVolumeTransitionDuration);
    }

    protected void AdjustArchivesMusicVolume(AudioClipParams clip) {
        float archivesMinVolume = archivesMinRelativVolume * AudioClipParams.BASE_VOLUME * clip.relativeVolume;
        archivesAudioSource.volume = Mathf.Max(archivesAudioSource.volume, archivesMinVolume);
        archivesAudioSource.volume *= archivesMultiplicativVolume;
    }

    protected void StopPlayArchivesIn(float duration) {
        if (stopArchivesCoroutine == null) {
            stopArchivesCoroutine = StartCoroutine(CStopPlayArchivesIn(duration));
        }
    }

    protected IEnumerator CStopPlayArchivesIn(float duration) {
        yield return new WaitForSeconds(duration);
        StopArchivesClip();
        SelectorManager.Instance.popupArchives.GetComponentInChildren<PausePlayArchivesClipButtonsInitializer>().SetPlayButtonVisible(); // We want this to only trigger when we reach the end of the clip, not when we stop it manually :)
        stopArchivesCoroutine = null;
    }

    public void StopArchivesClip() {
        if (archivesAudioSource != null) {
            archivesAudioSource.Stop();
            archivesAudioSource = null;
            IncreaseMusicVolume();
        }
        StopStopArchivesCoroutine();
    }

    protected void StopStopArchivesCoroutine() {
        if (stopArchivesCoroutine != null) {
            StopCoroutine(stopArchivesCoroutine);
            stopArchivesCoroutine = null;
        }
    }

    public float GetCurrentMusicVolume() {
        return musicAudioSource?.volume ?? 0;
    }

    public void SetCurrentMusicVolume(float newVolume) {
        if (musicAudioSource != null) {
            musicAudioSource.volume = newVolume;
        }
    }

    public void PauseArchivesClip() {
        if (archivesAudioSource != null) {
            archivesAudioSource.Pause();
            IncreaseMusicVolume();
            StopStopArchivesCoroutine();
        }
    }

    public void UnPauseArchivesClip() {
        if (archivesAudioSource != null) {
            archivesAudioSource.UnPause();
            DecreaseMusicVolume();
            float stopIn = archivesAudioSource.clip.length - archivesAudioSource.time;
            StopPlayArchivesIn(stopIn);
        } else {
            SelectorManager sm = SelectorManager.Instance;
            MenuLevel menuLevel = sm.GetCurrentLevel().menuLevel;
            if (!usingInitializationArchives) {
                PlayArchivesClip(menuLevel.archivesClip, usingInitializationArchives: usingInitializationArchives);
            } else {
                SelectorLevelRunIntroduction introductionRunner = sm.HasSelectorLevelOpen() ? menuLevel.GetComponent<SelectorLevelRunIntroduction>() : sm.introductionRunner;
                PlayArchivesClip(introductionRunner.archivesClip, usingInitializationArchives: usingInitializationArchives);
            }
        }
    }
}

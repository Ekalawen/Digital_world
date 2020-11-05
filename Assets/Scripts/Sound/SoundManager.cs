using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioClipParams jumpClips;
    public AudioClipParams landClips;
    public AudioClipParams gripClips;
    public AudioClipParams createCubeClips;
    public AudioClipParams getLumiereClips;
    public AudioClipParams getItemClips;
    public AudioClipParams failActionClips;
    public AudioClipParams hitClips;
    public AudioClipParams hitTracerClips;
    public AudioClipParams emissionTracerClips;
    public AudioClipParams firstBossPresenceClips;
    public AudioClipParams firstBossDecreasingBall;
    public AudioClipParams firstBossIncreasingBall;
    public AudioClipParams detectionClips;
    public AudioClipParams dissimuleClips;
    public AudioClipParams timeOutClips;
    public AudioClipParams receivedMessageClips;
    public AudioClipParams newBlockClips;
    public AudioClipParams eventStartClips;
    public AudioClipParams eventEndClips;
    public AudioClipParams jumpSuccessClips;
    public AudioClipParams jumpEventStunClips;
    public AudioClipParams jumpEventUnStunClips;
    public AudioClipParams activationPouvoirClips;
    public AudioClipParams deniedPouvoirClips;
    public AudioClipParams notFoundPouvoirClips;
    public AudioClipParams timeZoneButtonInClips;
    public AudioClipParams timeZoneButtonOutClips;
    public AudioClipParams rewardBestScoreClips;
    public AudioClipParams victoryClips;
    public AudioClipParams defeatClips;

    public AudioClipParams normalMusics;
    public AudioClipParams endGameMusics;

    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;
    protected AudioSource normalMusicSource;

    public void Initialize() {
        globalSoundsFolder = new GameObject("Sounds").transform;
        //globalSource.volume = PlayerPrefs.GetFloat(MenuOptions.MUSIC_VOLUME_KEY);
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        normalMusicSource = PlayClipsOnSource(normalMusics);
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
    public void PlayNewBlockClip() {
        PlayClipsOnSource(newBlockClips);
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
    public void PlayDissimuleClip() {
        PlayClipsOnSource(dissimuleClips);
    }
    public AudioSource PlayEmissionTracerClip(Vector3 pos, Transform parent) {
        //source.spatialBlend = 1.0f;
        AudioSource source = PlayClipsOnSource(emissionTracerClips, pos, parent);
        //StartCoroutine(StopClipIn(source, duree));
        return source;
    }
    public void PlayFirstBossPresenceClip(Vector3 pos, Transform parent) {
        PlayClipsOnSource(firstBossPresenceClips, pos, parent);
    }
    public void PlayHitClip(Vector3 pos) {
        // reverse !
        PlayClipsOnSource(hitClips, pos);
    }
    public void PlayHitTracerClip(Vector3 pos, Transform parent) {
        PlayClipsOnSource(hitTracerClips, pos, parent);
    }
    public void PlayDecreasingBallFirstBoss(Vector3 pos, float duration) {
        PlayClipsOnSource(firstBossDecreasingBall, pos, null, duration);
    }
    public void PlayIncreasingBallFirstBoss(Vector3 pos, float duration) {
        PlayClipsOnSource(firstBossIncreasingBall, pos, null, duration);
    }
    public void PlayGetLumiereClip(Vector3 pos) {
        //source.spatialBlend = 1.0f;
        PlayClipsOnSource(getLumiereClips, pos);
    }
    public void PlayGetItemClip(Vector3 pos) {
        PlayClipsOnSource(getItemClips, pos);
    }
    public void PlayEndGameMusic() {
        normalMusicSource.Stop();
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
    public void PlayTimeZoneButtonInClip(Vector3 pos) {
        PlayClipsOnSource(timeZoneButtonInClips, pos);
    }
    public void PlayTimeZoneButtonOutClip(Vector3 pos) {
        PlayClipsOnSource(timeZoneButtonOutClips, pos);
    }
    public void PlayRewardBestScore() {
        PlayClipsOnSource(rewardBestScoreClips);
    }
    public void PlayVictoryClip() {
        PlayClipsOnSource(victoryClips);
    }
    public void PlayDefeatClip() {
        PlayClipsOnSource(defeatClips);
    }

    //protected void PlayClipsOnSource(List<AudioClip> clips, AudioSource source, bool bReverse = false) {
    protected AudioSource PlayClipsOnSource(
        AudioClipParams audioClipParams,
        Vector3 pos = new Vector3(),
        Transform parent = null,
        float duration = -1.0f) {

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
        AudioClip clip = audioClipParams.clips[Random.Range(0, audioClipParams.clips.Count)];
        source.clip = clip;

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
}

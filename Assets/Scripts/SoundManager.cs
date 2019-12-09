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

    public List<AudioClip> normalMusics;
    public List<AudioClip> endGameMusics;

    public AudioSource globalSource;
    public AudioSource instantSource;

    public void Initialize() {
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
    public void PlayHitClip(AudioSource source) {
        if(!source.isPlaying)
            PlayClipsOnSource(hitClips, source);
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

    protected void PlayClipsOnSource(List<AudioClip> clips, AudioSource source) {
        AudioClip clip = clips[Random.Range(0, clips.Count)];
            source.clip = clip;
        source.Play();
    }

}

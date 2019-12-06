using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public List<AudioClip> jumpClips;
    public List<AudioClip> landClips;
    public List<AudioClip> gripClips;

    public void PlayJumpClip(AudioSource source) {
        PlayClipsOnSource(jumpClips, source);
    }
    public void PlayLandClip(AudioSource source) {
        PlayClipsOnSource(landClips, source);
    }
    public void PlayGripClip(AudioSource source) {
        PlayClipsOnSource(gripClips, source);
    }

    protected void PlayClipsOnSource(List<AudioClip> clips, AudioSource source) {
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        source.clip = clip;
        source.Play();
    }

}

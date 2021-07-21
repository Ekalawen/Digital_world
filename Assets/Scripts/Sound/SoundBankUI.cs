using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBankUI", menuName = "SoundBankUI")]
public class SoundBankUI : ScriptableObject {
    public AudioClipParams hoverButtonClips;
    public AudioClipParams clickedButtonClips;

    public AudioClipParams unlockPathClips;

    public AudioClipParams menuMusicClips;
}

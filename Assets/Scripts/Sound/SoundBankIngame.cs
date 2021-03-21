using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBankIngame", menuName = "SoundBankIngame")]
public class SoundBankIngame : ScriptableObject {
    public AudioClipParams jumpClips;
    public AudioClipParams landClips;
    public AudioClipParams gripClips;
    public AudioClipParams createCubeClips;
    public AudioClipParams getLumiereClips;
    public AudioClipParams getItemClips;
    public AudioClipParams failActionClips;
    public AudioClipParams hitClips;
    public AudioClipParams tracerHitClips;
    public AudioClipParams tracerBlastLoadClips;
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
    public AudioClipParams jumpEventStartClips;
    public AudioClipParams jumpSuccessClips;
    public AudioClipParams jumpEventStunClips;
    public AudioClipParams jumpEventUnStunClips;
    public AudioClipParams intersectionEventClips;
    public AudioClipParams activationPouvoirClips;
    public AudioClipParams deniedPouvoirClips;
    public AudioClipParams notFoundPouvoirClips;
    public AudioClipParams pouvoirAvailableClips;
    public AudioClipParams timeZoneButtonInClips;
    public AudioClipParams timeZoneButtonOutClips;
    public AudioClipParams bounceClips;
    public AudioClipParams rewardBestScoreClips;
    public AudioClipParams victoryClips;
    public AudioClipParams defeatClips;

    public AudioClipParams normalMusics;
    public AudioClipParams endGameMusics;
}

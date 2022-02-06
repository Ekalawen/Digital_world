﻿using System.Collections;
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
    public AudioClipParams catchSoulRobber;
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
    public AudioClipParams gainDash333;
    public AudioClipParams powerDashImpact;

    public AudioClipParams timeZoneButtonInClips;
    public AudioClipParams timeZoneButtonOutClips;
    public AudioClipParams orbTriggerActivationClips;
    public AudioClipParams generatorGeneratesClips;

    public AudioClipParams bounceClips;
    public AudioClipParams bombCubeExplosionClips;
    public AudioClipParams gravityChangeClips;

    public AudioClipParams rewardBestScoreClips;
    public AudioClipParams victoryClips;
    public AudioClipParams defeatClips;

    public AudioClipParams levelMusics;
    public AudioClipParams endGameMusics;
    public AudioClipParams transitionSoundsClips;
}

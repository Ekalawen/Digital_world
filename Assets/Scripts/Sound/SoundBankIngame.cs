using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBankIngame", menuName = "SoundBankIngame")]
public class SoundBankIngame : ScriptableObject {
    [Header("Main Sounds")]
    public AudioClipParams jumpClips;
    public AudioClipParams landClips;
    public AudioClipParams shiftLandingClips;
    public AudioClipParams gripClips;

    public AudioClipParams timeOutClips;

    public AudioClipParams receivedMessageClips;
    public AudioClipParams newBlockClips;
    public AudioClipParams logoutStartedClips;

    public AudioClipParams rewardBestScoreClips;
    public AudioClipParams victoryClips;
    public AudioClipParams defeatClips;


    [Header("Map Elements")]
    public AudioClipParams createCubeClips;
    public AudioClipParams getLumiereClips;
    public AudioClipParams escapeDataRecoverLifeClips;
    public AudioClipParams getItemClips;

    public AudioClipParams timeZoneButtonInClips;
    public AudioClipParams timeZoneButtonOutClips;
    public AudioClipParams orbTriggerActivationClips;
    public AudioClipParams generatorGeneratesClips;

    public AudioClipParams bounceClips;
    public AudioClipParams bounceWithJumpClips;
    public AudioClipParams voidCubeExplosionClips;
    public AudioClipParams gravityChangeClips;
    public AudioClipParams corruptedCubeLoad;
    public AudioClipParams corruptedCubeBlast;


    [Header("Ennemis")]
    public AudioClipParams detectionClips;
    public AudioClipParams dissimuleClips;

    public AudioClipParams hitClips;

    public AudioClipParams tracerHitClips;
    public AudioClipParams tracerBlastLoadClips;
    public AudioClipParams tracerInterruptedClips;
    public AudioClipParams emissionTracerClips;

    public AudioClipParams firstBossPresenceClips;
    public AudioClipParams firstBossDecreasingBall;
    public AudioClipParams firstBossIncreasingBall;

    public AudioClipParams catchSoulRobber;

    [Header("Events")]
    public AudioClipParams eventStartClips;
    public AudioClipParams eventEndClips;

    public AudioClipParams jumpEventStartClips;
    public AudioClipParams jumpSuccessClips;
    public AudioClipParams jumpEventStunClips;
    public AudioClipParams jumpEventUnStunClips;

    public AudioClipParams intersectionEventPrevisualizationClips;
    public AudioClipParams intersectionEventCreateCubesClips;
    public AudioClipParams intersectionEventSwapCubesClips;

    public AudioClipParams randomSpikeEventClips;

    [Header("Powers")]
    public AudioClipParams activationPouvoirClips;
    public AudioClipParams failActionClips; // Le pouvoir ne peut pas être lancé dans ces conditions
    public AudioClipParams deniedPouvoirClips; // Le pouvoir n'est pas disponible (probablement en cooldown)
    public AudioClipParams notFoundPouvoirClips; // Le pouvoir n'existe pas
    public AudioClipParams pouvoirAvailableClips;
    public AudioClipParams gainPouvoir;
    public AudioClipParams powerDashImpact;

    [Header("Musics")]
    public AudioClipParams levelMusics;
    public AudioClipParams endGameMusics;
    public AudioClipParams transitionSoundsClips;
}

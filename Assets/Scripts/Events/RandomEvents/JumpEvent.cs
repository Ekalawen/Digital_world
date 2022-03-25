using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class JumpEvent : RandomEvent {

    [Header("Delais")]
    public float delaisAvantJump = 1.5f;

    [Header("Malus")]
    public float timeMalus = 10f;
    public float dureeStun = 5.0f;
    public bool shouldDisplayJumpRules = false;

    [Header("ScreenShake")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 5;

    [Header("Flash")]
    public float dureeFadeInFlash = 0.1f;
    public float dureeFadeOutFlash = 0.1f;
    public float intensityFlash = 2.0f;

    [Header("LightningBox")]
    public GameObject lightningBoxPrefab;
    public float dureeFadeOutLightning = 0.5f;

    protected List<Coroutine> coroutines = new List<Coroutine>();

    protected override void StartEvent() {
        coroutines.Clear();
        coroutines.Add(StartCoroutine(JumpEffect()));
    }

    protected override void EndEvent() {
    }

    protected override void StartEventConsoleMessage() {
        coroutines.Add(StartCoroutine(AfficherMessagesPreventifs()));
    }

    protected IEnumerator JumpEffect() {
        InstantiateLightningBox();
        bEventIsOn = true;
        yield return new WaitForSeconds(delaisAvantJump - dureeFadeInFlash);
        // There is now collisions when making 2 jumps succeed each other because of the previsualisation of 2s whish is > than the dureeFadeIn + dureeFadeOut !
        // But if they are 2 RandomJumpsEvent at the same time, it could collide and break intensity ! :)
        gm.postProcessManager.MakePostExposureForJumpBounce(intensityFlash, dureeFadeInFlash, dureeFadeOutFlash);
        //gm.colorManager.MakeLightIntensityBounce(intensityFlash, dureeFadeInFlash, dureeFadeOutFlash);
        yield return new WaitForSeconds(dureeFadeInFlash);
        if(gm.player.GetEtat() != Player.EtatPersonnage.EN_CHUTE
        && gm.player.GetEtat() != Player.EtatPersonnage.EN_SAUT) {
            Stun();
        } else {
            SucceedJump();
        }
    }

    protected void SucceedJump() {
        gm.soundManager.PlayJumpSuccessClip();
        gm.eventManager.onJumpSuccess.Invoke();
        bEventIsOn = false;
    }

    protected void InstantiateLightningBox() {
        LightningBox lightningBox = Instantiate(lightningBoxPrefab, parent: transform).GetComponent<LightningBox>();
        lightningBox.Initialize(gm.map.GetCenter(), gm.map.GetHalfExtents(), delaisAvantJump, dureeFadeOutLightning);
    }

    protected virtual void Stun() {
        gm.timerManager.RemoveTime(timeMalus, EventManager.DeathReason.FAILED_JUMP_EVENT);
        gm.player.Stun();

        gm.soundManager.PlayJumpEventStunClip();
        if (shouldDisplayJumpRules) {
            gm.console.JumpStun();
        }

        StartCoroutine(CScreenShake());
        coroutines.Add(StartCoroutine(UnStun()));
        gm.eventManager.onJumpFailed.Invoke();
    }

    protected IEnumerator CScreenShake() {
        CameraShakeInstance cameraShakeInstance = CameraShaker.Instance.StartShake(screenShakeMagnitude, screenShakeRoughness, 0.1f);
        yield return new WaitForSeconds(dureeStun);
        cameraShakeInstance.StartFadeOut(0.1f);
    }

    protected IEnumerator UnStun() {
        yield return new WaitForSeconds(dureeStun);
        gm.player.UnStun();
        bEventIsOn = false;
        gm.soundManager.PlayJumpEventUnStunClip();
    }

    protected IEnumerator AfficherMessagesPreventifs() {
        float dureeAttente = delaisAvantJump / (3f + 1f);
        string precedantMessage = "";
        Timer timer = new Timer(delaisAvantJump);
        while(!timer.IsOver()) {
            AsyncOperationHandle<string> messageHandle = gm.console.strings.jumpTimer.GetLocalizedString(new object[] { TimerManager.TimerToClearerString(timer.GetRemainingTime()) });
            yield return messageHandle;
            gm.console.AjouterMessageImportant(messageHandle.Result, Console.TypeText.RED_TEXT, dureeAttente, bAfficherInConsole: false, precedantMessage);
            precedantMessage = messageHandle.Result;
            yield return null;
        }
        AsyncOperationHandle<string> messageFinalHandle = gm.console.strings.jumpActivation.GetLocalizedString();
        yield return messageFinalHandle;
        gm.console.AjouterMessageImportant(messageFinalHandle.Result, Console.TypeText.RED_TEXT, dureeAttente, bAfficherInConsole: false, precedantMessage);
    }

    public override void StopEvent() {
        foreach(Coroutine coroutine in coroutines) {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }

    protected override void PlayStartSound() {
        gm.soundManager.PlayJumpEventStartClip(delaisAvantJump + 0.4f);
    }

    protected override void SetEventIsDoneOnEndEvent() {
        // Don't do it ! :)
    }
}

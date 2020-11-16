using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEvent : RandomEvent {

    [Header("Delais")]
    public float delaisAvantJump = 1.5f;

    [Header("Malus")]
    public float timeMalus = 10f;
    public float dureeStun = 5.0f;

    [Header("ScreenShake")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 5;

    [Header("Flash")]
    public float dureeFadeFlash = 0.1f;
    public Color colorFlash = Color.white;

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
        yield return new WaitForSeconds(delaisAvantJump - dureeFadeFlash);
        gm.colorManager.MakeAllColorSourcesBounceToColor(colorFlash, dureeFadeFlash, dureeFadeFlash);
        yield return new WaitForSeconds(dureeFadeFlash);
        if(gm.player.GetEtat() != Player.EtatPersonnage.EN_CHUTE
        && gm.player.GetEtat() != Player.EtatPersonnage.EN_SAUT) {
            Stun();
        } else {
            gm.soundManager.PlayJumpSuccessClip();
        }
    }

    protected virtual void Stun() {
        gm.timerManager.AddTime(-timeMalus);
        gm.player.FreezePouvoirs(true);
        gm.player.bIsStun = true;

        gm.soundManager.PlayJumpEventStunClip();

        StartCoroutine(CScreenShake());
        coroutines.Add(StartCoroutine(UnStun()));
    }

    protected IEnumerator CScreenShake() {
        CameraShakeInstance cameraShakeInstance = CameraShaker.Instance.StartShake(screenShakeMagnitude, screenShakeRoughness, 0.1f);
        yield return new WaitForSeconds(dureeStun);
        cameraShakeInstance.StartFadeOut(0.1f);
    }

    protected IEnumerator UnStun() {
        yield return new WaitForSeconds(dureeStun);
        gm.player.FreezePouvoirs(false);
        gm.player.bIsStun = false;
        gm.soundManager.PlayJumpEventUnStunClip();
    }

    protected IEnumerator AfficherMessagesPreventifs() {
        float dureeAttente = delaisAvantJump / (3f + 1f);
        string precedantMessage = "";
        Timer timer = new Timer(delaisAvantJump);
        while(!timer.IsOver()) {
            string message = "JUMP : " + TimerManager.TimerToClearerString(timer.GetRemainingTime());
            gm.console.AjouterMessageImportant(message, Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false, precedantMessage);
            precedantMessage = message;
            yield return null;
        }
        gm.console.AjouterMessageImportant("JUMP !", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false, precedantMessage);
    }

    public override void StopEvent() {
        foreach(Coroutine coroutine in coroutines) {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }
}

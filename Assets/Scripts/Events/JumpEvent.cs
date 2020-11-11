using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEvent : RandomEvent {

    public float delaisAvantJump = 1.5f;
    public float timeMalus = 10f;
    public float dureeStun = 5.0f;

    protected override void StartEvent() {
        StartCoroutine(JumpEffect());
    }

    protected override void EndEvent() {
    }

    protected override void StartEventConsoleMessage() {
        StartCoroutine(AfficherMessagesPreventifs());
    }

    protected IEnumerator JumpEffect() {
        yield return new WaitForSeconds(delaisAvantJump);

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

        StartCoroutine(UnStun());
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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEvent : RandomEvent {

    public float delaisAvantJump = 1.5f;
    public float timeMalus = 10f;
    public float dureeStun = 5.0f;

    public override void StartEvent() {
        StartCoroutine(JumpEffect());
    }

    public override void EndEvent() {
    }

    public override void StartEventConsoleMessage() {
        StartCoroutine(AfficherMessagesPreventifs());
    }

    protected IEnumerator JumpEffect() {
        yield return new WaitForSeconds(delaisAvantJump);

        if(gm.player.GetEtat() != Player.EtatPersonnage.EN_CHUTE
        && gm.player.GetEtat() != Player.EtatPersonnage.EN_SAUT) {

            gm.timerManager.AddTime(-timeMalus);
            gm.player.FreezePouvoirs(true);
            gm.player.bIsStun = true;

            gm.soundManager.PlayHitClip(gm.soundManager.instantSource);

            StartCoroutine(UnStun());
        }
    }

    protected IEnumerator UnStun() {
        yield return new WaitForSeconds(dureeStun);
        gm.player.FreezePouvoirs(false);
        gm.player.bIsStun = false;
        gm.soundManager.PlayHitClip(gm.soundManager.instantSource, priority: false, bReverse: true);
    }

    protected IEnumerator AfficherMessagesPreventifs() {
        float dureeAttente = delaisAvantJump / (3f + 1f);
        gm.console.AjouterMessageImportant("JUMP ...", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false);
        gm.console.AjouterMessage("JUMP !", Console.TypeText.ENNEMI_TEXT);
        yield return new WaitForSeconds(dureeAttente);
        gm.console.AjouterMessageImportant("3", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false);
        yield return new WaitForSeconds(dureeAttente);
        gm.console.AjouterMessageImportant("2", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false);
        yield return new WaitForSeconds(dureeAttente);
        gm.console.AjouterMessageImportant("1", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false);
        yield return new WaitForSeconds(dureeAttente);
        gm.console.AjouterMessageImportant("JUMP !", Console.TypeText.ENNEMI_TEXT, dureeAttente, bAfficherInConsole: false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerBlast : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    [Header("Blast")]
    public float blastLoadDuree = 2.0f;
    public float blastPousseeDuree = 0.3f;
    public float blastPousseeDistance = 10f;

    [Header("Load Animation")]
    public float blastLoadMaxRotation = 10f;
    public AnimationCurve blastLoadRotationCurve;
    public float blastLoadSoundOffset = 0.7f;

    protected Coroutine blastCoroutine = null;
    protected Coroutine blastLoadRotationCoroutine = null;

    public override void Start() {
        base.Start();
    }

    public override void UpdateSpecific() {
    }

	void OnControllerColliderHit(ControllerColliderHit hit) {
        Player player = hit.collider.gameObject.GetComponent<Player>();
        if (player != null) {
            HitPlayer();
        }
    }

    public void StartBlast() {
        blastCoroutine = StartCoroutine(CStartBlast());
    }

    protected IEnumerator CStartBlast() {
        LoadBlast();
        yield return new WaitForSeconds(blastLoadDuree);
        Blast();
    }

    public void LoadBlast() {
        // Start loading animation
        blastLoadRotationCoroutine = StartCoroutine(CRotation());
        gm.soundManager.PlayTracerBlastLoadClip(transform.position, blastLoadDuree + blastLoadSoundOffset);
    }

    protected IEnumerator CRotation() {
        Timer timer = new Timer(blastLoadDuree);
        AutoRotate autoRotate = GetComponent<AutoRotate>();
        while (!timer.IsOver()) {
            autoRotate.vitesse = blastLoadRotationCurve.Evaluate(timer.GetAvancement()) * blastLoadMaxRotation;
            yield return null;
        }

        timer = new Timer(blastPousseeDuree);
        Quaternion startingRotation = transform.rotation;
        while (!timer.IsOver()) {
            transform.rotation = Quaternion.Slerp(startingRotation, Quaternion.identity, timer.GetAvancement());
            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }

    public void Blast() {
        // Start blast animation
        EnnemiController ennemiController = GetComponent<EnnemiController>();
        if(ennemiController != null && ennemiController.IsPlayerVisible()) {
            HitPlayer();
        }
    }
    protected override void HitPlayerSpecific() {
        Vector3 direction = player.transform.position - transform.position;
        Poussee poussee = new Poussee(direction, blastPousseeDuree, blastPousseeDistance);
        player.AddPoussee(poussee);
        player.ResetGrip();
        gm.postProcessManager.UpdateHitEffect();
    }

    protected override void HitContinuousPlayerSpecific() {
    }

    public void StopBlast() {
        if (blastCoroutine != null) {
            StopCoroutine(blastCoroutine);
        }
        if (blastLoadRotationCoroutine != null) {
            StopCoroutine(blastLoadRotationCoroutine);
        }
        AutoRotate autoRotate = GetComponent<AutoRotate>();
        autoRotate.vitesse = 0;
        transform.rotation = Quaternion.identity;
    }

    public override void DisplayHitMessage() {
        if (!gm.eventManager.IsGameOver()) {
            gm.console.JoueurToucheTracer();
        }
    }

    public override EventManager.DeathReason GetDeathReason() {
        return EventManager.DeathReason.TRACER_HIT;
    }
}

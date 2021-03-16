using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TracerBlast : Ennemi {

    public static string SHADER_MAIN_COLOR = "_MainColor";
    public static string SHADER_COLOR_CHANGE_COLOR_SOURCE = "_ColorChangeColorSource";
    public static string SHADER_COLOR_CANCELLED = "_ColorCancelled";

    public enum TracerState { WAITING, RUSHING, EMITING };

    [Header("Blast")]
    public float blastLoadDuree = 2.0f;
    public float blastPousseeDuree = 0.3f;
    public float blastPousseeDistance = 10f;

    [Header("Load Animation")]
    public float blastLoadMaxRotation = 10f;
    public AnimationCurve blastLoadRotationCurve;
    public float blastLoadSoundOffset = 0.7f;
    public VisualEffect loadAndBlastVfx;

    [Header("Detection Animation")]
    public float detectionDureeRotation = 0.4f;

    [Header("Contact Hit")]
    public float timeBetweenTwoContactHits = 0.5f;

    [Header("Cancel Blast")]
    public float dureeCancelAnimation = 0.5f;

    protected TracerController tracerController;
    protected Coroutine blastCoroutine = null;
    protected Coroutine blastLoadRotationCoroutine = null;
    protected Timer timerContactHit;
    protected Material material;
    protected Color shaderLoadColor;
    protected Color shaderMainColor;
    protected Color shaderCancelledColor;
    protected bool isCancelled = false;
    protected AudioSource loadAndBlastSource = null;
    protected Coroutine onDetectPlayerCoroutine = null;

    public override void Start() {
        base.Start();
        tracerController = GetComponent<TracerController>();
        timerContactHit = new Timer(timeBetweenTwoContactHits, setOver: true);
        material = GetComponent<Renderer>().material;
        shaderMainColor = material.GetColor(SHADER_MAIN_COLOR);
        shaderLoadColor = material.GetColor(SHADER_COLOR_CHANGE_COLOR_SOURCE);
        shaderCancelledColor = material.GetColor(SHADER_COLOR_CANCELLED);
    }

    public override void UpdateSpecific() {
        TestForPlayerCollision();
    }

    protected void TestForPlayerCollision() {
        if(MathTools.OBBSphere(transform.position, transform.localScale / 2.0f, transform.rotation, player.transform.position, player.GetSizeRadius() + 0.05f)) {
            if(!isCancelled) {
                if(tracerController != null && IsPlayerComingFromTop()) {
                    tracerController.TryCancelAttack();
                } else if (timerContactHit.IsOver()) {
                    HitPlayer();
                    timerContactHit.Reset();
                }
            }
        }
    }

    protected bool IsPlayerComingFromTop() {
        float playerHeight = gm.gravityManager.GetHigh(player.transform.position);
        float tracerHeight = gm.gravityManager.GetHigh(transform.position);
        float tracerHalfSize = transform.localScale.x / 2;
        return playerHeight > tracerHeight + tracerHalfSize;
    }

    private void OnCollisionEnter(Collision collision) {
        Cube cube = collision.gameObject.GetComponent<Cube>();
        if (cube != null) {
            cube.Explode();
        }
    }

    public void OnDetectPlayer() {
        isCancelled = false;
        onDetectPlayerCoroutine = StartCoroutine(COnDetectPlayer());
    }

    protected IEnumerator COnDetectPlayer() {
        Timer timer = new Timer(detectionDureeRotation);
        while(!timer.IsOver()) {
            float angle = timer.GetAvancement() * 360;
            transform.rotation = Quaternion.AngleAxis(angle, gm.gravityManager.Up());
            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }

    public void OnCancelBlast() {
        isCancelled = true;
        StopBlast();
        if(onDetectPlayerCoroutine != null) {
            StopCoroutine(onDetectPlayerCoroutine);
        }
        StartShaderColorChange(shaderCancelledColor, shaderMainColor, dureeCancelAnimation);
        StartCoroutine(CGoToIdentityRotation(dureeCancelAnimation));
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
        loadAndBlastVfx.SendEvent("Blast");
        StartShaderColorChange(shaderMainColor, shaderLoadColor, blastLoadDuree);
        blastLoadRotationCoroutine = StartCoroutine(CRotation());
        loadAndBlastSource = gm.soundManager.PlayTracerBlastLoadClip(transform.position, blastLoadDuree + blastLoadSoundOffset);
    }

    protected void StartShaderColorChange(Color sourceColor, Color targetColor, float duration) {
        material.SetFloat("_ColorChangeStartingTime", Time.time);
        material.SetFloat("_ColorChangeDuration", duration);
        material.SetColor(SHADER_COLOR_CHANGE_COLOR_SOURCE, sourceColor);
        material.SetColor(SHADER_MAIN_COLOR, targetColor);
    }

    protected IEnumerator CRotation() {
        Timer timer = new Timer(blastLoadDuree);
        AutoRotate autoRotate = GetComponent<AutoRotate>();
        while (!timer.IsOver()) {
            autoRotate.vitesse = blastLoadRotationCurve.Evaluate(timer.GetAvancement()) * blastLoadMaxRotation;
            yield return null;
        }

        yield return CGoToIdentityRotation(blastPousseeDuree);
    }

    protected IEnumerator CGoToIdentityRotation(float duree) {
        Timer timer = new Timer(duree);
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
        StartShaderColorChange(shaderLoadColor, shaderMainColor, blastPousseeDuree);
        if(ennemiController != null && ennemiController.IsPlayerVisible() && !gm.eventManager.IsGameOver()) {
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
        if(loadAndBlastSource != null && loadAndBlastSource.isPlaying) {
            loadAndBlastSource.Stop();
            loadAndBlastSource = null;
        }
        loadAndBlastVfx.SendEvent("StopBlast");
        AutoRotate autoRotate = GetComponent<AutoRotate>();
        autoRotate.vitesse = 0;
        transform.rotation = Quaternion.identity;
        StartShaderColorChange(shaderMainColor, shaderMainColor, 0);
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

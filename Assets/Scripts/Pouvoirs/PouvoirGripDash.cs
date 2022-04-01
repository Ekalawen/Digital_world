using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirGripDash : IPouvoir {

    [Header("Dash")]
    public float dashSpeed = 15.0f;
    public float dashDurationAdjustment = 0.1f;
    public TimeMultiplier dashSlowmotion;

    [Header("Targeting")]
    public float maxTargetingDistance = 15.0f;
    public float maxTargetingDuration = 1.0f;
    public TimeMultiplier targetingSlowmotion;
    public GameObject previsualizationCubePrefab;
    public AnimationCurve previsualizationAlphaCurve;
    public float rechargeTimeAfterFailedTargeting = 1.0f;

    protected RaycastHit currentHit;
    protected Cube currentHittedCube;
    protected Poussee currentPoussee = null;
    protected Coroutine targetingCoroutine = null;
    protected Coroutine unStunCoroutine = null;
    protected Coroutine restoreGravityCoroutine = null;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected float computedDuration = 0;
    protected bool useVerticalArrow = true;
    protected AudioClipParams reversedClip;
    protected NoAutomaticRechargeCooldown cooldownCustom;

    public override void Initialize() {
        base.Initialize();
        reversedClip = new AudioClipParams(activationAudioClips);
        reversedClip.bReverse = true;
        cooldownCustom = GetComponent<NoAutomaticRechargeCooldown>();
        player.onPowerDashImpact.AddListener(GainChargeListener);
    }

    public override bool IsAvailable() {
        return base.IsAvailable() && targetingCoroutine == null;
    }

    protected override void ApplyUsePouvoir() {
        targetingCoroutine = StartCoroutine(CTargeting());
    }

    protected IEnumerator CTargeting()
    {
        Timer timer = new Timer(maxTargetingDuration);
        GameObject previsualizationObject = CreatePrevisualization();
        AddTargetingMultiplier();
        PlayReversedActivationClip();
        SetTargetingPointeur();
        while (!timer.IsOver())
        {
            bool canGripDash = CanGripDash();
            ComputeTargetFromCube(currentHittedCube, currentHit);
            UpdatePrevisualization(previsualizationObject, timer.GetAvancement());
            if (InputManager.Instance.GetKeyUp(binding))
            {
                if (canGripDash)
                {
                    UsePouvoir();
                    ApplyUsePouvoirConsequences();
                }
                else
                {
                    FailUse();
                }
                break;
            }
            yield return null;
        }
        if (timer.IsOver())
        {
            FailUse();
        }
        SetNotTargetingPointeur();
        DestroyPrevisualization(previsualizationObject);
        RemoveTargetingMultiplier();
        // Faudra se débrouiller pour mettre un cooldown de 1s (ou plus) ici ! :)
        targetingCoroutine = null;
    }

    protected void SetNotTargetingPointeur() {
        gm.pointeur.EnableMainPointeur();
        gm.pointeur.SetDefaultGripPointeur();
    }

    protected void SetTargetingPointeur() {
        gm.pointeur.DisableMainPointeur();
        gm.pointeur.SetTargetingGripPointeur();
    }

    protected void FailUse() {
        gm.soundManager.PlayDeniedPouvoirClip();
        PenaliseCooldown();
    }

    private void PenaliseCooldown() {
        cooldownCustom.Use();
        cooldownCustom.GainChargeIn(rechargeTimeAfterFailedTargeting / player.GetTimeHackCurrentSlowmotionFactor());
    }

    protected void PlayReversedActivationClip() {
        gm.soundManager.PlayActivationPouvoirReversedClip(reversedClip);
    }

    protected void AddTargetingMultiplier() {
        RemoveTargetingMultiplier();
        currentTimeMultiplier = gm.timerManager.AddTimeMultiplier(new TimeMultiplier(targetingSlowmotion));
    }

    protected void RemoveTargetingMultiplier() {
        if (currentTimeMultiplier != null) {
            gm.timerManager.RemoveTimeMultiplier(currentTimeMultiplier);
            currentTimeMultiplier = null;
        }
    }

    protected GameObject CreatePrevisualization() {
        GameObject previsualizationCube = Instantiate(previsualizationCubePrefab, parent: gm.map.particlesFolder);
        return previsualizationCube;
    }

    protected void UpdatePrevisualization(GameObject previsualizationObject, float avancement) {
        if(currentHittedCube == null) {
            previsualizationObject.SetActive(false);
            return;
        }
        previsualizationObject.SetActive(true);
        previsualizationObject.transform.position = currentHittedCube.transform.position;
        previsualizationObject.transform.LookAt(previsualizationObject.transform.position + gm.gravityManager.Forward(), gm.gravityManager.Up());
        float alphaValue = previsualizationAlphaCurve.Evaluate(avancement);
        Material material = previsualizationObject.GetComponent<Renderer>().material;
        material.SetFloat("_Alpha", alphaValue);
        material.SetFloat("_UseVerticalArrow", useVerticalArrow ? 1 : 0);
        material.SetVector("_UpVector", gm.gravityManager.Up());
    }

    protected void DestroyPrevisualization(GameObject previsualizationObject) {
        if(previsualizationObject != null) {
            Destroy(previsualizationObject);
        }
    }

    protected override bool UsePouvoir() {
        Vector3 finalTarget = ComputeTargetFromCube(currentHittedCube, currentHit);
        float distance = (finalTarget - player.transform.position).magnitude;
        RegisterComputedDurationAccordingToDistance(distance);
        //player.RemoveAllNegativePoussees();
        player.RemoveAllPoussees(); // Also regular Dashses !
        currentPoussee = new PousseePrecise(finalTarget, player.transform.position, dashDurationAdjustment, dashSpeed, callback: RestorePlayerMouvementEarlier);
        player.AddPoussee(currentPoussee);
        player.ResetGrip();
        RemoveGravityEffectForDashDuration();
        StunForDashDuration();
        StartVfx();
        gm.timerManager.timeMultiplierController.RemoveAllEnnemisMultipliers();
        player.onUsePouvoir.Invoke(this);
        player.onUseGripDash.Invoke(this);
        return true;
    }

    protected void RegisterComputedDurationAccordingToDistance(float distance) {
        computedDuration = distance / dashSpeed;
    }

    protected void StunForDashDuration() {
        player.StunAndKeepPouvoirs();
        unStunCoroutine = StartCoroutine(CUnStunAtEndOfDash());
    }

    protected void UnStunEarlier() {
        if(unStunCoroutine != null) {
            StopCoroutine(unStunCoroutine);
            unStunCoroutine = null;
        }
        player.UnStun();
    }

    protected IEnumerator CUnStunAtEndOfDash() {
        yield return new WaitForSeconds(GetDuration());
        player.UnStun();
        unStunCoroutine = null;
    }

    protected void RemoveGravityEffectForDashDuration() {
        player.RemoveGravityEffect();
        restoreGravityCoroutine = StartCoroutine(CRestoreGravityAtEndOfDash());
    }

    protected void RestoreGravityEarlier() {
        if(restoreGravityCoroutine != null) {
            StopCoroutine(restoreGravityCoroutine);
            restoreGravityCoroutine = null;
        }
        player.RestoreGravityEffect();
    }

    protected IEnumerator CRestoreGravityAtEndOfDash() {
        yield return new WaitForSeconds(GetDuration());
        player.RestoreGravityEffect();
        restoreGravityCoroutine = null;
    }

    protected void RestorePlayerMouvementEarlier() {
        UnStunEarlier();
        RestoreGravityEarlier();
    }

    protected Vector3 ComputeTargetFromCube(Cube cube, RaycastHit hit) {
        if (hit.collider == null || cube == null) {
            return Vector3.zero;
        }
        Vector3 direction = (hit.point - cube.transform.position).normalized;
        Vector3 closestNormal = MathTools.GetClosestToNormals(cube.transform, direction);
        Vector3 aboveCube = cube.transform.position + gm.gravityManager.Up();
        Vector3 aboveTarget = JustAboveCube(cube);
        if (MathTools.IsNormalHorizontal(closestNormal, gm.gravityManager)) { // Targeting sides
            if (!gm.map.IsCubeAt(aboveCube) && WontBeBlockByCube(aboveTarget)) {
                useVerticalArrow = true;
                return aboveTarget;
            }
            useVerticalArrow = false;
            return JustNextToCube(cube, closestNormal);
        } else if (closestNormal == gm.gravityManager.Down()) { // Targeting from bottom
            if(!gm.map.IsCubeAt(aboveCube) && WontBeBlockByCube(aboveTarget)) {
                useVerticalArrow = true;
                return aboveTarget;
            }
            useVerticalArrow = false;
            return JustNextToCube(cube, closestNormal);
        } else { // Targeting from top
            useVerticalArrow = true;
            return aboveTarget;
        }
    }

    protected bool WontBeBlockByCube(Vector3 aboveTarget) {
        // 27° because arctan(1/2) = 27° et c'est l'angle entre le aboveCubePoint et le point en bas en diagonale
        float angle = Vector3.Angle(gm.gravityManager.Up(), (aboveTarget - player.transform.position));
        return angle >= 27f;
    }

    protected Vector3 JustNextToCube(Cube cube, Vector3 normal) {
        return cube.transform.position + normal * 0.75f;
    }

    protected Vector3 JustAboveCube(Cube cube) {
        return JustNextToCube(cube, gm.gravityManager.Up());
    }

    protected virtual void StartVfx() {
        gm.postProcessManager.StartGripDashVfx(computedDuration);
    }

    public Poussee GetCurrentPoussee() {
        return currentPoussee;
    }

    protected bool CanGripDash() {
        Ray ray = new Ray(player.camera.transform.position, player.camera.transform.forward);
        Physics.Raycast(ray, out currentHit, maxTargetingDistance);
        if(currentHit.collider == null) {
            currentHittedCube = null;
            return false;
        }
        currentHittedCube = currentHit.collider.gameObject.GetComponent<Cube>();
        return currentHittedCube != null;
    }

    public float GetDuration() {
        return computedDuration + dashDurationAdjustment;
    }

    protected void GainChargeListener(PouvoirPowerDash powerDash) {
        cooldownCustom.GainCharge();
    }
}

﻿using System;
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

    protected RaycastHit currentHit;
    protected Cube currentHittedCube;
    protected Poussee currentPoussee = null;
    protected Coroutine targetingCoroutine = null;
    protected Coroutine unStunCoroutine = null;
    protected Coroutine restoreGravityCoroutine = null;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected float computedDuration = 0;

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
        while (!timer.IsOver()) {
            bool canGripDash = CanGripDash();
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
                    gm.soundManager.PlayDeniedPouvoirClip();
                }
                break;
            }
            yield return null;
        }
        DestroyPrevisualization(previsualizationObject);
        RemoveTargetingMultiplier();
        // Faudra se débrouiller pour mettre un cooldown de 1s (ou plus) ici ! :)
        targetingCoroutine = null;
    }

    protected void AddTargetingMultiplier() {
        RemoveTargetingMultiplier();
        currentTimeMultiplier = gm.timerManager.AddTimeMultiplier(targetingSlowmotion);
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
        float alphaValue = previsualizationAlphaCurve.Evaluate(avancement);
        previsualizationObject.GetComponent<Renderer>().material.SetFloat("_Alpha", alphaValue);
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
        currentPoussee = new PousseePrecise(finalTarget, player.transform.position, dashDurationAdjustment, dashSpeed, callback: RestorePlayerMouvementEarlier);
        player.AddPoussee(currentPoussee);
        player.RemoveAllNegativePoussees();
        player.ResetGrip();
        RemoveGravityEffectForDashDuration();
        StunForDashDuration();
        StartVfx();
        gm.timerManager.timeMultiplierController.RemoveAllEnnemisMultipliers();
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
        Vector3 direction = (hit.point - cube.transform.position).normalized;
        Vector3 closestNormal = MathTools.GetClosestToNormals(cube.transform, direction);
        Vector3 aboveCube = cube.transform.position + gm.gravityManager.Up();
        Vector3 aboveTarget = JustAboveCube(cube);
        if (MathTools.IsNormalHorizontal(closestNormal, gm.gravityManager)) {
            if (!gm.map.IsCubeAt(aboveCube) && WontBeBlockByCube(aboveTarget)) {
                return aboveTarget;
            }
            return JustNextToCube(cube, closestNormal);
        } else if (closestNormal == gm.gravityManager.Down()) {
            if(!gm.map.IsCubeAt(aboveCube) && WontBeBlockByCube(aboveTarget)) {
                return aboveTarget;
            }
            return JustNextToCube(cube, closestNormal);
        } else {
            return aboveTarget;
        }
    }

    protected bool WontBeBlockByCube(Vector3 aboveTarget) {
        // 27° because arctan(1/2) = 27° et c'est l'angle entre le aboveCubePoint et le point en bas en diagonale
        float angle = Vector3.Angle(gm.gravityManager.Up(), (aboveTarget - player.transform.position));
        Debug.Log($"angle = {angle}");
        return angle >= 27f;
    }

    protected Vector3 JustNextToCube(Cube cube, Vector3 normal) {
        return cube.transform.position + normal * 0.75f;
    }

    protected Vector3 JustAboveCube(Cube cube) {
        return JustNextToCube(cube, gm.gravityManager.Up());
    }

    protected virtual void StartVfx() {
        //gm.postProcessManager.StartDashVfx(duree);
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SecondBoss : TracerBlast {

    [Header("SecondBoss Parameters")]

    [Header("Impact Faces")]
    public int nbImpactFacesByPhases = 3;
    public float impactStunDuration = 0.5f;

    [Header("PresenceSound")]
    public Vector2 presenceSoundVolumeRange = new Vector2(1, 7);

    protected int currentPhase;
    protected bool hasImpactFacesOn = false;
    protected int nbFacesToImpactRemaining;
    protected List<Vector3> faceNormalsUsable;
    protected Vector3 currentImpactFaceDirection;

    public override void Start () {
        base.Start();
        name = "SecondBoss";
        player.onPowerDashEnnemiImpact.AddListener(OnPowerDashImpactFace);
        GoToPhase1();
        StartPresenceClip();
    }

    protected void GoToPhase1() {
        Debug.Log($"Phase 1 !");
        currentPhase = 1;
        StartImpactFaces();
    }

    protected void StartImpactFaces() {
        hasImpactFacesOn = true;
        nbFacesToImpactRemaining = nbImpactFacesByPhases;
        faceNormalsUsable = MathTools.GetAllOrthogonalNormals();
        StartOneImpactFace();
    }

    protected void StopImpactFaces() {
        hasImpactFacesOn = false;
        material.SetFloat("_HasVulnerableFace", 0.0f);
    }

    protected void StartOneImpactFace() {
        currentImpactFaceDirection = MathTools.ChoseOne(faceNormalsUsable);
        faceNormalsUsable.Remove(currentImpactFaceDirection);
        material.SetFloat("_HasVulnerableFace", 1.0f);
        material.SetVector("_DirectionVulnerableFace", currentImpactFaceDirection);
    }

    public override bool CanBeHitByPowerDash(PouvoirPowerDash powerDash) {
        return hasImpactFacesOn
            && IsPowerDashGoingTowardImpactFace(powerDash)
            && IsPlayerOnSideOfTheImpactFace();
    }

    protected bool IsPowerDashGoingTowardImpactFace(PouvoirPowerDash powerDash) {
        Vector3 dashDirection = powerDash.GetCurrentPoussee().direction;
        Vector3 impactFaceDirection = transform.rotation * currentImpactFaceDirection;
        bool dashIsInGoodDirection = Vector3.Dot(dashDirection, impactFaceDirection) < 0;
        return dashIsInGoodDirection;
    }

    protected bool IsPlayerOnSideOfTheImpactFace() {
        Vector3 impactFaceDirection = transform.rotation * currentImpactFaceDirection;
        float bossHalfSize = transform.localScale.x / 2;
        float playerDistanceProjected = Vector3.Dot(player.transform.position - transform.position, impactFaceDirection);
        return playerDistanceProjected > bossHalfSize;
    }

    protected override bool IsPlayerComingFromTop() {
        return false; // Le joueur ne peut pas abuser la face du haut ! :)
    }

    protected override bool CanHitPlayerFromSides() {
        return timerContactHit.IsOver()
            && (!IsPlayerOnSideOfTheImpactFace() || !player.IsPowerDashing())
            && !IsStunned()
            && !player.IsTimeHackOn();
    }

    public void OnPowerDashImpactFace(PouvoirPowerDash powerDash, Ennemi ennemi) {
        if(ennemi != this) {
            return;
        }
        if(!hasImpactFacesOn) {
            return;
        }
        nbFacesToImpactRemaining--;
        if(nbFacesToImpactRemaining <= 0) {
            StopImpactFaces();
            GoToNextPhase();
        } else {
            StartOneImpactFace();
        }
    }

    protected override void ApplyStunOfPowerDash(PouvoirPowerDash powerDash) {
        float stunDuration = powerDash.dureePoussee + detectionDureeRotation + impactStunDuration;
        SpeedMultiplier newSpeedMultiplier = new SpeedMultiplier(powerDash.speedMultiplierStun);
        newSpeedMultiplier.duration = stunDuration;
        speedMultiplierController.AddMultiplier(newSpeedMultiplier);
        SetStunnedFor(stunDuration);
        StartCoroutine(CDetectPlayerIn(stunDuration - detectionDureeRotation));
    }

    protected IEnumerator CDetectPlayerIn(float duration) {
        yield return new WaitForSeconds(duration);
        OnDetectPlayer();
    }

    protected void GoToNextPhase() {
        switch (currentPhase) {
            case 1:
                GoToPhase2();
                break;
            case 2:
                GoToPhase3();
                break;
            case 3:
                GoToPhase4();
                break;
            default:
                break;
        }
    }

    protected void GoToPhase2() {
        Debug.Log($"Phase 2 !");
    }

    protected void GoToPhase3() {
        Debug.Log($"Phase 3 !");
    }

    protected void GoToPhase4() {
        Debug.Log($"Phase 4 !");
    }

    protected void StartPresenceClip() {
        gm.soundManager.PlayFirstBossPresenceClip(transform.position, transform, presenceSoundVolumeRange);
    }

    public override Vector3 PopPosition(MapManager map) {
        return map.GetFreeBoxLocation(Vector3.one * 3.0f);
    }
}

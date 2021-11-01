﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using static SoulRobberController;

public class SoulRobber : Ennemi {

    public static string SHADER_TELEPORT_STARTING_TIME = "_TeleportStartingTime";
    public static string SHADER_TELEPORT_DURATION = "_TeleportDuration";
    public static string VFX_RAY_THICKNESS = "Thickness";
    public static string VFX_RAY_DISTORSION_AMOUNT = "DistorsionAmount";

    [Header("Ray")]
    public GameObject rayPrefab;
    public float durationToFullRay = 6.0f; // Not really "full" ray, but take 100% of speed of the player (can go higher if the player has speed boosts ! :))
    public SpeedMultiplier speedDecreaseMultiplier;

    [Header("Ray Charging Animation")]
    public float timeAtMaxRayAnimation = 3.0f;
    public Vector2 rayThicknessRange;
    public Vector2 rayDistorsionAmountRange;

    [Header("Detection Animation")]

    [Header("Teleport")]
    public float durationBeforeTeleportAway = 0.3f;
    public int nbPositionsTestForTeleportAway = 100;
    public GameObject teleportPrevisualizationLightningPrefab;

    protected SoulRobberController soulRobberController;
    protected EventManager.DeathReason currentDeathReason; // TO INIT !!
    protected Coroutine fireringCoroutine;
    protected Coroutine teleportAwayCoroutine;
    protected Lightning ray;
    protected SpeedMultiplier currentMultiplier = null;

    public override void Start() {
        base.Start();
        soulRobberController = GetComponent<SoulRobberController>();
        StartTeleportAnimation(soulRobberController.tempsInactifDebutJeu);
    }

    public override void UpdateSpecific() {
        TestForPlayerCollision();
    }

    protected void TestForPlayerCollision() {
        if (MathTools.CapsuleSphere(transform.position, GetRadius() * 1.1f, GetHeight(), player.transform.position, player.GetSizeRadius())) {
            if (GetState() != SoulRobberState.ESCAPING) {
                if (teleportAwayCoroutine == null) {
                    teleportAwayCoroutine = StartCoroutine(CTeleportAway());
                }
            }
        }
    }

    protected IEnumerator CTeleportAway() {
        Vector3 teleportPosition = GetBestPositionToTeleportAway();
        controller.enabled = false;
        StopFirering();
        StartTeleportAnimation(durationBeforeTeleportAway * 2);
        Lightning lightning = Instantiate(teleportPrevisualizationLightningPrefab).GetComponent<Lightning>();
        lightning.Initialize(transform.position, teleportPosition);
        gm.soundManager.PlayCatchSoulRobberClip(transform.position);
        ShakeScreen();
        yield return new WaitForSeconds(durationBeforeTeleportAway);
        transform.position = teleportPosition;
        controller.enabled = true;
        yield return new WaitForSeconds(durationBeforeTeleportAway);
        if (soulRobberController.IsPlayerVisible()) {
            StartFirering();
        }
        teleportAwayCoroutine = null;
    }

    public float GetRadius() {
        return controller.radius * transform.localScale.x;
    }

    public float GetHeight() {
        return controller.height * transform.localScale.x;
    }

    protected void HitPlayerCustom(EventManager.DeathReason deathReason, float timeMalus) {
        EventManager.DeathReason oldDeathReason = currentDeathReason;
        currentDeathReason = deathReason;
        HitPlayer(useCustomTimeMalus: true, customTimeMalus: timeMalus);
        currentDeathReason = oldDeathReason;
    }

    public void StartFirering() {
        if(fireringCoroutine != null) {
            StopFirering();
        }
        fireringCoroutine = StartCoroutine(CFirering());
    }

    protected IEnumerator CFirering() {
        ray = Instantiate(rayPrefab, parent: transform).GetComponent<Lightning>();
        ray.Initialize(transform.position, GetRayTargetPosition());
        Timer timer = new Timer(durationToFullRay);
        currentMultiplier = player.speedMultiplierController.AddMultiplier(new SpeedMultiplier(speedDecreaseMultiplier));

        while(true) {
            ray.SetPosition(transform.position, GetRayTargetPosition(), parentSize: transform.localScale.x);
            if (timer.GetAvancement() > 1) {
                currentMultiplier.speedAdded = -timer.GetAvancement();
            }
            UpdateRaySize(ray, timer.GetElapsedTime() / timeAtMaxRayAnimation);
            yield return null;
        }
    }

    public void StopFirering() {
        DestroyRay();
        player.speedMultiplierController.RemoveMultiplier(currentMultiplier);
        currentMultiplier = null;
        if(fireringCoroutine != null) {
            StopCoroutine(fireringCoroutine);
            fireringCoroutine = null;
        }
    }

    protected void DestroyRay() {
        if (ray != null) {
            ray.StopRefreshing();
            Destroy(ray.gameObject, ray.refreshRate);
            ray = null;
        }
    }

    protected void UpdateRaySize(Lightning ray, float avancement) {
        ray.vfx.SetFloat(VFX_RAY_THICKNESS, MathCurves.Linear(rayThicknessRange.x, rayThicknessRange.y, avancement));
        ray.vfx.SetFloat(VFX_RAY_DISTORSION_AMOUNT, MathCurves.Linear(rayDistorsionAmountRange.x, rayDistorsionAmountRange.y, avancement));
    }

    protected Vector3 GetRayTargetPosition() {
        Vector3 playerPos = player.transform.position;
        return playerPos + gm.gravityManager.Down() * 0.3f + (transform.position - playerPos).normalized * 0.1f;
    }

    public override EventManager.DeathReason GetDeathReason() {
        return currentDeathReason;
    }

    public override bool CanCapture() {
        return GetState() != SoulRobberState.ESCAPING;
    }

    public SoulRobberState GetState() {
        return soulRobberController.GetState();
    }

    protected override void HitPlayerSpecific() {
    }

    protected override void HitContinuousPlayerSpecific() {
    }

    public Vector3 TeleportAway() {
        Vector3 bestPosition = GetBestPositionToTeleportAway();
        transform.position = bestPosition;
        return bestPosition;
    }

    public Vector3 GetBestPositionToTeleportAway() {
        List<Tuple<Vector3, float>> positionsAndScores = new List<Tuple<Vector3, float>>();
        Vector3 up = gm.gravityManager.Up();
        float maxRangeSqr = soulRobberController.distanceDeDetection * soulRobberController.distanceDeDetection;
        for (int i = 0; i < nbPositionsTestForTeleportAway; i++) {
            Vector3 position = gm.map.GetFreeRoundedLocation();
            float score;
            if(gm.map.IsCubeAt(position + up)) {
                score = 0;
            } else {
                position += up * 0.5f;
                float sqrDistance = Vector3.SqrMagnitude(player.transform.position - transform.position);
                score = sqrDistance;
                if(sqrDistance < maxRangeSqr) {
                    score /= 10;
                }
                if (!CanSeePlayerFrom(position)) {
                    score /= 10;
                }
            }
            positionsAndScores.Add(new Tuple<Vector3, float>(position, score));
        }
        return positionsAndScores.OrderBy(t => t.Item2).Last().Item1;
    }

    public bool CanSeePlayerFrom(Vector3 position) {
        RaycastHit hit;
        Ray ray = new Ray (position, player.transform.position - position);
        return Physics.Raycast(ray, out hit, player.camera.farClipPlane) && hit.collider.name == "Joueur";
    }

    protected void StartTeleportAnimation(float duration) {
        GetComponent<Renderer>().material.SetFloat(SHADER_TELEPORT_STARTING_TIME, Time.time);
        GetComponent<Renderer>().material.SetFloat(SHADER_TELEPORT_DURATION, duration);
    }
}

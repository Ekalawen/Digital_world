using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using static SoulRobberController;

public class SoulRobber : Ennemi {

    public enum TeleportMode { AWAY, ON_PLAYER };

    public static string SHADER_TELEPORT_STARTING_TIME = "_TeleportStartingTime";
    public static string SHADER_TELEPORT_DURATION = "_TeleportDuration";
    public static string SHADER_ENTROPY_SOURCE = "_EntropySource";
    public static string VFX_RAY_THICKNESS = "Thickness";
    public static string VFX_RAY_DISTORSION_AMOUNT = "DistorsionAmount";

    protected static bool isPlayerRobbed = false;

    [Header("Ray")]
    public GameObject rayPrefab;
    public float durationToFullRay = 6.0f; // Not really "full" ray, but take 100% of speed of the player (can go higher if the player has speed boosts ! :))
    public SpeedMultiplier speedDecreaseMultiplier;
    public GeoData rayGeoData;

    [Header("Ray Charging Animation")]
    public float timeAtMaxRayAnimation = 3.0f;
    public Vector2 rayThicknessRange;
    public Vector2 rayDistorsionAmountRange;

    [Header("Detection Animation")]

    [Header("Teleport")]
    public float durationBeforeTeleportAway = 0.3f;
    public int nbPositionsTestForTeleportAway = 100;
    public GameObject teleportPrevisualizationLightningPrefab;
    public float screenShakeTeleportMagnitude = 10.0f;
    public float screenShakeTeleportRoughness = 10.0f;
    public float screenShakeTeleportDuration = 0.2f;

    [Header("Robbing Mode")]
    public float durationRobBeforeKill = 7.0f;
    public float robbModeYScale = 0.2f;
    public float timeToChangeScale = 0.4f;
    public SpeedMultiplier speedBoostOnRob;
    public GeoData robbingGeoData;
    public SpeedMultiplier playerSpeedBoostOnEndRobb;

    protected SoulRobberController soulRobberController;
    protected EventManager.DeathReason currentDeathReason; // TO INIT !!
    protected Coroutine fireringCoroutine;
    protected Coroutine teleportAwayCoroutine;
    protected Lightning ray;
    protected SpeedMultiplier currentMultiplier = null;
    protected Fluctuator changeScaleFluctuator;
    protected float initialYScale;
    protected Coroutine robbCountdownCoroutine;
    protected GeoPoint rayGeoPoint = null;
    protected GeoPoint robbingGeoPoint = null;

    public override void Start() {
        base.Start();
        soulRobberController = GetComponent<SoulRobberController>();
        changeScaleFluctuator = new Fluctuator(this, GetYScale, SetYScale);
        initialYScale = transform.localScale.y;
        GetComponent<Renderer>().material.SetVector(SHADER_ENTROPY_SOURCE, transform.position);
        StartTeleportAnimation(soulRobberController.tempsInactifDebutJeu, avancement: 0.5f);
    }

    public override void UpdateSpecific() {
        TestForPlayerCollision();
    }

    protected void TestForPlayerCollision() {
        //if (controller.enabled && MathTools.CapsuleSphere(transform.position, GetRadius() * 1.15f, GetHeight(), player.transform.position, player.GetSizeRadius())) {
        if (controller.enabled && MathTools.CylinderSphere(transform.position, GetRadius() * 1.05f, GetHeight(), player.transform.position, player.GetSizeRadius())) {
            if (GetState() != SoulRobberState.ESCAPING) {
                TeleportAway();
            }
            else {
                StartUnrobb();
                TeleportAway();
            }
        }
    }

    public void TeleportAway() {
        if (teleportAwayCoroutine != null) {
            StopCoroutine(teleportAwayCoroutine);
        }
        teleportAwayCoroutine = StartCoroutine(CTeleport(TeleportMode.AWAY));
    }

    public void TeleportOnPlayer() {
        if (teleportAwayCoroutine != null) {
            StopCoroutine(teleportAwayCoroutine);
        }
        teleportAwayCoroutine = StartCoroutine(CTeleport(TeleportMode.ON_PLAYER));
    }

    protected IEnumerator CTeleport(TeleportMode teleportMode) {
        Vector3 teleportPosition = GetBestPositionToTeleport(teleportMode);
        controller.enabled = false;
        StopFirering();
        StartTeleportAnimation(durationBeforeTeleportAway * 2);
        if (teleportMode == TeleportMode.AWAY) {
            gm.soundManager.PlayCatchSoulRobberClip(transform.position);
            Lightning lightning = Instantiate(teleportPrevisualizationLightningPrefab).GetComponent<Lightning>();
            lightning.Initialize(transform.position, teleportPosition);
            ShakeScreenOnTeleport();
        }

        yield return new WaitForSeconds(durationBeforeTeleportAway);
        transform.position = teleportPosition;
        controller.enabled = true;
        if (teleportMode == TeleportMode.ON_PLAYER) {
            gm.soundManager.PlayCatchSoulRobberClip(teleportPosition);
        }

        yield return new WaitForSeconds(durationBeforeTeleportAway);
        teleportAwayCoroutine = null;
        if (soulRobberController.IsPlayerVisible() && GetState() == SoulRobberState.FIRERING) {
            StartFirering();
        }
    }

    protected void ShakeScreenOnTeleport() {
        CameraShaker cs = CameraShaker.Instance;
        cs.ShakeOnce(screenShakeTeleportMagnitude, screenShakeTeleportRoughness, 0.1f, screenShakeTeleportDuration);
    }

    protected void ShakeScreenOnRob() {
        ShakeScreen();
    }

    public float GetRadius() {
        return controller.radius * transform.localScale.x;
    }

    public float GetHeight() {
        return controller.height * transform.localScale.x;
    }

    public void StartFirering() {
        if(teleportAwayCoroutine != null) {
            return;
        }
        if(fireringCoroutine != null) {
            StopFirering();
        }
        fireringCoroutine = StartCoroutine(CFirering());
    }

    protected IEnumerator CFirering() {
        ray = Instantiate(rayPrefab, parent: transform).GetComponent<Lightning>();
        ray.Initialize(transform.position, GetRayTargetPosition());
        Timer timer = new Timer(durationToFullRay);
        currentMultiplier = player.AddMultiplier(new SpeedMultiplier(speedDecreaseMultiplier));
        rayGeoPoint = player.geoSphere.AddGeoPoint(rayGeoData);

        while(true) {
            ray.SetPosition(transform.position, GetRayTargetPosition(), parentSize: transform.localScale.x);
            if (timer.GetAvancement() > 1) {
                currentMultiplier.speedAdded = -timer.GetAvancement();
            }
            if(!IsPlayerRobbed() && player.GetSpeedMultiplier() == 0.0f) {
                StartRobb();
            }
            UpdateRaySize(ray, timer.GetElapsedTime() / timeAtMaxRayAnimation);
            yield return null;
        }
    }

    public virtual void StartEscaping() {
        AddMultiplier(speedBoostOnRob);
        changeScaleFluctuator.GoTo(robbModeYScale, timeToChangeScale);
        robbingGeoPoint = player.geoSphere.AddGeoPoint(robbingGeoData);
    }

    public void StartRobb() {
        if (gm.eventManager.IsGameOver())
            return;
        RobPlayer();
        gm.postProcessManager.StartBlackAndWhiteEffect(durationRobBeforeKill);
        ShakeScreenOnRob();
        TriggerHitEffect();
        player.geoSphere.AddGeoPoint(geoDataHit);
        // Start Sound
        // Réduire le son de la musique
        StartCountdown();
        // Noircir l'écran petit à petit !
    }

    protected void StartCountdown() {
        StopCountdown();
        robbCountdownCoroutine = StartCoroutine(CRobbCountdown());
    }

    protected void StopCountdown() {
        foreach(SoulRobber soulRobber in gm.ennemiManager.GetEnnemisOfType<SoulRobber>()) {
            if (soulRobber.robbCountdownCoroutine != null) {
                soulRobber.StopCoroutine(soulRobber.robbCountdownCoroutine);
            }
        }
    }

    public static void RobPlayer() {
        isPlayerRobbed = true;
    }

    protected IEnumerator CRobbCountdown() {
        yield return new WaitForSeconds(durationRobBeforeKill);
        gm.eventManager.LoseGame(EventManager.DeathReason.SOUL_ROBBER_ASPIRATION);
        gm.postProcessManager.SetBlackAndWhiteEffectToBarelyVisible();
    }

    public virtual void StopEscaping() {
        changeScaleFluctuator.GoTo(initialYScale, timeToChangeScale);
        if (robbingGeoPoint != null) {
            robbingGeoPoint.Stop();
            robbingGeoPoint = null;
        }
    }

    public void StartUnrobb() {
        UnrobPlayer();
        gm.postProcessManager.StopBlackAndWhiteEffect();
        ShakeScreenOnRob();
        TriggerHitEffect();
        StopCountdown();
        player.AddMultiplier(playerSpeedBoostOnEndRobb);
        // Start Sound
        // Réaugmenter le son de la musique
    }

    public static void UnrobPlayer() {
        isPlayerRobbed = false;
    }

    public static bool IsPlayerRobbed() {
        return isPlayerRobbed;
    }

    public void StopFirering() {
        DestroyRay();
        RemoveCurrentSpeedMultiplier();
        if(rayGeoPoint != null) {
            rayGeoPoint.Stop();
            rayGeoPoint = null;
        }
        if (fireringCoroutine != null) {
            StopCoroutine(fireringCoroutine);
            fireringCoroutine = null;
        }
    }

    protected void RemoveCurrentSpeedMultiplier() {
        player.speedMultiplierController.RemoveMultiplier(currentMultiplier);
        currentMultiplier = null;
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

    public Vector3 GetBestPositionToTeleport(TeleportMode teleportMode) {
        List<Tuple<Vector3, float>> positionsAndScores = new List<Tuple<Vector3, float>>();
        Vector3 up = gm.gravityManager.Up();
        float maxRangeSqr = soulRobberController.distanceDeDetection * soulRobberController.distanceDeDetection;
        float onPlayerMaxRangeSqr = maxRangeSqr / 4; // /2 deux fois c'est /4 ! :D
        Vector3 position;
        for (int i = 0; i < nbPositionsTestForTeleportAway; i++) {
            if (teleportMode == TeleportMode.AWAY) {
                position = gm.map.GetFreeRoundedLocation();
            } else {
                if(i < nbPositionsTestForTeleportAway / 2) {
                    position = gm.map.GetFreeRoundedLocation();
                } else {
                    position = gm.map.GetRandomEmptyLocationInSphere(player.transform.position, soulRobberController.distanceDeDetection / 2);
                }
            }
            float score;
            if(gm.map.IsCubeAt(position + up)) {
                score = 0;
            } else {
                position += up * 0.5f;
                float sqrDistance = Vector3.SqrMagnitude(player.transform.position - position);
                score = sqrDistance;
                if(sqrDistance > maxRangeSqr) {
                    score /= 10;
                }
                if(teleportMode == TeleportMode.ON_PLAYER) {
                    if(sqrDistance > onPlayerMaxRangeSqr) {
                        score /= 5;
                    }
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

    protected void StartTeleportAnimation(float duration, float avancement = 0.0f) {
        duration = duration / (1 - avancement);
        GetComponent<Renderer>().material.SetFloat(SHADER_TELEPORT_STARTING_TIME, Time.time - duration * avancement);
        GetComponent<Renderer>().material.SetFloat(SHADER_TELEPORT_DURATION, duration);
    }

    public float GetYScale() {
        return transform.localScale.y;
    }

    public void SetYScale(float value) {
        Vector3 scale = transform.localScale;
        scale.y = value;
        transform.localScale = scale;
    }

    public override Vector3 PopPosition(MapManager map) {
        Vector3 pos = map.GetFreeRoundedLocationWithoutLumiere();
        Vector3 up = gm.gravityManager.Up();
        for(int k = 0; k < 1000; k++) {
            if(!map.IsCubeAt(pos + up) && !map.IsLumiereAt(pos + up)) {
                pos += up * 0.5f;
                break;
            }
            pos = map.GetFreeRoundedLocationWithoutLumiere();
        }
        return pos;
    }
}

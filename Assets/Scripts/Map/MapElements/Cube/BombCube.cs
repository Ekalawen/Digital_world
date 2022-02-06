using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCube : NonBlackCube {

    [Header("Explosion")]
    public float explosionRange = 5.0f;
    public float pousseeDistance = 10.0f;
    public float pousseeDuree = 0.5f;
    public TimeMultiplier slowmotion;
    public float screenShakeMagnitude;
    public float screenShakeRoughness;
    public float screenShakeDecreaseTime;
    public GeoData geoDataImpact;

    public override void Initialize() {
        base.Initialize();
        CheckPlayerCollisionOnStart();
    }

    protected virtual void CheckPlayerCollisionOnStart() {
        GameManager gm = GameManager.Instance;
        if (gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        float time = Time.time;
        float decomposeStartingTime = transparentMaterial.GetFloat("_DecomposeStartingTime");
        if (time < decomposeStartingTime && !gm.player.IsInvincible() && gm.eventManager.IsGameOver()) {
            BombExplosion();
        }
    }

    public void BombExplosion() {
        DestroyAllNearByCubes();
        DivideTime();
        AddPoussee();
        gm.timerManager.AddTimeMultiplierForEnnemiImpact(slowmotion);
        gm.soundManager.PlayBombCubeExplosionClip(transform.position);
        gm.postProcessManager.UpdateHitEffect();
        ShakeScreen();
        AddGeoPointOfImpact();
    }

    protected void AddGeoPointOfImpact() {
        gm.player.geoSphere.AddGeoPoint(new GeoData(geoDataImpact));
    }

    protected void AddPoussee() {
        Vector3 pousseeDirection = (gm.player.transform.position - transform.position).normalized;
        gm.player.AddPoussee(new Poussee(pousseeDirection, pousseeDuree, pousseeDistance), isNegative: true);
    }

    protected void DivideTime() {
        gm.timerManager.DivideTimeBy(2, EventManager.DeathReason.TOUCHED_DEATH_CUBE); // Never supposed to kill !
    }

    protected void DestroyAllNearByCubes() {
        List<Cube> nearByCubes = gm.map.GetCubesInSphere(transform.position, explosionRange);
        foreach(Cube cube in nearByCubes) {
            cube.Explode();
        }
    }

    protected void ShakeScreen() {
        CameraShaker cs = CameraShaker.Instance;
        cs.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.1f, screenShakeDecreaseTime);
    }
}

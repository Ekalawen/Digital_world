using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static SoulRobberController;

public class SoulRobber : Ennemi {

    [Header("Ray")]
    public GameObject rayPrefab;
    public SpeedMultiplier speedDecreaseMultiplier;

    [Header("Ray Charging Animation")]

    [Header("Detection Animation")]

    protected SoulRobberController soulRobberController;
    protected EventManager.DeathReason currentDeathReason; // TO INIT !!
    protected Coroutine fireringCoroutine;
    protected Lightning ray;

    public override void Start() {
        base.Start();
        soulRobberController = GetComponent<SoulRobberController>();
    }

    public override void UpdateSpecific() {
        TestForPlayerCollision();
    }

    protected void TestForPlayerCollision() {
    }

    protected void HitPlayerCustom(EventManager.DeathReason deathReason, float timeMalus) {
        EventManager.DeathReason oldDeathReason = currentDeathReason;
        currentDeathReason = deathReason;
        HitPlayer(useCustomTimeMalus: true, customTimeMalus: timeMalus);
        currentDeathReason = oldDeathReason;
    }

    public void StartFirering() {
        fireringCoroutine = StartCoroutine(CFirering());
    }

    public void StopFirering() {
        if(fireringCoroutine != null) {
            StopCoroutine(fireringCoroutine);
            DestroyRay();
        }
    }

    protected IEnumerator CFirering() {
        ray = Instantiate(rayPrefab, parent: transform).GetComponent<Lightning>();
        ray.Initialize(transform.position, GetRayTargetPosition());
        Timer timer = new Timer();

        while(true) {
            ray.SetPosition(transform.position, GetRayTargetPosition(), parentSize: transform.localScale.x);
            yield return null;
        }
    }

    protected Vector3 GetRayTargetPosition() {
        Vector3 playerPos = player.transform.position;
        return playerPos + gm.gravityManager.Down() * 0.3f + (transform.position - playerPos).normalized * 0.1f;
    }

    protected void DestroyRay() {
        if (ray != null) {
            ray.StopRefreshing();
            Destroy(ray.gameObject, ray.refreshRate);
            ray = null;
        }
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
}

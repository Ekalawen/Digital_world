using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ennemi : Character {

    [Header("Hit times")]
    public float timeMalusOnHit = 5.0f; // Le temps que perd le joueur lorsqu'il se fait touché !
    public float timeBetweenTwoHits = 0.2f;
    public float timeBetweenTwoHitsDamages = 1.0f;
    public GeoData geoDataHit;

    [Header("Screen Shake")]
    public float screenShakeMagnitude = 5.0f;
    public float screenShakeRoughness = 10.0f;
    public float screenShakeDecreaseTime = 0.2f;

	protected GameManager gm;
	protected Player player;
    protected Timer timerHit;
    protected Timer timerHitDamages;
    protected Poussee currentPousseeByPlayer = null;
    protected Timer isStunnedTimer;

    public override void Start () {
        base.Start();
        gm = GameManager.Instance;
        player = gm.player;
		controller = this.GetComponent<CharacterController> ();
        timerHit = new Timer(timeBetweenTwoHits);
        timerHit.SetOver();
        timerHitDamages = new Timer(timeBetweenTwoHitsDamages);
        timerHitDamages.SetOver();
        isStunnedTimer = new Timer(setOver: true);
	}

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(GameManager.Instance.IsTimeFreezed()) {
            return;
        }

        UpdateSpecific();
	}

    public virtual void FixedUpdate() {
        ApplyPoussees();
    }

    public abstract void UpdateSpecific();

    protected virtual void HitPlayer(bool useCustomTimeMalus = false, float customTimeMalus = 0.0f) {
        HitContinuousPlayerSpecific();
        if(timerHit.IsOver())
        {
            timerHit.Reset();

            HitPlayerSpecific();
            TriggerHitEffect();
            player.OnHit();
            player.geoSphere.AddGeoPoint(geoDataHit);
            if (timerHitDamages.IsOver())
            {
                timerHitDamages.Reset();
                float timeMalusToUse = useCustomTimeMalus ? customTimeMalus : timeMalusOnHit;
                gm.timerManager.RemoveTime(timeMalusToUse, GetDeathReason());
            }
            DisplayHitMessage(GetDeathReason());
            PlayHitSound();
            ShakeScreen();
        }
    }

    public void HitByPlayerPowerDash(PouvoirPowerDash powerDash)
    {
        if (currentPousseeByPlayer != null && !currentPousseeByPlayer.IsOver())
        {
            currentPousseeByPlayer.Stop();
        }
        Vector3 direction = (transform.position - player.transform.position).normalized;
        if (powerDash.GetCurrentPoussee() != null)
        {
            direction = (direction + powerDash.GetCurrentPoussee().direction.normalized).normalized;
        }
        currentPousseeByPlayer = new Poussee(direction, powerDash.dureePoussee, powerDash.distancePoussee);
        AddPoussee(currentPousseeByPlayer);
        ApplyStunOfPowerDash(powerDash);
    }

    protected virtual void ApplyStunOfPowerDash(PouvoirPowerDash powerDash) {
        speedMultiplierController.AddMultiplier(new SpeedMultiplier(powerDash.speedMultiplierStun));
        SetStunnedFor(powerDash.speedMultiplierStun.GetTotalDuration());
    }

    public void SetUnstunned() {
        isStunnedTimer.SetOver();
    }

    public void SetStunnedFor(float duration) {
        if(isStunnedTimer.GetRemainingTime() > duration) {
            return;
        }
        isStunnedTimer.SetRemainingTime(duration);
    }

    public bool IsStunned() {
        return !isStunnedTimer.IsOver();
    }

    protected void TriggerHitEffect() {
        gm.postProcessManager.UpdateHitEffect();
    }

    public virtual Vector3Int GetRoundedSize() {
        int nbOnX = Mathf.CeilToInt(transform.localScale.x) - 1;
        int nbOnY = Mathf.CeilToInt(transform.localScale.y) - 1;
        int nbOnZ = Mathf.CeilToInt(transform.localScale.z) - 1;
        return new Vector3Int(nbOnX, nbOnY, nbOnZ);
    }

    public List<Vector3> GetAllOccupiedRoundedPositions() {
        Vector3 pos = MathTools.Round(transform.position);
        Vector3Int nbOnAxes = GetRoundedSize();
        List<Vector3> occupiedPositions = new List<Vector3>();
        for(int i = -nbOnAxes.x; i <= nbOnAxes.x; i++) {
            for(int j = -nbOnAxes.y; j <= nbOnAxes.y; j++) {
                for(int k = -nbOnAxes.z; k <= nbOnAxes.z; k++) {
                    occupiedPositions.Add(pos + new Vector3(i, j, k));
                }
            }
        }
        return occupiedPositions;
    }

    public abstract EventManager.DeathReason GetDeathReason();

    protected void ShakeScreen() {
        CameraShaker cs = CameraShaker.Instance;
        cs.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.1f, screenShakeDecreaseTime);
    }

    public void SetStartWaitingTime(float startWaitingTime) {
        if (startWaitingTime != -1) {
            IController iController = GetComponent<IController>();
            iController.SetWaitingTime(startWaitingTime);
        }
    }

    public virtual void DisplayHitMessage(EventManager.DeathReason deathReason) {
        // Et on affiche un message dans la console !
        if (!gm.eventManager.IsGameOver()) {
            gm.console.JoueurToucheSonde();
        }
    }

    public virtual void PlayHitSound() {
        if (!gm.eventManager.IsGameOver()) {
            gm.soundManager.PlayHitClip(transform.position);
        }
    }

    protected abstract void HitPlayerSpecific();
    protected abstract void HitContinuousPlayerSpecific();

	// Permet de savoir si l'ennemi est en mouvement
	public virtual bool IsMoving() {
        IController controller = GetComponent<IController>();
        return controller.IsMoving();
	}

	// Permet de savoir si l'ennemi ne bouge pas
    public virtual bool IsInactive() {
        IController controller = GetComponent<IController>();
        return controller.IsInactive();
    }

    public void DestroyEnnemi() {
        gm.ennemiManager.RemoveEnnemi(this);
        Destroy(gameObject);
    }

    public virtual bool CanCapture() {
        return true;
    }

    public virtual Vector3 PopPosition(MapManager map) {
        if (!map.gm.ennemiManager.shouldUseBoundingBoxForWandering) {
            return map.GetFreeRoundedLocationWithoutLumiere();
        } else {
            return map.GetFreeRoundedLocationInBoundingBoxWithoutLumiere();
        }
    }

    public virtual bool CanBeHitByPowerDash(PouvoirPowerDash powerDash) {
        return true;
    }

    public float GetHalfSize() {
        return transform.localScale.x / 2.0f;
    }
}

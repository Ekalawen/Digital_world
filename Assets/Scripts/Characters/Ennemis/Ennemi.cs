﻿using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ennemi : Character {

    [Header("Hit times")]
    public float timeMalusOnHit = 5.0f; // Le temps que perd le joueur lorsqu'il se fait touché !
    public float timeBetweenTwoHits = 1.0f;
    public float timeBetweenTwoHitsDamages = 1.0f;

    [Header("Screen Shake")]
    public float screenShakeMagnitude = 5.0f;
    public float screenShakeRoughness = 15.0f;

	protected GameManager gm;
	protected Player player;
    protected Timer timerHit;
    protected Timer timerHitDamages;

	public override void Start () {
        base.Start();
        gm = GameManager.Instance;
        player = gm.player;
		controller = this.GetComponent<CharacterController> ();
        timerHit = new Timer(timeBetweenTwoHits);
        timerHit.SetOver();
        timerHitDamages = new Timer(timeBetweenTwoHitsDamages);
        timerHitDamages.SetOver();
	}

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(GameManager.Instance.IsTimeFreezed()) {
            return;
        }

        UpdateSpecific();

        ApplyPoussees();
	}

    public abstract void UpdateSpecific();

    protected virtual void HitPlayer(bool useCustomTimeMalus = false, float customTimeMalus = 0.0f) {
        HitContinuousPlayerSpecific();
        float timeMalusToUse = useCustomTimeMalus ? customTimeMalus : timeMalusOnHit;
        if(timerHit.IsOver()) {
            timerHit.Reset();

            HitPlayerSpecific();
            if (timerHitDamages.IsOver()) {
                timerHitDamages.Reset();
                gm.timerManager.AddTime(-timeMalusToUse);
            }
            DisplayHitMessage();
            PlayHitSound();
            ShakeScreen();
        }
    }

    protected void ShakeScreen() {
        CameraShaker cs = CameraShaker.Instance;
        cs.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.1f, timeBetweenTwoHits);
    }

    public virtual void DisplayHitMessage() {
        // Et on affiche un message dans la console !
        if (!gm.eventManager.IsGameOver()) {
            gm.console.JoueurToucheSonde();
        }
    }

    public virtual void PlayHitSound() {
        if(!gm.eventManager.IsGameOver())
            gm.soundManager.PlayHitClip(transform.position);
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

    public virtual Vector3 PopPosition(MapManager map) {
        return map.GetFreeRoundedLocation();
    }
}

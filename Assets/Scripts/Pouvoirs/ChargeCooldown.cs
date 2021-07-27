﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ChargeCooldown : Cooldown {

    public int maxCharges = 1;
    public bool dontStartFull = false;
    [ConditionalHide("dontStartFull")]
    public int startCharges = 0;

    protected int currentCharges;
    protected Coroutine chargingCoroutine = null;

    public override void Initialize() {
        base.Initialize();
        currentCharges = dontStartFull ? startCharges : maxCharges;
    }

    public override void Use() {
        if(currentCharges <= 0) {
            Debug.LogError($"Tentative d'utiliser une ChargeCooldown sans charges sur le pouvoir {pouvoir.name} !", pouvoir); 
        }
        currentCharges--;
        StartCharging();
    }

    protected void StartCharging() {
        if(chargingCoroutine == null) {
            chargingCoroutine = StartCoroutine(CStartChargingCoroutine());
        }
    }

    protected IEnumerator CStartChargingCoroutine() {
        cooldownTimer.Reset();
        yield return new WaitForSeconds(cooldown);
        chargingCoroutine = null;
        currentCharges++;
        if(currentCharges < maxCharges) {
            StartCharging();
        }
    }

    public override bool IsAvailable() {
        return currentCharges > 0;
    }
}

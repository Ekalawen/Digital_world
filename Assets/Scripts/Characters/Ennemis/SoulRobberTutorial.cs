using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class SoulRobberTutorial : SoulRobber {

    [Header("Give Pouvoir Back")]
    public GameObject pouvoirPrefab;
    public PouvoirGiverItem.PouvoirBinding pouvoirBinding;

    [Header("Matrix Regeneration")]
    public GameObject matrixRegenerationEventPrefab;
    public GameObject firstMatrixRegenerationEventPrefab;

    [Header("Renforts")]
    public GameObject soulRobbersRenfortsEventPrefab;
    public GameObject tracerRenfortEventPrefab;

    [Header("PhaseSwap")]
    public GameObject goToPhase1EventPrefab;
    public GameObject goToPhase2EventPrefab;

    [Header("Time")]

    protected bool hasAlreadyStartEscaping = false;
    protected bool hasAlreadyStopEscaping = false;

    public override void StartEscaping() {
        base.StartEscaping();
        if(!hasAlreadyStartEscaping) {
            hasAlreadyStartEscaping = true;
            PouvoirGiverItem.GivePouvoir(gm, pouvoirPrefab, pouvoirBinding);
            StartTimer();
            gm.eventManager.StartSingleEvent(matrixRegenerationEventPrefab);
            gm.eventManager.StartSingleEvent(firstMatrixRegenerationEventPrefab);
            gm.eventManager.StartSingleEvent(goToPhase1EventPrefab);
            gm.eventManager.StartSingleEvent(goToPhase2EventPrefab);
        }
    }

    public override void StopEscaping() {
        base.StopEscaping();
        if(!hasAlreadyStopEscaping) {
            hasAlreadyStopEscaping = true;
            gm.eventManager.StartSingleEvent(soulRobbersRenfortsEventPrefab);
            gm.eventManager.StartSingleEvent(tracerRenfortEventPrefab);
        }
    }

    protected void StartTimer() {
        gm.timerManager.isInfinitTime = false;
        gm.timerManager.SetTime(gm.timerManager.initialTime, showVolatileText: false);
    }
}

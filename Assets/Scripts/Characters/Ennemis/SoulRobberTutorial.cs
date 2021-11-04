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

    [Header("Renforts")]
    public GameObject soulRobbersRenfortsEventPrefab;
    public GameObject tracerRenfortEventPrefab;

    protected bool hasAlreadyStartEscaping = false;
    protected bool hasAlreadyStopEscaping = false;

    public override void StartEscaping() {
        base.StartEscaping();
        if(!hasAlreadyStartEscaping) {
            hasAlreadyStartEscaping = true;
            PouvoirGiverItem.GivePouvoir(gm, pouvoirPrefab, pouvoirBinding);
            gm.soundManager.PlayPouvoirAvailableClip();
        }
    }

    public override void StopEscaping() {
        base.StopEscaping();
        if(!hasAlreadyStopEscaping) {
            hasAlreadyStopEscaping = true;
            gm.eventManager.StartSingleEvent(matrixRegenerationEventPrefab);
            gm.eventManager.StartSingleEvent(soulRobbersRenfortsEventPrefab);
            gm.eventManager.StartSingleEvent(tracerRenfortEventPrefab);
            // Déclencher le Timer
        }
    }
}

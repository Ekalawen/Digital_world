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
            // Déclencher la régénération de la Matrice
            // Ajouter un deuxième SR
            // Déclencher le Timer
        }
    }
}

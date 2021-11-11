using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventAddEnnemi : SingleEvent {

    public int ennemiIndice = 0;
    public int nbEnnemisToAdd = 1;

    [Header("Start Position")]
    public bool useRandomStartPosition = true;
    [ConditionalHide("useRandomStartPosition", false)]
    public Vector3 customStartPosition;

    [Header("Waiting Time")]
    public bool shouldUseCustomWaitingTime = false;
    [ConditionalHide("shouldUseCustomWaitingTime")]
    public float customWaitingTime = 1.0f;

    protected GameObject ennemiPrefab;

    public override void Initialize() {
        base.Initialize();
        ennemiPrefab = gm.ennemiManager.ennemisPrefabs[ennemiIndice];
    }

    public override void TriggerSpecific() {
        for(int i = 0; i < nbEnnemisToAdd; i++) {
            Ennemi ennemi;
            float waitingTime = shouldUseCustomWaitingTime ? customWaitingTime : -1;
            if(useRandomStartPosition) {
                ennemi = gm.ennemiManager.PopEnnemi(ennemiPrefab, waitingTime);
            } else {
                ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, customStartPosition, waitingTime);
            }
        }
    }
}

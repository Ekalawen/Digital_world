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
    public bool shouldUseWaitingTime = false;

    protected GameObject ennemiPrefab;

    public override void Initialize() {
        base.Initialize();
        ennemiPrefab = gm.ennemiManager.ennemisPrefabs[ennemiIndice];
    }

    public override void TriggerSpecific() {
        for(int i = 0; i < nbEnnemisToAdd; i++) {
            Ennemi ennemi;
            if(useRandomStartPosition) {
                ennemi = gm.ennemiManager.PopEnnemi(ennemiPrefab);
            } else {
                ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, customStartPosition);
            }
            if(!shouldUseWaitingTime) {
                IController controller = ennemi.GetComponent<IController>();
                if(controller != null) {
                    controller.SetWaitingTime(0);
                }
            }
        }
    }
}

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

    protected GameObject ennemiPrefab;

    public override void Start() {
        base.Start();
        ennemiPrefab = gm.ennemiManager.ennemisPrefabs[ennemiIndice];
    }

    public override void Trigger() {
        for(int i = 0; i < nbEnnemisToAdd; i++) {
            if(useRandomStartPosition) {
                gm.ennemiManager.PopEnnemi(ennemiPrefab);
            } else {
                gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, customStartPosition);
            }
        }
    }
}

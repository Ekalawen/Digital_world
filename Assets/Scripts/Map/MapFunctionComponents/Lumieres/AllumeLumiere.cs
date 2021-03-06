﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllumeLumiere : MonoBehaviour {

    public enum AllumageType { PROCHE, ELOIGNE }

    public AllumageType type = AllumageType.PROCHE;
    public bool shouldThrowLightningToNewLumiere = true;
    [ConditionalHide("shouldThrowLightningToNewLumiere")]
    public GameObject lightningPrefab;

    protected GameManager gm;
    protected MapManager map;

    protected void Start() {
        gm = GameManager.Instance;
        map = gm.map;
    }

    public void AllumeOneLumiere(Vector3 position) {
        List<Lumiere> lumieres = map.GetLumieres();
        foreach (Lumiere lumiere in lumieres) {
            LumiereSwitchable ls = (LumiereSwitchable)lumiere;
            ls.SetState(LumiereSwitchable.LumiereSwitchableState.OFF);
        }
        if (lumieres.Count <= 0)
            return;
        Lumiere chosenOne = GetChosenOne(position);
        LumiereSwitchable chosenOneSwitchable = (LumiereSwitchable)chosenOne;
        chosenOneSwitchable.SetState(LumiereSwitchable.LumiereSwitchableState.ON);
        chosenOneSwitchable.TriggerLightExplosion();

        ThrowLightning(position, chosenOneSwitchable.transform.position);
    }

    protected void ThrowLightning(Vector3 from, Vector3 to) {
        if (shouldThrowLightningToNewLumiere) {
            Lightning lightning = Instantiate(lightningPrefab, from, Quaternion.identity).GetComponent<Lightning>();
            lightning.Initialize(from, to);
        }
    }

    private Lumiere GetChosenOne(Vector3 position) {
        if(type == AllumageType.PROCHE)
            return map.GetLumieres().OrderBy(l => Vector3.Distance(l.transform.position, position)).First();
        else
            return map.GetLumieres().OrderBy(l => Vector3.Distance(l.transform.position, position)).Last();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SecondBossCollecteur : MonoBehaviour {

    public int phaseIndice = 2;
    protected bool hasCollected = false;

    public void Collect() {
        if (!hasCollected) {
            SecondBoss secondBoss = FindObjectOfType<SecondBoss>();
            if (secondBoss != null) {
                if (phaseIndice == 2) {
                    secondBoss.CollectGeneratorsOfPhase2();
                } else {
                    secondBoss.CollectGeneratorsOfPhase3();
                }
                hasCollected = true;
            }
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FirstBossCollecteur : MonoBehaviour {

    protected bool hasCollected = false;

    public void Collect() {
        if (!hasCollected) {
            FirstBoss firstBoss = FindObjectOfType<FirstBoss>();
            if (firstBoss != null) {
                firstBoss.CollectGeneratorsOfPhase2();
                hasCollected = true;
            }
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FirstBossCollecteur : MonoBehaviour {

    public void Collect() {
        FirstBoss firstBoss = FindObjectOfType<FirstBoss>();
        firstBoss.CollectGeneratorsOfPhase2();
    }

}

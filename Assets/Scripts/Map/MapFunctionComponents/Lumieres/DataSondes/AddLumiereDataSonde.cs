using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddLumiereDataSonde: GenerateLumieresMapFunction {

    public OrbTrigger orbTrigger;
    public GameObject lightningPrefab;

    public void Trigger() {
        Initialize();
        Activate();
    }

    public override void Activate() {
        CreateLumiereAtSondePosition();
        InitializeOtherDataSonde();
    }

    protected void CreateLumiereAtSondePosition() {
        Lumiere data = map.CreateLumiere(orbTrigger.transform.position, lumiereType, dontRoundPositions: true);
    }

    protected void InitializeOtherDataSonde() {
        DataSoundManager dataSoundManager = gm.ennemiManager.dataSoundManager;
        dataSoundManager.UnregisterDataSondeTrigger(orbTrigger);
        OrbTrigger newOrbTrigger = dataSoundManager.ActivateNextDataSonde(orbTrigger.transform.position);
        ThrowLightningToNextOne(newOrbTrigger);
    }

    protected void ThrowLightningToNextOne(OrbTrigger newOrbTrigger) {
        if (newOrbTrigger != null) {
            Lightning lightning = Instantiate(lightningPrefab).GetComponent<Lightning>();
            lightning.Initialize(orbTrigger.transform.position, newOrbTrigger.transform.position, Lightning.PivotType.EXTREMITY);
        }
    }
}

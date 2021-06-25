using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddLumiereDataSonde: GenerateLumieresMapFunction {

    public Ennemi ennemi;

    public override void Activate() {
        CreateLumiereAtSondePosition();
    }

    public void Trigger() {
        Initialize();
        Activate();
    }

    protected void CreateLumiereAtSondePosition() {
        Lumiere data = map.CreateLumiere(ennemi.transform.position, lumiereType, dontRoundPositions: true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereSwitchableParkourTraining : LumiereSwitchable {

    protected override void CapturedSpecific() {
        ParkourTrainingMap map = (ParkourTrainingMap)gm.map;
        map.AllumePlusProcheLumiere(transform.position);
    }
}

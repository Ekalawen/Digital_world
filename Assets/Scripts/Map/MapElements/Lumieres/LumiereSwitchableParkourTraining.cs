﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereSwitchableParkourTraining : LumiereSwitchable {

    protected override void CapturedSpecific() {
        gm.map.GetComponent<AllumePlusProcheLumiere>().AllumerPlusProcheLumiere(transform.position);
    }
}

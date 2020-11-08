using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereSwitchableLightOtherOne : LumiereSwitchable {

    protected override void CapturedSpecific() {
        gm.map.GetComponent<AllumeLumiere>().AllumeOneLumiere(transform.position);
    }
}

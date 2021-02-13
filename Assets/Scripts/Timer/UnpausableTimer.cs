using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnpausableTimer : Timer {

    public UnpausableTimer(float duree = 0) : base(duree) {
    }

    protected override float GetTimeSinceLevelLoad() {
        // On doit soustraire une date récente sinon le float n'est pas assez grand !
        TimeSpan timestamp = DateTime.UtcNow - DateTime.Today.AddDays(-1);
        float nbSecondes = (float)timestamp.TotalSeconds;
        return nbSecondes;
    }
}

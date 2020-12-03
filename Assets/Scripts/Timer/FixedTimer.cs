using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedTimer : Timer {

    public FixedTimer(float duree = 0) : base(duree) {
    }

    protected override float GetTimeSinceLevelLoad() {
        return Time.fixedTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RewardLumiereDisplayer : RewardObjectDisplayer {

    public override void Update() {
        if (obj == null)
            return;
        float lastTime = history.LastTime();
        float avancement = durationTimer.GetAvancement();
        if (avancement > 1.0f) {
            Destroy(obj.gameObject);
            obj = null;
        } else {
            obj.transform.position = curve.GetAvancement(avancement);
        }
    }
}

